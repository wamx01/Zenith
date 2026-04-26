# Despliegue de `Zenith` en VPS con Docker

## Índice

1. [Objetivo](#objetivo)
2. [Arquitectura actual](#arquitectura-actual)
3. [Archivos relevantes](#archivos-relevantes)
4. [Variables de entorno necesarias](#variables-de-entorno-necesarias)
5. [Preparación del VPS](#preparación-del-vps)
6. [Publicación inicial](#publicación-inicial)
7. [Primer acceso con SuperAdmin bootstrap](#primer-acceso-con-superadmin-bootstrap)
8. [Validaciones posteriores](#validaciones-posteriores)
9. [Mantenimiento operativo](#mantenimiento-operativo)
10. [Troubleshooting: nginx en restart loop](#troubleshooting-nginx-en-restart-loop)

## Objetivo

Esta guía deja documentado el despliegue actual de `Zenith` en un VPS Linux usando el stack ya existente del repositorio:

- `MundoVs` como app ASP.NET Core / Blazor Server
- `MariaDB` como base de datos
- `Nginx` como proxy inverso
- volumen persistente para base y adjuntos
- soporte para `WebSockets`
- soporte para `ForwardedHeaders`
- `bootstrap` opcional de usuario `SuperAdmin` por variables de entorno

## Arquitectura actual

```text
Internet
  |
  v
Nginx :80
  |
  v
MundoVs :5130
  |
  v
MariaDB :3306 (solo red interna Docker)
```

Persistencia:

- volumen `mariadb_data`
- volumen `mundovs_uploads`

Puertos públicos esperados:

- `80`
- `443` si después se agrega TLS en proxy externo o `certbot`

## Archivos relevantes

- `docker-compose.yml`
- `.env.example`
- `nginx/default.conf`
- `MundoVs/Dockerfile`
- `MundoVs/Program.cs`

## Variables de entorno necesarias

El archivo real de despliegue debe ser `.env` en la raíz del repositorio.

Base de datos:

- `MARIADB_DATABASE`
- `MARIADB_USER`
- `MARIADB_PASSWORD`
- `MARIADB_ROOT_PASSWORD`

Bootstrap de acceso inicial:

- `BOOTSTRAP_SUPERADMIN_EMAIL`
- `BOOTSTRAP_SUPERADMIN_PASSWORD`

### Ejemplo de `.env`

```dotenv
MARIADB_DATABASE=CrmMundoVs
MARIADB_USER=zenith_user
MARIADB_PASSWORD=cambia-esta-password-app
MARIADB_ROOT_PASSWORD=cambia-esta-password-root

BOOTSTRAP_SUPERADMIN_EMAIL=superadmin@zenitherp.tech
BOOTSTRAP_SUPERADMIN_PASSWORD=CambiaEstaPassword!2026
```

### Qué hace cada variable de bootstrap

- si ambas variables tienen valor, la app crea o resetea al arrancar ese usuario con rol `SuperAdmin`
- si alguna está vacía, el bootstrap no corre
- el acceso sigue pasando por login normal; no es un bypass fuera del sistema
- el usuario queda activo, desbloqueado y con contraseña conocida desde entorno

## Preparación del VPS

Checklist mínimo:

- Ubuntu LTS actualizado
- Docker instalado
- Docker Compose disponible
- DNS del dominio apuntando al VPS
- puertos abiertos: `22`, `80` y `443`

Comandos base:

```powershell
sudo apt update
sudo apt upgrade -y
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
sudo mkdir -p /opt/Zenith
cd /opt/Zenith
```

## Publicación inicial

### 1. Copiar el repositorio al VPS

```powershell
cd /opt/Zenith
git clone <URL-DEL-REPO> .
```

### 2. Crear archivo `.env`

```powershell
cp .env.example .env
nano .env
```

### 3. Revisar `nginx/default.conf`

El archivo apunta al upstream `http://mundovs:5130` usando el nombre de servicio Docker Compose.
El `resolver 127.0.0.11` ya está configurado para resolver nombres por petición en lugar de al arranque,
lo que evita errores `host not found in upstream` en Dokploy.

Si se requiere editarlo:

```powershell
nano nginx/default.conf
```

### 4. Construir y levantar el stack

```powershell
docker compose build
docker compose up -d
docker compose ps
```

### 5. Revisar logs

```powershell
docker compose logs --tail=100 mariadb
docker compose logs --tail=100 mundovs
docker compose logs --tail=100 nginx
```

## Primer acceso con SuperAdmin bootstrap

Para garantizar el primer acceso, define en `.env`:

```dotenv
BOOTSTRAP_SUPERADMIN_EMAIL=superadmin@zenitherp.tech
BOOTSTRAP_SUPERADMIN_PASSWORD=CambiaEstaPassword!2026
```

Luego recrea la app:

```powershell
docker compose up -d --build mundovs
docker compose logs --tail=100 mundovs
```

En el log debe aparecer un mensaje similar a:

- `Bootstrap SuperAdmin: se creó el usuario ...`
- `Bootstrap SuperAdmin: se reseteó el usuario ...`

Después del primer acceso exitoso, se recomienda quitar esas variables del `.env` para que la contraseña no se reescriba en cada reinicio o despliegue.

Aplicar el cambio:

```powershell
nano .env
docker compose up -d mundovs
```

## Validaciones posteriores

Validar desde el VPS:

```powershell
curl http://127.0.0.1/health/live
curl http://127.0.0.1/health/ready
```

Validar funcionalmente:

- abrir `http://tu-dominio`
- confirmar que carga la app Blazor Server
- iniciar sesión con el `SuperAdmin` bootstrap o con el usuario sembrado
- validar creación de adjuntos
- validar que los contenedores reinicien sin pérdida de datos

## Mantenimiento operativo

Actualizar aplicación:

```powershell
cd /opt/Zenith
git pull
docker compose build
docker compose up -d
```

Revisar estado:

```powershell
docker compose ps
docker compose logs --tail=100 mundovs
```

Respaldos mínimos:

- volumen `mariadb_data`
- volumen `mundovs_uploads`
- archivo `.env`
- archivo `nginx/default.conf`

Capacidad sugerida:

- `KVM 2` puede servir para arranque ligero
- si suben PDFs, concurrencia o uso de RRHH/Nómina, vigilar RAM y CPU
- si aparece swapping o lentitud sostenida, subir a `KVM 4`

## Troubleshooting: nginx en restart loop

### Síntoma

```
[emerg] 1#1: host not found in upstream "mundovs" in /etc/nginx/conf.d/default.conf:12
```

El contenedor `nginx` reinicia continuamente con `ExitCode: 1`.

### Causa raíz

El nginx estándar resuelve los nombres de upstream **en el momento de arranque** (al cargar la
configuración). Si en ese instante el servicio `mundovs` todavía no está registrado en el DNS interno
de Docker (o si Dokploy reinicia nginx de forma independiente del servicio app), nginx falla con
`[emerg]` y entra en restart loop.

### Solución implementada

`nginx/default.conf` ahora incluye:

```nginx
resolver 127.0.0.11 valid=30s ipv6=off;
```

Y cada `proxy_pass` usa una variable en lugar de la URL literal:

```nginx
set $backend http://mundovs:5130;
proxy_pass $backend;
```

Esto hace que nginx resuelva el hostname por petición usando el DNS interno de Docker
(`127.0.0.11`), en lugar de resolverlo una sola vez al arrancar. Si el servicio app aún no está
disponible al iniciar nginx, nginx arranca correctamente de todos modos y sólo devolverá un `502`
temporal hasta que el backend esté listo.

`docker-compose.yml` eliminó los `container_name` explícitos de todos los servicios, de modo que
el nombre resolvible en la red Docker (`mundovs`) coincide exactamente con el nombre de servicio
declarado en el compose, sin ambigüedad.

### Validación post-deploy

```powershell
# 1. Todos los contenedores deben estar en estado "running" (no "restarting")
docker compose ps

# 2. nginx debe levantar sin errores
docker compose logs --tail=50 nginx

# 3. Health checks deben responder 200
curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1/health/live
curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1/health/ready

# 4. La app debe cargar desde el dominio público
curl -I http://tu-dominio
```

Resultado esperado:
- `docker compose ps` → todos los servicios en `running`
- `/health/live` → `200`
- `/health/ready` → `200` (indica que la app y la DB están listos)
