# Plan SaaS para `MundoVs`

## Objetivo
Convertir `MundoVs` en un producto **SaaS multiempresa** estable, seguro y desplegable, dejando abierta la opción de **instancia dedicada por cliente** cuando sea necesario.

---

## Decisión base

### Estrategia recomendada
- **Producto principal:** `MundoVs SaaS`
- **Modelo técnico principal:** multiempresa con `EmpresaId`
- **Excepción:** clientes enterprise con base de datos o despliegue separado

### Regla de negocio
- SaaS primero
- Instancia dedicada solo por excepción

---

## Estado actual del proyecto

### Ya existe
- `EmpresaId` en entidades principales
- filtros por empresa en `CrmDbContext`
- `SuperAdmin`
- roles y capacidades
- creación de empresas
- configuración por empresa con `AppConfig`
- módulos iniciales de:
  - clientes
  - productos
  - pedidos
  - serigrafía
  - cuentas por pagar
  - empleados
  - nóminas

### Falta para SaaS sólido
- blindaje completo de multiempresa
- protección real por capacidad en páginas y acciones
- ciclo de vida de tenant
- onboarding SaaS
- planes y suscripciones
- auditoría
- seguridad operativa
- despliegue dockerizado

---

# Roadmap general

## Fase 1. Blindaje multiempresa
### Objetivo
Asegurar que ninguna empresa vea datos de otra.

### Tareas
- [x] Revisar todas las entidades de negocio y confirmar `EmpresaId` o dependencia tenant indirecta
- [x] Confirmar `QueryFilter` en entidades tenant directas y agregar filtros faltantes a entidades hijas/directas críticas
- [x] Revisar uso de `IgnoreQueryFilters()`
- [ ] Confirmar aislamiento en módulos nuevos:
  - [ ] `Proveedor`
  - [ ] `CuentaPorPagar`
  - [ ] `PagoCxP`
  - [ ] `Empleado`
  - [ ] `Nomina`
  - [ ] `NominaDetalle`
- [ ] Clasificar entidades en:
  - [x] globales
  - [x] tenant

### Resultado esperado
Toda entidad de negocio queda aislada por `EmpresaId`.

### Hallazgos de auditoría inicial

#### Entidades globales
- `Empresa`
- `Capacidad`

#### Entidades tenant directas (con `EmpresaId`)
- `AppConfig`
- `Cliente`
- `Producto`
- `Usuario`
- `TipoUsuario`
- `MateriaPrima`
- `Insumo`
- `TipoProceso`
- `ActividadManoObra`
- `GastoFijo`
- `Pantalla`
- `Diseno`
- `EscalaSerigrafia`
- `Horma`
- `Proveedor`
- `CuentaPorPagar`
- `Empleado`
- `Nomina`

#### Entidades tenant indirectas (aisladas por relación padre)
- `Contacto`
- `Pedido`
- `PedidoDetalle`
- `PedidoSeguimiento`
- `PagoPedido`
- `ProductoCliente`
- `ProductoCalzado`
- `TallaCalzado`
- `ProductoSerigrafia`
- `ColorSerigrafia`
- `EscalaSerigrafiaTalla`
- `CotizacionSerigrafia`
- `CotizacionDetalle`
- `PresupuestoProducto`
- `PresupuestoDetalle`
- `PedidoSerigrafia`
- `PedidoSerigrafiaProcesoDetalle`
- `PedidoSerigrafiaTalla`
- `PedidoSerigrafiaTallaProceso`
- `PagoCxP`
- `NominaDetalle`

#### Ajustes aplicados
- Se agregaron `QueryFilters` faltantes a entidades tenant indirectas críticas en `CrmDbContext`
- Se evitaron búsquedas por `FindAsync` en páginas tenant nuevas, usando consultas filtradas por tenant

---

## Fase 2. Autorización real por capacidad
### Objetivo
No depender solo del menú lateral.

### Tareas
- [ ] Proteger páginas por capacidad
- [ ] Proteger acceso por URL directa
- [ ] Validar capacidad también en acciones:
  - [ ] crear
  - [ ] editar
  - [ ] eliminar
  - [ ] registrar pagos
  - [ ] crear nómina
  - [ ] editar configuración
