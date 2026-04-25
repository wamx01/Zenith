# Módulo `Auth`

## Objetivo
Este módulo controla el acceso al sistema: inicio de sesión, recuperación de contraseña, restablecimiento y cambio obligatorio de contraseña inicial.

## Páginas incluidas
- `MundoVs/Components/Pages/Auth/Login.razor`
- `MundoVs/Components/Pages/Auth/RecuperarPassword.razor`
- `MundoVs/Components/Pages/Auth/RestablecerPassword.razor`
- `MundoVs/Components/Pages/Auth/CambiarPasswordInicial.razor`
- `MundoVs/Components/Pages/Login.razor` (actualmente vacío)

## Qué información maneja
- credenciales del usuario,
- identidad autenticada,
- empresa activa del usuario,
- capacidades/permisos cargados como claims,
- estado `RequirePasswordChange`,
- token temporal de recuperación,
- validez operativa de la empresa y su suscripción.

## Fuentes técnicas principales
### Páginas
- `Auth/Login.razor`
  - usa `CustomAuthStateProvider` para iniciar sesión,
  - redirige según resultado del login y el estado del usuario.
- `Auth/RecuperarPassword.razor`
  - usa `IAuthService.GenerarTokenRecuperacionAsync`.
- `Auth/RestablecerPassword.razor`
  - usa `IAuthService.RestablecerPasswordAsync`.
- `Auth/CambiarPasswordInicial.razor`
  - usa `AuthenticationStateProvider` para leer claims del usuario,
  - usa `IAuthService.CambiarPasswordInicialAsync`.

### Servicios e interfaces
- `MundoVs/Core/Interfaces/IAuthService.cs`
  - contrato de login, cambio de contraseña, recuperación y validación.
- `MundoVs/Core/Services/CustomAuthStateProvider.cs`
  - restaura sesión,
  - valida si el usuario sigue activo,
  - carga `EmpresaId`, rol y capacidades como claims,
  - verifica si la empresa puede operar según estado y suscripción.
- `MundoVs/wwwroot/js/auth-browser.js`
  - soporte del flujo navegador/servidor usado por el proveedor de autenticación.

### Entidades y tablas relacionadas
Revisar en `MundoVs/Infrastructure/Data/CrmDbContext.cs`:
- `Usuarios`
- `TiposUsuario`
- `Capacidades`
- `TipoUsuarioCapacidades`
- `Empresas`
- `SuscripcionesEmpresa`

## Dónde sale cada dato
### Inicio de sesión
- origen principal: `Usuario`
- validación técnica: `IAuthService.LoginAsync`
- reconstrucción de sesión y claims: `CustomAuthStateProvider.RestoreValidatedPrincipalAsync`

### Permisos del usuario
- origen principal: relación `TipoUsuario` -> `TipoUsuarioCapacidades` -> `Capacidad`
- consumo en UI: claims `Capacidad`
- archivo clave: `MundoVs/Core/Services/CustomAuthStateProvider.cs`

### Empresa activa del usuario
- origen principal: claim `EmpresaId`
- asignación: `CustomAuthStateProvider`
- contexto transversal: `IEmpresaContext`

### Cambio obligatorio de contraseña
- origen principal: propiedad `RequiereCambioPassword` del usuario
- exposición a UI: claim `RequirePasswordChange`
- consumo: `CambiarPasswordInicial.razor`

### Acceso bloqueado por empresa o suscripción
- origen principal:
  - estado de `Empresa`,
  - `IsSuspended`,
  - `TrialEndsAt`,
  - estado y fechas de `SuscripcionEmpresa`
- lógica clave: método `EmpresaPuedeOperar` en `CustomAuthStateProvider`

## Preguntas futuras y dónde buscar
### ¿De dónde salen los permisos que ve el usuario?
Buscar en:
1. `CustomAuthStateProvider.cs`
2. `TipoUsuarioCapacidades`
3. `Capacidades`

### ¿Por qué un usuario entra pero luego no puede navegar?
Buscar en:
1. claims cargados por `CustomAuthStateProvider`,
2. capacidades del tipo de usuario,
3. validación de empresa/suscripción.

### ¿Dónde se valida la contraseña?
Buscar en:
1. `IAuthService.ValidarPassword`
2. implementación concreta de `AuthService`

### ¿De dónde sale el token de recuperación?
Buscar en:
1. `IAuthService.GenerarTokenRecuperacionAsync`
2. implementación de `AuthService`
3. entidad `Usuario`

## Dónde buscar primero
- `MundoVs/Components/Pages/Auth/`
- `MundoVs/Core/Interfaces/IAuthService.cs`
- `MundoVs/Core/Services/CustomAuthStateProvider.cs`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
