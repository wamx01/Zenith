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

Debe apuntar al dominio real y al upstream `http://mundovs:5130`.

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
