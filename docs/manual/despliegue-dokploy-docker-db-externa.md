# Despliegue de `Zenith` en `Dokploy` con `Docker` y `MariaDB` externa

## Objetivo

Esta guía deja listo el despliegue de `Zenith` en `Dokploy` usando:

- `MundoVs/Dockerfile`
- `MariaDB` fuera del contenedor
- adjuntos persistentes en volumen del `VPS`
- `Dokploy` como proxy y terminación TLS

## Arquitectura recomendada

```text
Internet
  |
  v
Dokploy / Reverse Proxy :443
  |
  v
MundoVs :8080
  |
  +--> /app/wwwroot/uploads (volumen persistente)
  |
  v
MariaDB externa :3306
```

## Archivos relevantes

- `MundoVs/Dockerfile`
- `MundoVs/Program.cs`
- `.env.dokploy.example`
- `MundoVs/appsettings.json`

## Variables de entorno en `Dokploy`

Usa como base el archivo `.env.dokploy.example`.

Variables mínimas:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_HTTP_PORTS=8080` para no sobreescribir `HTTPS_PORTS`
- `ConnectionStrings__ZenithConnection=Server=...;Port=3306;Database=...;User=...;Password=...;CharSet=utf8mb4;`
- `Database__ApplyMigrationsOnStartup=true`
- `Auth__CookieSecurePolicy=Always`
- `Auth__SameSite=Lax`
- `Auth__UseHttpsRedirection=true`

Opcionales:

- `Bootstrap__SuperAdmin__Email`
- `Bootstrap__SuperAdmin__Password`
- `Asistencia__WorkerApiKey`

## Configuración en `Dokploy`

### Aplicación

- tipo: `Dockerfile`
- ruta del `Dockerfile`: `MundoVs/Dockerfile`
- puerto interno: `8080`
- rama: la rama a publicar

### Volumen para adjuntos

Crear un volumen persistente y montarlo en:

- `/app/wwwroot/uploads`

Sin este volumen, los adjuntos se perderán cuando `Dokploy` recree el contenedor.

### Dominio y TLS

- asignar el dominio público de `Zenith`
- usar el dominio raíz en `TRAEFIK_DOMAIN`; el compose también enruta `www.<dominio>`
- activar `SSL`
- mantener `UseHttpsRedirection=true` para respetar `X-Forwarded-Proto`

## Base de datos externa

La base debe aceptar conexiones desde el `VPS` donde corre `Dokploy`.

Recomendaciones:

- crear usuario exclusivo para la app
- limitar acceso por IP
- no exponer acceso abierto a internet
- respaldar la base por separado del contenedor

## Migraciones

`Zenith` ahora soporta controlar migraciones con:

- `Database__ApplyMigrationsOnStartup`

Uso sugerido:

- primer despliegue: `true`
- operación estable posterior: `false` si quieres ejecutar migraciones de forma controlada

## Checklist de validación

1. abrir `/health/live`
2. abrir `/health/ready`
3. iniciar sesión
4. subir un adjunto
5. recrear el despliegue en `Dokploy`
6. confirmar que el adjunto sigue disponible
7. validar creación o acceso del `SuperAdmin` bootstrap si fue configurado

## Integración con `ZkTecoApi`

Si se usará el agente interno del cliente:

- `ZkTecoApi` debe apuntar a la URL pública de `MundoVs`
- la autenticación usa el header `X-Zenith-Worker-Key`
- puede configurarse con `Asistencia__WorkerApiKey` o con agentes específicos en `Asistencia:Agentes`

Endpoints relevantes en `MundoVs`:

- `/api/rrhh/agentes/configuracion`
- `/api/rrhh/marcaciones/sync`
- `/api/rrhh/agentes/heartbeat`
- `/api/rrhh/agentes/logs`
