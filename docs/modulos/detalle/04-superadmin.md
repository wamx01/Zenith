# Módulo `SuperAdmin`

## Objetivo
Este módulo administra la plataforma a nivel global: empresas, suscripciones, capacidades y elementos transversales de tenant.

## Páginas incluidas
- `MundoVs/Components/Pages/SuperAdmin/Empresas.razor`
- `MundoVs/Components/Pages/SuperAdmin/Capacidades.razor`
- `MundoVs/Components/Pages/Empresas.razor` (actualmente vacío)

## Qué información maneja
- catálogo global de empresas,
- planes comerciales,
- suscripciones activas o trial,
- pagos de suscripción,
- logo por tenant,
- capacidades/permisos globales,
- creación de admin inicial.

## Fuentes técnicas principales
### `SuperAdmin/Empresas.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `IAuthService`
- `IAuditService`
- `ITenantFileStorageService`
- `AuthenticationStateProvider`

Consulta directamente:
- `Empresas`
- `Planes`
- `SuscripcionesEmpresa`
- `PagosSuscripcion`
- probablemente `Usuarios` cuando crea admin inicial

### `SuperAdmin/Capacidades.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `IAuditService`
- `AuthenticationStateProvider`

Consulta directamente:
- `Capacidades`

## Entidades y tablas relacionadas
Ver en `CrmDbContext.cs`:
- `Empresas`
- `Planes`
- `SuscripcionesEmpresa`
- `PagosSuscripcion`
- `Usuarios`
- `Capacidades`
- `AuditLogs`

## Dónde sale cada dato
### Datos generales de empresa
- origen principal: `Empresa`
- campos relevantes: código, razón social, RFC, estado, trial, activo, suspendido, plan actual, máximo de usuarios.

### Plan disponible para una empresa
- origen principal: `Plan`
- asociación: `Empresa.PlanActualId` y/o `SuscripcionEmpresa.PlanId`

### Estado de suscripción actual
- origen principal: `SuscripcionEmpresa`
- se toma normalmente la suscripción más reciente por empresa.

### Pagos de suscripción
- origen principal: `PagoSuscripcion`
- asociados a `SuscripcionEmpresa`

### Logo del tenant
- origen principal funcional: `ITenantFileStorageService`
- referencia de negocio: datos de la empresa actual.

### Catálogo de permisos globales
- origen principal: `Capacidad`
- esta tabla alimenta la asignación posterior en `TiposUsuario`.

## Preguntas futuras y dónde buscar
### ¿Dónde se crea o edita una empresa?
Buscar en:
1. `SuperAdmin/Empresas.razor`
2. entidad `Empresa`
3. `CrmDbContext.Empresas`

### ¿Dónde se define el plan o trial de una empresa?
Buscar en:
1. `SuperAdmin/Empresas.razor`
2. `Plan`
3. `SuscripcionEmpresa`

### ¿Dónde se registran pagos de suscripción?
Buscar en:
1. `SuperAdmin/Empresas.razor`
2. entidad `PagoSuscripcion`
3. `CrmDbContext.PagosSuscripcion`

### ¿De dónde salen los permisos globales del sistema?
Buscar en:
1. `SuperAdmin/Capacidades.razor`
2. entidad `Capacidad`

## Dónde buscar primero
- `MundoVs/Components/Pages/SuperAdmin/Empresas.razor`
- `MundoVs/Components/Pages/SuperAdmin/Capacidades.razor`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
- `MundoVs/Core/Interfaces/ITenantFileStorageService.cs`
- `MundoVs/Core/Interfaces/IAuthService.cs`
