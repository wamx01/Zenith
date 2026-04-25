# Despliegue de `MundoVs` en VPS con Docker

## 1. Objetivo

Publicar `MundoVs` en un VPS Linux usando Docker con el siguiente stack:

- `MundoVs` como contenedor ASP.NET Core
- `Nginx` como proxy inverso
- `MariaDB` en el mismo VPS
- volúmenes persistentes para base de datos y adjuntos

Este plan está pensado para arrancar en un VPS `Hostinger KVM 2` con una carga inicial aproximada de 10 a 20 usuarios concurrentes ligeros.

## 2. Estado actual del proyecto

Actualmente el proyecto ya tiene:

- `TargetFramework` `net10.0`
- cadena de conexión `ConnectionStrings:ZenithConnection`
- puerto HTTP configurable por `Hosting:Urls`
- valor actual por defecto `http://*:5130`
- endpoints de salud:
  - `/health/live`
  - `/health/ready`
- almacenamiento local de archivos en `wwwroot/uploads` cuando no existe una ruta configurada en base de datos
- autenticación por cookies
- `Blazor Server`, por lo que el proxy debe soportar `WebSockets`

Actualmente el proyecto no tiene:

- `Dockerfile`
- `.dockerignore`
- `docker-compose.yml`
- configuración de `Nginx`
- archivo de variables de entorno de despliegue
- soporte explícito para `ForwardedHeaders`

## 3. Arquitectura objetivo

```text
Internet
  |
  v
Nginx :80/:443
  |
  v
MundoVs :5130
  |
  v
MariaDB :3306 (red interna Docker)
```

Persistencia requerida:

- volumen para `MariaDB`
- volumen para `MundoVs/wwwroot/uploads`

## 4. Archivos a crear o modificar

### Nuevos archivos

- `docs/manual/despliegue-vps-docker.md`
- `MundoVs/Dockerfile`
- `.dockerignore`
- `docker-compose.yml`
- `.env.example`
- `nginx/default.conf`

### Archivos a modificar

- `MundoVs/Program.cs`

## 5. Orden de ejecución

### Paso 1. Preparar la app para ejecutarse detrás de proxy inverso

Objetivo:

- permitir que `MundoVs` respete `X-Forwarded-For` y `X-Forwarded-Proto`
- evitar problemas de cookies, redirecciones y esquema HTTPS detrás de `Nginx`

Cambios esperados:

- agregar `UseForwardedHeaders`
- configurar `ForwardedHeadersOptions`
- colocarlo antes de autenticación, `UseHttpsRedirection` y demás middlewares sensibles al esquema

Validación:

- compilar correctamente
- mantener funcionando los endpoints de salud

### Paso 2. Crear la imagen Docker de `MundoVs`

Objetivo:

- construir una imagen de producción multi-stage
- publicar la app sobre la imagen `aspnet`

Archivo:

- `MundoVs/Dockerfile`

Debe incluir:

- `sdk:10.0` para restaurar, compilar y publicar
- `aspnet:10.0` para ejecutar
- `EXPOSE 5130`
- `ENTRYPOINT ["dotnet", "MundoVs.dll"]`

Validación:

- `docker build` exitoso

### Paso 3. Crear exclusiones para el contexto Docker

Objetivo:

- evitar enviar archivos innecesarios al contexto de build
- reducir tiempo y tamaño del build

Archivo:

- `.dockerignore`

Debe excluir al menos:

- `bin/`
- `obj/`
- `.vs/`
- `.git/`
- `node_modules/`

### Paso 4. Crear el stack de despliegue

Objetivo:

- levantar app, proxy y base de datos con un solo comando

Archivo:

- `docker-compose.yml`

Servicios esperados:

#### `mariadb`

- imagen `mariadb`
- variables para base, usuario y password
- volumen persistente
- `healthcheck`
- sin exponer `3306` al público salvo que se necesite administración externa

#### `mundovs`