- [ ] Separar correctamente:
  - [ ] capacidades globales `empresas.*`
  - [ ] capacidades tenant (`clientes.*`, `cxp.*`, `nominas.*`, etc.)

### Páginas prioritarias
- [x] `SuperAdmin/Empresas`
- [x] `SuperAdmin/Capacidades`
- [x] `Admin/Usuarios`
- [x] `Admin/TiposUsuario`
- [x] `Compras/Proveedores`
- [x] `Compras/CuentasPorPagar`
- [x] `RRHH/Empleados`
- [x] `RRHH/Nominas`

### Resultado esperado
Aunque un usuario conozca la URL, no entra ni ejecuta acciones sin permiso.

---

## Fase 3. Ciclo de vida de empresa (tenant lifecycle)
### Objetivo
Administrar una empresa como tenant SaaS.

### Tareas
Extender `Empresa` con campos como:
- [x] `Estado`
- [x] `TrialEndsAt`
- [x] `IsSuspended`
- [x] `PlanActualId`
- [x] `MaxUsuarios`
- [x] `ActivatedAt`

### Estados sugeridos
- `Demo`
- `Activa`
- `Suspendida`
- `Cancelada`

### Resultado esperado
Cada empresa tiene estado y ciclo de vida controlado.

### Avance aplicado
- [x] Lifecycle agregado a `Empresa`
- [x] `SuperAdmin/Empresas` permite crear/editar estado, trial, suspensión, activación y límite de usuarios
- [x] `AuthService.LoginAsync()` bloquea tenants suspendidos, cancelados o demo vencida
- [x] `CustomAuthStateProvider` revalida el tenant al restaurar sesión
- [x] Migración EF Core creada para persistir lifecycle y backfill de empresas existentes

---

## Fase 4. Onboarding SaaS
### Objetivo
Dar de alta una empresa de forma repetible y ordenada.

### Tareas
- [x] Crear empresa
- [x] Crear admin inicial
- [x] Definir branding inicial:
  - [x] logo
  - [x] nombre comercial
  - [x] slogan
- [x] Configurar módulos iniciales
- [x] Definir flujo de bienvenida
- [x] Preparar cambio de contraseña inicial

### Resultado esperado
Un tenant nuevo queda listo para usar en minutos.

### Avance aplicado
- [x] El alta de empresa ya crea tenant, branding inicial, módulos base y admin inicial
- [x] Los usuarios creados para onboarding quedan con `RequiereCambioPassword = true`
- [x] Se creó la pantalla `Auth/CambiarPasswordInicial` para obligar el cambio en el primer acceso
- [x] `MainLayout` redirige al flujo de onboarding si el usuario aún no cambia su contraseña
- [x] `CustomAuthStateProvider` conserva el flag en sesión/claims para mantener el flujo consistente

---

## Fase 5. Planes y suscripciones
### Objetivo
Poder vender `MundoVs` como SaaS.

### Entidades sugeridas
- [x] `Plan`
- [x] `SuscripcionEmpresa`
- [x] `PagoSuscripcion`

### Campos sugeridos
#### `Plan`
- [x] nombre
- [x] precio mensual
- [x] precio anual
- [x] límite de usuarios
- [x] módulos incluidos
- [x] trial days
- [x] activo

#### `SuscripcionEmpresa`
- [x] empresaId
- [x] planId
- [x] fechaInicio
- [x] fechaFin
- [x] estado
- [x] renovación automática

### Reglas sugeridas
- [x] trial
- [x] expiración
- [x] suspensión por falta de pago
- [x] upgrade/downgrade
- [x] límites por plan

### Resultado esperado
El sistema soporta cobro recurrente por tenant.

### Avance aplicado
- [x] Modelo comercial creado con `Plan`, `SuscripcionEmpresa` y `PagoSuscripcion`
- [x] `Empresa` ya puede apuntar a un `PlanActual`
- [x] `SuperAdmin/Empresas` permite asignar plan, periodicidad, estado y vigencia de suscripción
- [x] Se agregaron seeds de planes base (`Base`, `Pro`, `Enterprise`)
- [x] Migración EF Core creada para persistir planes y suscripciones
- [x] El inicio de sesión y la restauración de sesión validan suscripción vigente
- [x] El sistema sincroniza `Empresa` con la suscripción actual al arrancar
- [x] Se aplica límite de usuarios activos con base en plan o `MaxUsuarios`

