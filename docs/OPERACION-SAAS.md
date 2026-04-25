# Operación SaaS de `MundoVs`

## Objetivo
Dejar lineamientos mínimos para operar `MundoVs` en staging/producción.

## Variables de entorno
ASP.NET Core ya permite sobreescribir configuración por variables de entorno.

### Recomendadas
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__DefaultConnection=<cadena de conexión real>`
- `ASPNETCORE_URLS=http://+:8080`

## Secretos fuera del repo
No guardar secretos reales en `appsettings.json` del repo.

### Desarrollo
Usar:
- `appsettings.Development.json`
- Secret Manager si aplica

### Producción
Usar:
- variables de entorno
- secretos del proveedor de hosting
- `.env` fuera del repo si se usa Docker Compose

## Logging
El proyecto ya expone logging base por:
- consola
- debug
- HTTP logging de request/response básicos

## Monitoreo y health checks
Endpoints disponibles:
- `GET /health/live`
- `GET /health/ready`

`/health/ready` valida conexión a base de datos.

## Manejo centralizado de errores
Producción usa:
- `UseExceptionHandler("/Error")`
- página central `Components/Pages/Core/Error.razor`

## Backups
### Base de datos
Recomendado mínimo:
- backup diario completo
- retención de 7 a 30 días
- prueba periódica de restauración

Ejemplo MariaDB/MySQL:
- `mysqldump --single-transaction --routines --triggers <db> > backup.sql`

### Uploads
Respaldar también:
- `wwwroot/uploads`

Porque ahí viven logos y futuros documentos por tenant.

## Restauración
### Base de datos
1. crear base vacía
2. restaurar dump SQL
3. aplicar migraciones pendientes si existen
4. validar `/health/ready`

### Archivos
1. restaurar carpeta `wwwroot/uploads`
2. verificar que las rutas en `AppConfig` sigan apuntando a archivos existentes

## Checklist de producción
- cadena de conexión fuera del repo
- HTTPS habilitado
- backups automáticos
- monitoreo consumiendo `/health/ready`
- logs persistidos por plataforma o contenedor
- volumen persistente para `wwwroot/uploads`
