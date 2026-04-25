# Módulo `Admin`

## Objetivo
Este módulo administra configuración operativa del tenant, catálogos generales y seguridad interna por empresa.

## Páginas incluidas
- `MundoVs/Components/Pages/Admin/Configuracion.razor`
- `MundoVs/Components/Pages/Admin/ConfiguracionNomina.razor`
- `MundoVs/Components/Pages/Admin/TiposInventario.razor`
- `MundoVs/Components/Pages/Admin/TiposUsuario.razor`
- `MundoVs/Components/Pages/Admin/Usuarios.razor`

## Qué información maneja
- activación de módulos,
- configuración de nómina,
- tipos de inventario por categoría,
- roles por empresa,
- permisos por rol,
- usuarios del tenant,
- consecutivos y parámetros guardados en `AppConfig`.

## Fuentes técnicas principales
### `Configuracion.razor`
- usa `CustomAuthStateProvider`
- usa `IWebHostEnvironment`
- consume el estado de módulos y accesos a catálogos administrativos.

### `ConfiguracionNomina.razor`
- usa `IAppConfigRepository`
- guarda y lee claves de configuración de nómina desde `AppConfigs`.

### `TiposInventario.razor`
- usa `IDbContextFactory<CrmDbContext>`
- consulta directamente:
  - `CategoriasInventario`
  - `TiposInventario`
- filtra por `EmpresaId` obtenido desde claims.

### `TiposUsuario.razor`
- usa `CrmDbContext`
- usa `IAuditService`
- consulta:
  - `TiposUsuario`
  - `Capacidades`
  - `TipoUsuarioCapacidades`
- separa capacidades visibles según si el usuario es `SuperAdmin`.

### `Usuarios.razor`
- usa `CrmDbContext`
- usa `IAuthService`
- usa `IAuditService`
- consulta:
  - `Usuarios`
  - `TiposUsuario`
  - `Empresas`
  - `PlanActual`
- valida límite de usuarios por empresa o plan activo.

## Entidades y tablas relacionadas
Ver en `CrmDbContext.cs`:
- `AppConfigs`
- `CategoriasInventario`
- `TiposInventario`
- `Usuarios`
- `TiposUsuario`
- `Capacidades`
- `TipoUsuarioCapacidades`
- `Empresas`
- `Planes`
- `AuditLogs`

## Dónde sale cada dato
### Configuración de nómina
- origen principal: `AppConfigs`
- acceso técnico: `IAppConfigRepository`
- claves esperadas: configuraciones de días base, horas base y factor de horas extra.

### Tipos de inventario
- origen principal:
  - `CategoriaInventario`
  - `TipoInventario`
- consulta directa por empresa desde `CrmDbContext`.

### Roles y permisos
- origen principal:
  - `TipoUsuario`
  - `Capacidad`
  - tabla relacional `TipoUsuarioCapacidad`

### Usuarios y su perfil
- origen principal: `Usuario`
- datos complementarios:
  - `TipoUsuario`
  - `Empresa`
  - `PlanActual`

### Límite de usuarios permitidos
- origen principal:
  - `Empresa.MaxUsuarios`
  - o `Empresa.PlanActual.LimiteUsuarios`

## Preguntas futuras y dónde buscar
### ¿Dónde se guardan los parámetros de nómina?
Buscar en:
1. `Admin/ConfiguracionNomina.razor`
2. `IAppConfigRepository`
3. `AppConfigRepository`
4. tabla `AppConfigs`

### ¿De dónde salen los permisos asignables a los roles?
Buscar en:
1. `Admin/TiposUsuario.razor`
2. `Capacidades`
3. `TipoUsuarioCapacidades`

### ¿Dónde se valida el límite de usuarios?
Buscar en:
1. `Admin/Usuarios.razor`
2. entidad `Empresa`
3. entidad `Plan`

### ¿De dónde salen las categorías y tipos de inventario?
Buscar en:
1. `Admin/TiposInventario.razor`
2. `CategoriasInventario`
3. `TiposInventario`

## Dónde buscar primero
- `MundoVs/Components/Pages/Admin/`
- `MundoVs/Infrastructure/Repositories/AppConfigRepository.cs`
- `MundoVs/Core/Interfaces/IAppConfigRepository.cs`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
- `MundoVs/Core/Services/CustomAuthStateProvider.cs`