---

## Fase 6. Auditoría
### Objetivo
Registrar acciones importantes del sistema.

### Entidad sugerida
- [x] `AuditLog`

### Datos a guardar
- [x] `EmpresaId`
- [x] `UsuarioId`
- [x] `Accion`
- [x] `Entidad`
- [x] `EntidadId`
- [x] `Detalle`
- [x] `Fecha`
- [x] IP opcional

### Eventos a auditar primero
- [x] crear empresa
- [x] editar empresa
- [x] crear usuario
- [x] cambiar rol
- [x] crear capacidad
- [x] registrar pago CxP
- [x] crear nómina
- [ ] editar configuración

### Resultado esperado
Bitácora mínima para soporte y seguridad.

### Avance aplicado
- [x] Servicio central `IAuditService` / `AuditService`
- [x] Entidad `AuditLog` con filtro tenant y metadatos mínimos
- [x] Auditoría en `SuperAdmin/Empresas` para crear/editar empresa y registrar pago de suscripción
- [x] Auditoría en `Admin/Usuarios` para crear/editar usuario
- [x] Auditoría en `Admin/TiposUsuario` para crear/editar rol y asignar/quitar capacidades
- [x] Auditoría en `SuperAdmin/Capacidades` para crear/editar/eliminar capacidades
- [x] Auditoría en `Compras/CuentasPorPagar` para crear/editar cuenta y registrar pago
- [x] Auditoría en `RRHH/Nominas` para crear/editar nómina
- [x] Migración EF Core creada para persistir `AuditLog`

---

## Fase 7. Seguridad de acceso
### Objetivo
Endurecer autenticación y acceso.

### Tareas
- [x] recuperación de contraseña
- [x] cambio de contraseña inicial
- [x] expiración de sesión
- [x] bloqueo por intentos fallidos
- [x] políticas mínimas de contraseña
- [x] logout seguro
- [ ] opcional MFA

### Resultado esperado
Menor riesgo operativo y mejor control de acceso.

### Avance aplicado
- [x] Recuperación de contraseña por token temporal con páginas `Auth/RecuperarPassword` y `Auth/RestablecerPassword`
- [x] Bloqueo automático por intentos fallidos en `AuthService`
- [x] Expiración de sesión por inactividad en `CustomAuthStateProvider`
- [x] Política mínima centralizada de contraseña reutilizada en onboarding, reset y administración de usuarios
- [x] Logout más seguro limpiando sesión y contexto tenant

---

## Fase 8. Archivos por tenant
### Objetivo
Separar archivos por empresa.

### Tareas
- [x] mover uploads a estructura por tenant
- [x] definir convención como:
  - [x] `/uploads/{empresaId}/logos`
  - [x] `/uploads/{empresaId}/documentos`
- [x] validar tamaño y tipo
- [x] preparar futura migración a storage externo

### Resultado esperado
Archivos aislados y ordenados por tenant.

### Avance aplicado
- [x] Servicio `ITenantFileStorageService` / `TenantFileStorageService` para centralizar rutas y guardado por tenant
- [x] Los logos de empresa nuevos se guardan en `/uploads/{empresaId}/logos`
- [x] `Admin/Configuracion` y `SuperAdmin/Empresas` ya usan almacenamiento por tenant
- [x] Se mantiene compatibilidad con rutas antiguas guardadas en `CompanyLogo` y se elimina el archivo previo al reemplazarlo

---

## Fase 9. Operación SaaS
### Objetivo
Preparar el sistema para producción.

### Tareas
- [x] logging
- [x] monitoreo
- [x] backups
- [x] restauración
- [x] variables de entorno
- [x] secretos fuera del repo
- [x] health checks
- [x] manejo centralizado de errores

### Resultado esperado
Operación estable en ambientes reales.

### Avance aplicado
- [x] Logging base por consola, debug y HTTP logging
- [x] Endpoints `health/live` y `health/ready`
- [x] `DatabaseHealthCheck` para validar conectividad a MariaDB/MySQL
- [x] Página central `/Error` mejorada para producción
- [x] Runbook `docs/OPERACION-SAAS.md` con variables de entorno, secretos, backups y restauración

