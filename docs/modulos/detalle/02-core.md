# Módulo `Core`

## Objetivo
Este módulo concentra páginas base del sistema: inicio, errores y componentes de plantilla.

## Páginas incluidas
- `MundoVs/Components/Pages/Core/Home.razor`
- `MundoVs/Components/Pages/Core/Error.razor`
- `MundoVs/Components/Pages/Core/NotFound.razor`
- `MundoVs/Components/Pages/Core/Counter.razor`
- `MundoVs/Components/Pages/Core/Weather.razor`

## Qué información muestra
- métricas generales del sistema,
- accesos rápidos según capacidades,
- datos de empresa visibles en home,
- conteos para dashboard de `SuperAdmin`,
- información de errores y `RequestId`.

## Fuentes técnicas principales
### `Home.razor`
Usa estas fuentes:
- `IClienteRepository`
- `IProductoRepository`
- `IAppConfigRepository`
- `IDbContextFactory<CrmDbContext>`
- `ModuloStateService`

Esto indica que la pantalla mezcla:
- datos de negocio obtenidos por repositorio,
- configuración obtenida por `AppConfig`,
- conteos y consultas directas con `CrmDbContext`,
- estado visual de módulos activos mediante `ModuloStateService`.

### `Error.razor`
- toma el `RequestId` desde `Activity.Current` o `HttpContext.TraceIdentifier`.

### `Counter.razor` y `Weather.razor`
- son páginas de ejemplo,
- no deben tomarse como fuente de lógica funcional del negocio.

## Entidades y tablas relacionadas
En `CrmDbContext.cs` las más relevantes para `Home` son:
- `Clientes`
- `Productos`
- `Empresas`
- `Usuarios`
- `Capacidades`
- `Planes`
- `SuscripcionesEmpresa`
- `AppConfigs`

## Dónde sale cada dato
### KPIs de usuarios normales
Normalmente salen de:
- `Clientes` mediante `IClienteRepository`
- `Productos` mediante `IProductoRepository`
- configuración general mediante `IAppConfigRepository`

### KPIs de `SuperAdmin`
Normalmente salen de consultas directas a:
- `Empresas`
- `Usuarios`
- `Capacidades`
- `Planes`
- `SuscripcionesEmpresa`

### Accesos rápidos visibles
- no salen de una tabla específica,
- dependen de capacidades del usuario y del estado de módulos activos.

### Módulos activos
- fuente visual: `ModuloStateService`
- la activación funcional suele estar ligada a configuración almacenada en `AppConfig` y cargada por otras pantallas.

## Preguntas futuras y dónde buscar
### ¿De dónde salen los totales del home?
Buscar en:
1. `Components/Pages/Core/Home.razor`
2. repositorios inyectados en esa página,
3. consultas directas a `CrmDbContext` dentro de la misma pantalla.

### ¿Por qué un acceso rápido no aparece?
Buscar en:
1. capacidades del usuario,
2. condiciones `Tiene(...)` dentro de `Home.razor`,
3. estado de módulos activos.

### ¿De dónde sale el `RequestId` de error?
Buscar en:
1. `Core/Error.razor`
2. `Activity.Current`
3. `HttpContext.TraceIdentifier`

## Dónde buscar primero
- `MundoVs/Components/Pages/Core/Home.razor`
- `MundoVs/Core/Services/ModuloStateService.cs`
- `MundoVs/Core/Interfaces/IClienteRepository.cs`
- `MundoVs/Core/Interfaces/IProductoRepository.cs`
- `MundoVs/Core/Interfaces/IAppConfigRepository.cs`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