- build desde `MundoVs/Dockerfile`
- variables de entorno:
  - `ASPNETCORE_ENVIRONMENT=Production`
  - `Hosting__Urls=http://+:5130`
  - `ConnectionStrings__ZenithConnection=...`
  - `Auth__CookieSecurePolicy=Always`
  - `Auth__SameSite=Lax`
  - `Auth__UseHttpsRedirection=false`
- volumen persistente para `/app/wwwroot/uploads`
- dependencia de `mariadb`
- `healthcheck` apuntando a `/health/ready`

#### `nginx`

- imagen `nginx`
- puerto `80:80`
- volumen con configuración
- dependencia de `mundovs`

### Paso 5. Crear la configuración de `Nginx`

Objetivo:

- publicar la app por dominio
- soportar `Blazor Server`
- pasar encabezados correctos al backend

Archivo:

- `nginx/default.conf`

Debe incluir:

- `proxy_pass http://mundovs:5130`
- `proxy_http_version 1.1`
- encabezados `Upgrade` y `Connection`
- encabezados `X-Forwarded-For`, `X-Forwarded-Proto`, `Host`
- `location /health/ready` y `location /health/live`
- timeouts razonables

### Paso 6. Crear plantilla de variables de entorno

Objetivo:

- dejar claro qué valores se deben configurar en producción

Archivo:

- `.env.example`

Variables esperadas:

- `MYSQL_DATABASE`
- `MYSQL_USER`
- `MYSQL_PASSWORD`
- `MYSQL_ROOT_PASSWORD`

### Paso 7. Levantar el VPS

Checklist del VPS:

- Ubuntu LTS actualizado
- Docker instalado
- Docker Compose disponible
- puertos abiertos:
  - `22`
  - `80`
  - `443`
- dominio apuntando al VPS

### Paso 8. Publicar el stack

Orden recomendado:

1. copiar archivos al VPS
2. crear archivo `.env` real a partir de `.env.example`
3. levantar el stack
4. revisar logs
5. verificar salud

Comandos esperados:

```powershell
copy .env.example .env
```

```powershell
docker compose build
```

```powershell
docker compose up -d
```

```powershell
docker compose ps
```

```powershell
docker compose logs -f mundovs
```

```powershell
docker compose logs -f nginx
```

```powershell
docker compose logs -f mariadb
```

### Paso 9. Aplicar migraciones

Como el proyecto no tiene una migración automática explícita al arranque, se debe validar cuidadosamente el estado de la base.

Opción recomendada:

- ejecutar migraciones de EF Core antes de abrir tráfico productivo

Validación mínima:

- la app arranca
- la conexión a `MariaDB` responde
- `/health/ready` devuelve estado saludable

### Paso 10. Validar operación

Pruebas mínimas:

- abrir la app por dominio
- validar login
- validar navegación principal
- validar carga de componentes Blazor Server
- validar que no fallen las conexiones persistentes
- validar creación y lectura de archivos adjuntos
- validar reinicio de contenedores sin pérdida de datos

### Paso 11. Respaldos mínimos

Respaldar:

- volumen de `MariaDB`
- volumen de `uploads`
- archivo `.env`
- configuración de `Nginx`

## 6. Riesgos del VPS `KVM 2`

Con `MundoVs + Nginx + MariaDB` en el mismo VPS, `KVM 2` es viable como arranque, pero con poco margen.

Escenario razonable:

- 10 a 20 usuarios concurrentes ligeros

Escenario riesgoso:

- muchos reportes o PDFs simultáneos
- consultas pesadas de BD
- crecimiento rápido de adjuntos
- más servicios en el mismo VPS

Señales para subir a `KVM 4`:

- uso de RAM sostenido alto
- swapping
- latencia visible en la UI
- CPU alta en horas pico
- lentitud en `MariaDB`

## 7. Resultado esperado al terminar esta implementación

Al concluir los cambios del repositorio, debe quedar listo lo siguiente:

- aplicación compatible con proxy inverso
- imagen Docker de producción
- stack `docker compose`
- proxy `Nginx` listo para `Blazor Server`
- variables de entorno documentadas
- guía operativa para llevarlo al VPS