---

## Fase 10. Docker y despliegue
### Objetivo
Despliegue portable y repetible.

### Arquitectura recomendada
- [ ] `MundoVs` en Docker
- [ ] reverse proxy (`nginx`)
- [ ] BD separada
- [ ] volúmenes para uploads
- [ ] HTTPS

### Infraestructura sugerida
- VPS Linux o Azure
- app en contenedor
- base de datos fuera del contenedor de app

### Resultado esperado
Despliegue SaaS listo para staging y producción.

---

## Fase 11. Instancia dedicada por cliente
### Objetivo
Soportar clientes que pidan base o despliegue propio.

### Estrategia
- misma aplicación
- despliegue separado
- base separada
- configuración separada

### Cuándo aplica
- cliente enterprise
- cumplimiento/compliance
- cliente pide infraestructura propia
- personalización especial

### Resultado esperado
Oferta premium sin romper el modelo SaaS principal.

---

# Prioridad recomendada

## Orden sugerido
1. **Fase 1 — Blindaje multiempresa**
2. **Fase 2 — Autorización real por capacidad**
3. **Fase 3 — Ciclo de vida de empresa**
4. **Fase 4 — Onboarding**
5. **Fase 5 — Planes y suscripciones**
6. **Fase 6 — Auditoría**
7. **Fase 7 — Seguridad de acceso**
8. **Fase 8 — Archivos por tenant**
9. **Fase 9 — Operación SaaS**
10. **Fase 10 — Docker y despliegue**
11. **Fase 11 — Instancia dedicada opcional**

---

# Próximo paso práctico

## Siguiente ejecución sugerida
### Empezar por Fase 1 + Fase 2

#### Sprint 1
- [x] Auditar entidades tenant/globales
- [x] Auditar filtros por empresa
- [x] Auditar páginas nuevas

#### Sprint 2
- [x] Proteger páginas por capacidad
- [x] Proteger acciones por capacidad

#### Sprint 3
- [x] Auditar repositorios/servicios críticos para acceso por Id y filtros tenant
- [x] Endurecer `Repository<T>` para evitar `FindAsync` en `GetByIdAsync` y `ExistsAsync`
- [x] Endurecer `AuthService` para evitar `FindAsync` en cambio de password

#### Hallazgos del audit de repositorios/servicios
- El `IgnoreQueryFilters()` restante en `AuthService.LoginAsync()` es intencional para poder autenticar antes de establecer el tenant en sesión.
- No se encontraron otros `IgnoreQueryFilters()` en servicios/repositorios `.cs` de negocio fuera de los casos ya endurecidos.
- El método `FindAsync(predicate)` del repositorio genérico no rompe aislamiento por sí mismo; respeta los `QueryFilters` porque ejecuta `Where(...)` sobre el `DbSet` filtrado.

### Avance inicial aplicado
- [x] `SuperAdmin/Empresas` protegido con `empresas.editar`
- [x] `SuperAdmin/Capacidades` protegido con rol `SuperAdmin`
- [x] `Admin/Usuarios` protegido con `usuarios.ver` / `usuarios.editar`
- [x] `Admin/TiposUsuario` protegido con `usuarios.ver` / `usuarios.editar`
- [x] `Compras/Proveedores` protegido con `proveedores.ver` / `proveedores.editar`
- [x] `Compras/CuentasPorPagar` protegido con `cxp.ver` / `cxp.editar`
- [x] `RRHH/Empleados` protegido con `empleados.ver` / `empleados.editar`
- [x] `RRHH/Nominas` protegido con `nominas.ver` / `nominas.editar`

---

# Notas de implementación para este repo
- Mantener `SuperAdmin` solo como usuario de plataforma
- Evitar que `SuperAdmin` tenga acceso a operación diaria del tenant
- Mantener `empresas.*` como capacidades globales
- Las capacidades de negocio deben seguir siendo por tenant
- Para migraciones EF Core, seguir `.github/copilot-instructions.md`

---

# Criterio de éxito
El proyecto podrá operar como:
- `MundoVs SaaS` multiempresa
- con opción de cliente dedicado por excepción
- con seguridad, despliegue y crecimiento ordenado
