# Módulo `RRHH`

> Última revisión: 2026-07-03
> Mantenedor: equipo MundoVs

## Objetivo

Este módulo administra empleados, turnos, marcaciones, asistencias, esquemas de pago, vales de destajo, prenóminas, ausencias, banco de horas, bonos, deducciones e integración final de nómina. Es el módulo con mayor superficie del CRM MundoVs.

---

## Páginas incluidas

Páginas con ruta propia (24):

| Página | Ruta | Función |
|---|---|---|
| `Empleados.razor` | `/rrhh/empleados` | Lista de empleados con CRUD, asignación de turnos y esquemas. |
| `EmpleadoPerfil.razor` | `/rrhh/empleados/{Id:guid}/perfil` | Perfil con pestañas: Resumen, Personal, Horario, Laboral, Nómina, Asistencias, Ausencias, Saldos, Conceptos, Notas. |
| `Turnos.razor` | `/rrhh/turnos` | Catálogo de turnos con jornada diaria y descansos dinámicos (1-4). |
| `Marcaciones.razor` | `/rrhh/marcaciones` | Listado de marcaciones crudas con corrección de zona horaria. |
| `Asistencias.razor` | `/rrhh/asistencias` | Asistencias interpretadas por día; permite reprocesar y reintentar religado por `CodigoChecador`. |
| `AsistenciasSemanal.razor` | `/rrhh/asistencias-semanal` | Resumen semanal de asistencias. |
| `AsistenciasCorreccionModal.razor` | (modal, sin ruta) | Modal de corrección de asistencia con 4 pestañas (ver sección dedicada). |
| `Ausencias.razor` | `/rrhh/ausencias` | Solicitudes y administración de vacaciones, permisos, incapacidades. |
| `BancoHoras.razor` | `/rrhh/banco-horas` | Saldos y movimientos del banco de horas por empleado. |
| `BonosDistribuidos.razor` | `/rrhh/bonos-distribuidos` | Distribución de bonos por período/estructura/rubro. |
| `Checadores.razor` | `/rrhh/checadores` | Catálogo de relojes checadores ZKTeco. |
| `ControlTiempo.razor` | `/rrhh/control-tiempo` | Vista combinada de checadores + marcaciones + asistencias. |
| `Dashboard.razor` | `/rrhh/dashboard` | KPIs y resumen ejecutivo de RRHH. |
| `EmpleadoPerfil.razor` | (ya listado arriba) | – |
| `EsquemasPago.razor` | `/rrhh/esquemas-pago` | Catálogo de esquemas y tarifas por proceso/posición. |
| `EstadoAgente.razor` | `/rrhh/estado-agente` | Monitoreo del agente de sincronización ZKTeco. |
| `NominaReciboCard.razor` | (componente) | Tarjeta reutilizable de recibo de nómina. |
| `Nominas.razor` | `/rrhh/nominas` | Cálculo y administración de nóminas. |
| `Prenominas.razor` | `/rrhh/prenominas` | Prenóminas con captura rápida e incidencias. |
| `ReciboNomina.razor` | `/rrhh/nominas/recibo/{DetalleId:guid}` | Vista individual de recibo de nómina. |
| `RecibosNomina.razor` | `/rrhh/nominas/recibos/{NominaId:guid}` | Listado de recibos de una nómina. |
| `Turnos.razor` | (ya listado arriba) | – |
| `ValesDestajo.razor` | `/rrhh/vales-destajo` | Vales de destajo con autocarga desde producción. |

Partials / pestañas internas (no tienen `@page` propia, se renderizan dentro de otras páginas):

- `AsistenciasCorreccionMarcacionesTab.razor`
- `AsistenciasCorreccionPermisosTab.razor`
- `AsistenciasCorreccionResumenTab.razor`
- `AsistenciasCorreccionTiempoTab.razor`
- `NominaReciboCard.razor`

> **Nota:** Las páginas `ControlTiempo.razor` y `EstadoAgente.razor` existen en el código y están registradas en `sectionRoutes` de `NavMenu.razor`, pero **no tienen un `NavLink` renderizado**. Solo son accesibles por URL directa o por links contextuales desde `Checadores.razor` / `Dashboard.razor`.

---

## Qué información maneja

- expediente laboral del empleado,
- posición, departamento y tipo de nómina,
- turnos por vigencia del empleado,
- configuración diaria de jornada y **n descansos dinámicos** (`TurnoBaseDetalleDescanso`, 1-4),
- marcaciones crudas del reloj (ZKTeco),
- asistencias interpretadas por día (con perdón manual, descansos no descontados, modo de sugerencia de tiempo extra),
- tiempo trabajado bruto y neto,
- sueldo base, periodicidad y tipo de nómina,
- afiliación IMSS/ISR/INFONAVIT por empleado,
- esquema de pago vigente e historial,
- tarifas por proceso y/o posición,
- vales de destajo,
- prenóminas e incidencias del período,
- vacaciones, faltas, incapacidades y días trabajados,
- banco de horas (movimientos y saldo),
- bonos estructurados y distribuidos,
- deducciones estructuradas y conceptos configurables,
- integración de destajo a nómina,
- catálogo SAT (percepciones/deducciones),
- provisiones, vacaciones, ISR/subsidio, IMSS, INFONAVIT,
- configuración global de nómina (UMA, salario mínimo, tablas ISR/subsidio).

---

## Fuentes técnicas principales

### Páginas con code-behind dedicado

#### `Empleados.razor` / `Empleados.razor.cs`
Lista de empleados con CRUD, asignación de turnos y esquemas de pago.
El code-behind (`Empleados.razor.cs`) contiene la lógica de carga, validación y persistencia.
Cada fila incluye un botón de perfil que navega a `/rrhh/empleados/{id}/perfil`.

Usa:
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`
- `CodigoNegocioService`
- `INominaLegalPolicyService`

Consulta directamente:
- `Empleados`
- `Posiciones`
- `TurnosBase`
- `RrhhEmpleadosTurno` (DbSet; entity `RrhhEmpleadoTurno`)
- `EsquemasPago`
- `EmpleadosEsquemaPago` (DbSet; entity `EmpleadoEsquemaPago`)

#### `EmpleadoPerfil.razor` / `EmpleadoPerfil.razor.cs`
Página de perfil detallado por empleado con pestañas: Resumen, Personal, Horario, Laboral, Nómina, Asistencias, Ausencias, Saldos, Conceptos y Notas.
Ruta: `/rrhh/empleados/{id:guid}/perfil`.
Edición por capability (`empleados.editar`).

Usa:
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`
- `IRrhhEmpleadoPerfilPageService` (inyectado, separa lógica de consulta)
- `INominaLegalPolicyService`
- `CodigoNegocioService`

Contenido por pestaña:
- **Resumen**: KPIs, datos identitarios y resumen reciente.
- **Personal**: Datos personales con edición CRUD (capability protegida).
- **Horario**: Vista de calendario semanal con horario laboral y descansos por día, incluyendo el turno vigente.
- **Laboral**: Cargo, departamento, fecha contratación, etc.
- **Nómina**: Conceptos aplicables, salarios base, periodicidad, tipo de nómina.
- **Asistencias**: Últimos 30 días de control de tiempo con estatus y horas extra.
- **Ausencias**: Vacaciones, permisos y días solicitados/aprobados.
- **Saldos**: Días vacacionales disponibles y movimientos de banco de horas.
- **Conceptos**: Conceptos nómina recurrentes por empleado (ISR, IMSS, INFONAVIT, etc.).
- **Notas**: Campo libre de notas del empleado.

### Servicios del módulo

Servicios de asistencia (procesamiento y corrección):

| Servicio | Interfaz | Responsabilidad |
|---|---|---|
| `RrhhAsistenciaProcessor` | `IRrhhAsistenciaProcessor` | Procesa marcaciones pendientes y genera `RrhhAsistencia`. |
| `RrhhAsistenciasPageService` | `IRrhhAsistenciasPageService` | Datos agregados para la página `Asistencias.razor`. |
| `RrhhAsistenciaCorreccionAdvisor` | `IRrhhAsistenciaCorreccionAdvisor` | Sugiere la pestaña y la acción para el modal de corrección. |
| `RrhhTurnoValidator` | (sin interfaz) | Valida que el turno tenga 1-4 descansos y que no se traslapen. |
| `RrhhTiempoExtraPolicy` | (sin interfaz) | Reglas de elegibilidad de tiempo extra. |
| `RrhhTiempoExtraResolutionService` | `IRrhhTiempoExtraResolutionService` | Resuelve el destino del tiempo extra (pago / banco) según configuración. |
| `RrhhPermisoCompensationPolicy` | (sin interfaz) | Política de compensación de permisos con banco de horas. |
| `RrhhMarcacionSegmentActionHelper` | (sin interfaz) | Acciones de clasificado de segmentos desde el modal. |

Servicios de marcación e ingesta (ZKTeco):

| Servicio | Interfaz | Responsabilidad |
|---|---|---|
| `RrhhMarcacionIngestionService` | `IRrhhMarcacionIngestionService` | Recibe lotes del worker ZKTeco (`MarcacionSyncBatchDto`) y crea `RrhhMarcacion`. |
| `RrhhMarcacionZonaHorariaService` | `IRrhhMarcacionZonaHorariaService` | Corrige marcaciones guardadas como hora local vs UTC. |

Servicios de prenómina y nómina:

| Servicio | Interfaz | Responsabilidad |
|---|---|---|
| `RrhhPrenominaSnapshotService` | `IRrhhPrenominaSnapshotService` | Snapshot inmutable de configuración al cerrar prenómina. |
| `RrhhEmpleadoPerfilPageService` | `IRrhhEmpleadoPerfilPageService` | Datos agregados del perfil de empleado. |
| `NominaCalculator` | `INominaCalculator` | Cálculo principal de nómina (sueldo, destajo, bonos, horas extra, IMSS, ISR, etc.). |
| `NominaConfiguracionLoader` | (sin interfaz) | Carga `NominaConfiguracion` por empresa desde `AppConfig`. |
| `NominaLegalPolicyService` | `INominaLegalPolicyService` | Reglas legales: IMSS, INFONAVIT, festivos, prima dominical, etc. |
| `NominaPeriodoHelper` | (sin interfaz) | Cortes de período (semanal, quincenal, mensual). |
| `NominaReciboBuilder` | `INominaReciboBuilder` | Construcción del modelo de recibo de nómina. |
| `NominaResumenBuilder` | `INominaResumenBuilder` | Resumen de la nómina (totales, costo empresa, provisiones). |
| `NominaPdfService` | `INominaPdfService` | Generación del PDF del recibo. |
| `NominaSatCatalogInitializer` | `INominaSatCatalogInitializer` | Inicializa el catálogo SAT (`NominaSatCatalogos`). |
| `DestajoTarifaResolver` | (sin interfaz) | Resuelve la tarifa aplicada a una pieza/proceso (cotización → esquema de pago). |

Modelos auxiliares (`MundoVs/Core/Models/`):
- `RrhhAsistenciaCorreccionAdvisorModels.cs`
- `RrhhAsistenciaDescansoSettings.cs`
- `RrhhAsistenciaReprocesoRequest.cs`
- `RrhhAsistenciasFiltroState.cs`
- `RrhhMarcacionIngestionResult.cs`
- `RrhhMarcacionZonaHorariaCorrectionRequest.cs`
- `RrhhMarcacionZonaHorariaCorrectionResult.cs`

### Páginas adicionales

#### `Turnos.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`

Consulta directamente:
- `TurnosBase` (entity `TurnoBase`)
- `TurnosBaseDetalle`
- `TurnosBaseDetalleDescanso` (sub-entidad para los n descansos)

#### `Marcaciones.razor`
Consulta directamente:
- `RrhhMarcaciones` (entity `RrhhMarcacion`)
- `RrhhChecadores` (entity `RrhhChecador`)
- `Empleados`

#### `Asistencias.razor`
Consulta directamente:
- `RrhhAsistencias` (entity `RrhhAsistencia`)
- `TurnosBase`
- `Empleados`

Además permite:
- reprocesar asistencias por rango,
- reintentar religado de marcaciones por `CodigoChecador`,
- abrir el `AsistenciasCorreccionModal` (4 pestañas).

#### `AsistenciasCorreccionModal.razor` (4 pestañas)

| Pestaña | Función |
|---|---|
| **Resumen** | Resumen del día (entrada/salida, jornada, minutos, descansos, estatus). |
| **Tiempo** | Ajuste manual de minutos (jornada, retardo, salida anticipada, extra) y resolución. |
| **Marcaciones** | Edición directa de las marcaciones crudas del día. |
| **Permisos** | Selección / creación de un permiso o ausencia que cubra los faltantes. |

Las correcciones quedan registradas en `RrhhSegmentoResolucion` (entidad añadida por la migración `20260506152252_RrhhSegmentoResolucion`).

#### `ControlTiempo.razor`
Consulta directamente:
- `RrhhChecadores`
- `RrhhMarcaciones`
- `RrhhAsistencias`

#### `EsquemasPago.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`

Consulta directamente:
- `EsquemasPago`
- `EsquemasPagoTarifa`
- `TiposProceso`
- `Posiciones`

#### `ValesDestajo.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`
- `DestajoTarifaResolver` (inyectado)

Consulta directamente:
- `ValesDestajo`
- `ValeDestajoDetalle`
- `Empleados`
- `TiposProceso`
- `Pedidos`
- `Posiciones`
- `EsquemasPago`
- `CotizacionSerigrafia.Detalles` (en `Core/Entities/Serigrafia/`)
- configuración de nómina

#### `Prenominas.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`
- `IRrhhPrenominaSnapshotService` (al cerrar prenómina)
- `INominaConfiguracionLoader` (vía `NominaConfiguracionLoader`)

Consulta directamente:
- `Prenominas`
- `PrenominaDetalles`
- `Empleados`
- `ValesDestajo`
- `PrenominaCapturaRapidaRrhh` (bonos / percepciones de captura rápida)
- configuración de nómina

#### `Nominas.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `IAppConfigRepository`
- `IAuditService`
- `AuthenticationStateProvider`
- `INominaCalculator`
- `INominaResumenBuilder`
- `INominaReciboBuilder`
- `INominaLegalPolicyService`
- `INominaPdfService`
- `INominaSatCatalogInitializer`

Consulta directamente:
- `Nominas`
- `NominaDetalles`
- `Empleados`
- `ValesDestajo`
- `Prenominas`
- `PrenominaDetalles`
- `EmpleadosEsquemaPago`
- configuración de nómina desde `NominaConfiguracion` y `NominaConfiguracionGlobal`

#### `ReciboNomina.razor`, `RecibosNomina.razor`, `NominaReciboCard.razor`
Componentes relacionados con la visualización del recibo.

#### `EstadoAgente.razor`
Vista de monitoreo del agente ZKTeco (`RrhhEstadoAgente`): último heartbeat, marcaciones leídas/enviadas, último error, versión del agente.

#### `Checadores.razor`
Catálogo de checadores ZKTeco. Permite configurar IP, puerto, número de máquina, zona horaria, último evento leído.

#### `Dashboard.razor`
Vista de KPIs: empleados activos, checadores en línea, prenóminas pendientes, nóminas del período, alertas de jornada, etc.

#### `BancoHoras.razor`, `Ausencias.razor`, `BonosDistribuidos.razor`, `AsistenciasSemanal.razor`
Vistas operativas que consumen directamente las entidades `RrhhBancoHorasMovimiento`, `RrhhAusencia`, `BonoDistribucionPeriodoRrhh` y agregaciones semanales de `RrhhAsistencia`.

---

## Entidades y tablas relacionadas

Catálogo (`MundoVs/Core/Entities/`):

### Empleado y puesto
- `Empleado` — expediente maestro (datos personales, fiscales, IMSS, INFONAVIT, `CodigoChecador`, `TipoNomina`, `PeriodicidadPago`).
- `EmpleadoEsquemaPago` — esquema de pago vigente con historial.
- `Posicion` — puesto / actividad.
- `DepartamentoRrhh` — catálogo de departamentos.

### Turnos
- `TurnoBase` — turno base.
- `TurnoBaseDetalle` — jornada por día de la semana.
- `TurnoBaseDetalleDescanso` — **cada descanso es una fila** (1 a 4 descansos por día).
- `RrhhEmpleadoTurno` — vigencia de turno por empleado.

### Esquemas de pago
- `EsquemaPago` — tipos: `SueldoFijo`, `DestajoPorPieza`, `DestajoPorOperacion`, `BonoMetaPedidos`, `Mixto`.
- `EsquemaPagoTarifa` — tarifa por proceso/posición.

### Checador y marcaciones
- `RrhhChecador` — reloj físico (IP, puerto, zona horaria, último evento).
- `RrhhLogChecador` — bitácora de eventos del checador.
- `RrhhMarcacion` — marcación cruda con `FechaHoraMarcacionUtc`, `TipoMarcacionRaw`, `HashUnico`, `Procesada`, `ResultadoProcesamiento`.
- `RrhhEstadoAgente` — estado del agente de sincronización.

### Asistencia y corrección
- `RrhhAsistencia` — asistencia interpretada del día (campos clave: `MinutosTrabajadosBrutos/Netos`, `MinutosDescansoProgramado/Tomado/Pagado/NoPagado`, `MinutosRetardo`, `MinutosSalidaAnticipada`, `MinutosExtra`, `MinutosExtraAutorizadosPago/Banco`, `MinutosCubiertosBancoHoras`, `MinutosPerdonadosManual`, `ModoSugerenciaExtra`, `ResolucionTiempoExtra`, `DescansosNoDescontar`, `ObservacionPerdonManual`).
- `RrhhSegmentoResolucion` — resoluciones manuales de segmentos (Trabajo/Extra/Descanso/SalidaTemporal/Permiso/NoConsiderar/DescansoNoDescontar).

### Tiempo y descansos
- `RrhhBancoHorasMovimiento` — movimientos del banco (`GeneradoPorHorasExtra`, `AjusteManual`, `Consumo`).
- `RrhhAusencia` — solicitud de ausencia con tipo (`Vacaciones`, `PermisoConGoce`, `PermisoSinGoce`, `Incapacidad`, `FaltaInjustificada`, `Suspension`, `DiaEconomico`, `PermisoPaternidad`, `PermisoMaternidad`, `Capacitacion`, `Otro`) y estatus; incluye `ConGocePago` y `DescuentaBancoHoras`.

### Prenómina y nómina
- `Prenomina` — prenómina con `SnapshotConfiguracionJson` y `FechaCierre`.
- `PrenominaDetalle` — detalle por empleado con `DiasPagados`, `HorasTrabajadasNetas`, `HorasBancoAcumuladas/Consumidas`, `MinutosRetardo/SalidaAnticipada/PerdonadosManual/FaltanteDescontable`, `MontoDestajoInformativo`, `ComplementoSalarioMinimoSugerido`.
- `PrenominaCapturaRapidaRrhh` — captura rápida de bonos/percepciones de prenómina.
- `Nomina` — nómina con `Folio`, `NumeroNomina`, `PrenominaId`, `EstatusNomina` (`Borrador`/`Aprobada`/`Pagada`/`Cancelada`).
- `NominaDetalle` — desglose completo (`TotalPagar`, `TotalObligacionesTerceros`, `TotalAportacionesPatronales`, `TotalProvisiones`, `CostoEmpresa`).
- `NominaConfiguracion` — configuración de nómina por empresa (UMA, ISR, IMSS, banco, vacaciones, etc.).
- `NominaConfiguracionGlobal` — parámetros globales (`UmaDiaria`, `SalarioMinimoGeneral`, `SalarioMinimoFrontera`, `TablaIsrJson`, `TablaSubsidioJson`).
- `NominaPeriodoRrhh` — contiene `NominaCorteRrhh` (cortes semanales/quincenales/mensuales) y `FestivoRrhh` (catálogo de festivos).

### Bonos y deducciones
- `NominaBonoRrhh` — `BonoRubroRrhh`, `BonoEstructuraRrhh`, `BonoEstructuraDetalleRrhh`, `NominaBono`, `NominaBonoDetalle`.
- `BonoDistribucionPeriodoRrhh` — `BonoDistribucionEmpleadoRrhh`, `BonoDistribucionEmpleadoDetalleRrhh` (bonos distribuidos por período/posición/rubro).
- `NominaDeduccionRrhh` — `DeduccionTipoRrhh`, `NominaDeduccion`.
- `NominaPercepcionRrhh` — `NominaPercepcionTipo`, `NominaPercepcion`, con enums `CategoriaPercepcionNomina` y `OrigenPercepcionNomina`.
- `NominaConceptoConfigRrhh` — conceptos configurables con `EmpleadoConceptoRrhh` y `NominaProvisionDetalleRrhh` + enums `NaturalezaConceptoNominaRrhh`, `DestinoConceptoNominaRrhh`, `TipoCalculoConceptoNominaRrhh`.

### Vales de destajo
- `ValeDestajo` — vale agrupador.
- `ValeDestajoDetalle` — detalle por proceso/pedido/empleado.

### AppConfig (catálogo general)
- `AppConfigs` — claves de configuración de nómina (factor horas extra, banco de horas, periodicidad, etc.).
- `TiposProceso` — tipos de proceso usados en esquemas de pago y vales de destajo.
- `Pedidos` — pedido del que se hereda la cotización para destajo.
- `CotizacionSerigrafia` y `CotizacionSerigrafia.Detalles` — fuente del tiempo estándar por proceso.

---

## Dónde sale cada dato

### Información principal del empleado
- origen principal: `Empleado`
- complementos:
  - `PosicionId`
  - `PeriodicidadPago`, `TipoNomina`
  - `Departamento` (string o `DepartamentoRrhh`)
  - `SueldoSemanal`

### Turno vigente e histórico
- origen principal: `RrhhEmpleadoTurno` (vigencias)
- fallback actual visible en empleado: `Empleado.TurnoBaseId`
- catálogo base: `TurnoBase`
- detalle diario del turno: `TurnoBaseDetalle`

### Jornada diaria y descansos (descansos dinámicos)
- origen principal: `TurnoBaseDetalle`
- por día se define:
  - entrada,
  - salida,
  - si labora,
  - **n descansos** (1-4) almacenados como filas en `TurnoBaseDetalleDescanso`,
  - horario de cada descanso,
  - si cada descanso es pagado o no.
- validación: `RrhhTurnoValidator.Validate(...)` (ver `RrhhTurnoValidatorTests.Validate_AceptaTurnosConEntreUnoYCuatroDescansos`).

### Asistencia diaria interpretada
- origen principal: `RrhhAsistencia`
- datos operativos actuales:
  - entrada y salida programada,
  - entrada y salida real,
  - total de marcaciones,
  - tiempo trabajado bruto y neto,
  - minutos de jornada programada y neta programada,
  - descansos programados, tomados, pagados y no pagados,
  - retardo, salida anticipada, tiempo extra,
  - minutos perdonados manualmente (`MinutosPerdonadosManual`, `ObservacionPerdonManual`),
  - descansos no descontados (`DescansosNoDescontar`, p. ej. `"1,2"`),
  - resolución de tiempo extra (`ResolucionTiempoExtra`),
  - modo de sugerencia de extra (`ModoSugerenciaExtra` = `"EntradaSalida"` o `"NetoVsNeto"`).

### Esquema de pago vigente
- origen principal: `EmpleadoEsquemaPago` (con `VigenteDesde/Hasta`)
- catálogo base: `EsquemaPago`

### Tarifas por operación o proceso
- origen principal: `EsquemaPagoTarifa`
- referencias cruzadas:
  - `TipoProceso`
  - `Posicion`

### Destajo del empleado
- origen principal:
  - `ValeDestajo`
  - `ValeDestajoDetalle`
- el detalle guarda proceso, pedido, cantidad, tarifa e importe.

### Horas extra
- captura base:
  - `PrenominaDetalle`
- configuración base:
  - `NominaConfiguracion` (vía `AppConfig`)
- reflejo final:
  - `NominaDetalle`
- política:
  - `RrhhTiempoExtraPolicy` (reglas de elegibilidad)
  - `RrhhTiempoExtraResolutionService` (resolución pago / banco, topes)

### Días trabajados, vacaciones e incidencias
- origen principal:
  - `PrenominaDetalle`
- reglas:
  - `NominaConfiguracion` y `NominaConfiguracionGlobal`

### Aplicación de IMSS
- origen principal:
  - `Empleado` (`AplicaImss`, `Nss`)
- parámetros base:
  - `NominaConfiguracion`

### Total a pagar en nómina
- origen principal: `NominaDetalle`
- componentes típicos:
  - sueldo base,
  - destajo,
  - días trabajados y vacaciones,
  - bono,
  - horas extra,
  - bonos distribuidos y adicionales,
  - deducciones,
  - IMSS, INFONAVIT, ISR, subsidio al empleo cuando apliquen,
  - provisiones, obligaciones a terceros, costo empresa.

---

## Preguntas futuras y dónde buscar

### ¿Cómo saber qué esquema de pago tiene un empleado?
Buscar en:
1. `RRHH/Empleados.razor`
2. `EmpleadoEsquemaPago` (vigencias)
3. `EsquemaPago`

### ¿Dónde se guardan las tarifas por proceso?
Buscar en:
1. `RRHH/EsquemasPago.razor`
2. `EsquemaPagoTarifa`
3. `TipoProceso`
4. `Posicion`

### ¿De dónde sale el destajo que entra a nómina?
Buscar en:
1. `RRHH/ValesDestajo.razor`
2. `DestajoTarifaResolver.cs` (reglas de resolución de tarifa)
3. `ValeDestajo`
4. `ValeDestajoDetalle`
5. integración en `RRHH/Nominas.razor`

### ¿De dónde salen vacaciones, faltas y días trabajados?
Buscar en:
1. `RRHH/Prenominas.razor`
2. `Prenomina`
3. `PrenominaDetalle`
4. `RrhhAusencia` (vacaciones, permisos, incapacidades)
5. integración en `RRHH/Nominas.razor`

### ¿Cómo saber si a un empleado se le calcula IMSS?
Buscar en:
1. `RRHH/Empleados.razor`
2. `Empleado` (campos `AplicaImss`, `Nss`, `AplicaInfonavit`, `FactorDescuentoInfonavit`, `NumeroCreditoInfonavit`)
3. configuración de nómina en `NominaConfiguracion` / `AppConfigs`

### ¿Dónde se configuran horas base y factor de horas extra?
Buscar en:
1. `Admin/ConfiguracionNomina.razor`
2. `AppConfigs` (claves en `ClavesConfiguracionNomina` — `FactorHoraExtra`, `BancoHorasTopeHoras`, etc.)
3. `RRHH/Nominas.razor`
4. `RrhhTiempoExtraResolutionService.cs`

### ¿Dónde se configuran turnos, descansos y jornada diaria?
Buscar en:
1. `RRHH/Turnos.razor`
2. `TurnoBase`
3. `TurnoBaseDetalle`
4. `TurnoBaseDetalleDescanso` (1-4 descansos)
5. `RrhhTurnoValidator.cs` (validación)

### ¿Dónde se cambia el turno de un empleado con vigencia?
Buscar en:
1. `RRHH/Empleados.razor`
2. `RrhhEmpleadoTurno`
3. `Empleado.TurnoBaseId`

### ¿Dónde se ve tiempo trabajado diario y revisión de descansos?
Buscar en:
1. `RRHH/Asistencias.razor`
2. `RRHH/ControlTiempo.razor`
3. `RRHH/AsistenciasSemanal.razor`
4. `RrhhAsistencia`
5. `AsistenciasCorreccionModal.razor`

### ¿Cómo se procesan descansos a partir de marcaciones?
Buscar en:
1. `MundoVs/Core/Services/RrhhAsistenciaProcessor.cs`
2. `RrhhTurnoValidator.cs`
3. `RrhhPermisoCompensationPolicy.cs`
4. `RrhhMarcacionSegmentActionHelper.cs`
5. `RrhhMarcacion`
6. `RrhhAsistencia`

### ¿Cómo se monitorea el agente de sincronización ZKTeco?
Buscar en:
1. `RRHH/EstadoAgente.razor`
2. `RRHH/Dashboard.razor`
3. `RrhhEstadoAgente`
4. `RrhhLogChecador`
5. `Zenith.Workers.Asistencia/` (proyecto aparte — worker service).

---

## Cobertura de pruebas

Tests unitarios en `MundoVs.Tests/` que cubren el módulo RRHH:

| Test | Cubre |
|---|---|
| `RrhhTurnoValidatorTests` | Validación de turnos con 1-4 descansos y detección de traslapes. |
| `RrhhAsistenciaProcessorTests` | Procesamiento de marcaciones (compensación retardo/salida tardía, no generación de extra incorrecta, etc.). |
| `RrhhAsistenciaCorreccionAdvisorTests` | Lógica de sugerencia de pestaña y acción en el modal de corrección. |
| `RrhhMarcacionIngestionServiceTests` | Ingesta de lotes ZKTeco, conteo de nuevas/duplicadas/fallidas. |
| `RrhhMarcacionZonaHorariaServiceTests` | Corrección de marcaciones guardadas como hora local. |
| `RrhhTiempoExtraResolutionServiceTests` | Cálculo de contexto de empleado, saldo banco y configuración consolidada. |
| `NominaConfiguracionLoaderTests` | Carga de configuración de nómina. |
| `NominaPeriodoHelperTests` | Cortes de período. |
| `NominaReciboBuilderTests` | Construcción del recibo. |
| `NominaVacacionesImssTests` | Cálculo de vacaciones e IMSS. |

`MultiEmpresaIsolationTests` (raíz de `MundoVs.Tests/`) aplica al módulo pero está más alineado con contabilidad.

---

## Dónde buscar primero

- `MundoVs/Components/Pages/RRHH/` — todas las páginas
- `MundoVs/Core/Entities/Empleado.cs` — entidad maestra
- `MundoVs/Core/Entities/RrhhAsistencia.cs` — datos diarios
- `MundoVs/Core/Entities/TurnoBase.cs` — incluye `TurnoBaseDetalle` y `TurnoBaseDetalleDescanso`
- `MundoVs/Core/Entities/EsquemaPago.cs` y `EsquemaPagoTarifa.cs`
- `MundoVs/Core/Entities/ValeDestajo.cs` y `ValeDestajoDetalle.cs`
- `MundoVs/Core/Entities/Prenomina.cs` y `PrenominaDetalle.cs`
- `MundoVs/Core/Entities/Nomina.cs`, `NominaDetalle.cs`, `NominaConfiguracion.cs`, `NominaConfiguracionGlobal.cs`
- `MundoVs/Core/Services/Rrhh*.cs` y `Nomina*.cs`
- `MundoVs/Core/Interfaces/IRrhh*.cs` e `INomina*.cs`
- `MundoVs/Core/Models/Rrhh*.cs`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
- `MundoVs/Zenith.Workers.Asistencia/` — worker ZKTeco

---

## Flujo funcional actual: cotización → pedido → destajo → prenómina → nómina

### 1. Cotización
- la base del destajo arranca en la cotización seleccionada del pedido,
- el tiempo estándar por operación se toma de `CotizacionSerigrafia.Detalles`,
- solo cuenta detalle con:
  - categoría `ManoObra`,
  - `TipoProcesoId` del proceso,
  - `Tiempo` capturado.

Referencia técnica:
- `PedidoSeguimiento.razor` → `ObtenerMinutosCotizacion(...)`
- `ValesDestajo.razor` → `ObtenerMinutosEstandarPorPieza(...)`

### 2. Pedido
- el pedido conserva la cotización seleccionada en el detalle,
- el seguimiento de producción trabaja contra `PedidoSerigrafia`,
- por cada talla y proceso se crea o consulta `PedidoSerigrafiaTallaProceso`.

Referencia técnica:
- `PedidoSeguimiento.razor` → carga de `PedidosSerigrafia`, `PedidoDetalle`, `CotizacionSerigrafia`, `Tallas` y `TiposProceso`

### 3. Seguimiento de producción
- en la pantalla de seguimiento se captura quién trabajó, cuántas piezas hizo, minutos e importe,
- el registro operativo histórico sigue siendo `RegistroDestajoProceso`,
- cada registro queda ligado a:
  - pedido,
  - talla,
  - proceso,
  - empleado,
  - fecha,
  - cantidad,
  - tarifa,
  - importe.

Referencia técnica:
- `PedidoSeguimiento.razor` → `AgregarDestajo(...)`
- `PedidoSeguimiento.razor` → `GuardarDestajo(...)`

### 4. Cómo se resuelve hoy la tarifa del destajo
- el sistema calcula primero los minutos por pieza desde la cotización,
- después carga las tarifas del esquema activo del empleado para la fecha del movimiento,
- finalmente ejecuta `DestajoTarifaResolver.Resolver(...)`.

Prioridad actual implementada:
1. tarifa derivada de `minutosPorPieza × TarifaPorMinuto` de la operación,
2. si falta información de cotización u operación, usa `EsquemaPagoTarifa` como respaldo.

Referencia técnica:
- `PedidoSeguimiento.razor` → `ResolverTarifaDestajoAsync(...)`
- `ValesDestajo.razor` → `ResolverTarifa(...)`
- `Core/Services/DestajoTarifaResolver.cs`

### 5. Vales de destajo
- los vales son el documento agrupador que llega a nómina,
- pueden capturarse manualmente o autocargarse desde `RegistroDestajoProceso`,
- cada línea guarda:
  - proceso,
  - pedido,
  - cantidad,
  - tarifa aplicada,
  - importe,
  - tarifa origen,
  - registros origen cuando vino de producción.

Referencia técnica:
- `ValesDestajo.razor` → `AutocargarDesdeProduccion()`
- `ValesDestajo.razor` → `GuardarVale(...)`

### 6. Prenómina
- la prenómina no es la fuente final del destajo pagado,
- la prenómina guarda `MontoDestajoInformativo` para revisión del período,
- su función principal es consolidar:
  - días trabajados,
  - días pagados,
  - vacaciones,
  - faltas,
  - incapacidades,
  - horas extra,
  - validación de IMSS,
  - captura rápida de bonos y percepciones (`PrenominaCapturaRapidaRrhh`).

Al cerrar la prenómina:
- se congela la configuración en `SnapshotConfiguracionJson` (vía `IRrhhPrenominaSnapshotService`),
- la prenómina queda ligada a la nómina por `Nomina.PrenominaId`.

Referencia técnica:
- `PrenominaDetalle.cs`
- `Prenominas.razor`
- `RrhhPrenominaSnapshotService.cs`

### 7. Nómina
- la nómina exige una prenómina cerrada del mismo período,
- al sincronizar:
  - toma incidencias desde `PrenominaDetalle`,
  - toma destajo real desde `ValesDestajo` aprobados,
  - suma `TotalPiezas` y `MontoDestajo`,
  - liga cada vale al `NominaDetalle`,
  - cambia el vale a `EnNomina`,
  - calcula: sueldo base, tiempo extra, bonos, deducciones, IMSS, INFONAVIT, ISR, subsidio, provisiones, costo empresa,
  - persiste el detalle en `NominaDetalle` y el recibo en `ReciboNomina.razor`.

Referencia técnica:
- `Nominas.razor` → `GuardarNomina()`
- `Nominas.razor` → `SincronizarNominaAsync(...)`
- `NominaCalculator.cs`
- `NominaResumenBuilder.cs`
- `NominaReciboBuilder.cs`
- `NominaPdfService.cs`

---

## Observaciones funcionales vigentes

### Lo que ya sí funciona
- la cotización sí alimenta el tiempo estándar por proceso,
- el destajo sí llega a nómina desde vales aprobados,
- la prenómina sí queda ligada a la nómina por `PrenominaId`,
- el cálculo de nómina ya combina incidencias del período + destajo real,
- el catálogo SAT se inicializa con `NominaSatCatalogInitializer`,
- la corrección de asistencia se hace con 4 pestañas vía `AsistenciasCorreccionModal`,
- los descansos dinámicos (1-4) están soportados en `TurnoBaseDetalleDescanso` y validados por `RrhhTurnoValidator`,
- el banco de horas tiene consumo por permisos y generación por horas extra.

### Resoluciones aplicadas
1. `VincularRegistrosDestajoAVales` (migración 2026-04-08) — el `ValeDestajo` ahora se vincula formalmente al `RegistroDestajoProceso` origen; la captura manual coexiste con la autocarga.
2. `RrhhSegmentoResolucion` (migración 2026-05-06) — la corrección manual de asistencia se persiste como segmento resoluble (incluye `DescansoNoDescontar`).
3. `RrhhAsistenciaPerdonManualVisible` (migración 2026-05-07) — el perdón manual de minutos es visible para RRHH y se persiste en prenómina y nómina.
4. `AddDescansosNoDescontar` (migración 2026-06-22) — los descansos no tomados se pueden marcar explícitamente como no descontados (campo `DescansosNoDescontar` en `RrhhAsistencia`).
5. `AusenciaDescuentaBancoHoras` (migración 2026-06-09) — las ausencias marcadas con `DescuentaBancoHoras = true` consumen saldo del banco de horas.
6. `RrhhBancoHorasCapacidades` (migración 2026-06-10) — capacidades explícitas (`rrhh.bancohoras.ver`/`rrhh.bancohoras.editar`).
7. `RrhhTurnosDescansosDinamicos` (migración 2026-05-14) — los descansos pasaron de slots fijos a colección de `TurnoBaseDetalleDescanso`.

### Huecos vigentes
1. En `ValesDestajo.razor`, para resolver tarifa hoy se requiere pedido; aunque la propuesta funcional habla de pedido opcional, la implementación actual devuelve error de resolución si no hay pedido seleccionado. **Sigue abierto.**
2. En `PedidoSeguimiento.razor`, la tarifa del registro sigue siendo editable manualmente en pantalla; eso mantiene un riesgo de captura fuera de la lógica auditada. **Sigue abierto.**

---

## Resumen operativo
- `Cotización` define tiempos de mano de obra por proceso.
- `Pedido` hereda esa cotización seleccionada.
- `Seguimiento` registra producción por talla/proceso/empleado.
- `ValeDestajo` agrupa y formaliza el destajo del período.
- `Prenómina` concentra incidencias laborales y congela su configuración al cerrar.
- `Nómina` consolida incidencias + vales aprobados + tiempo extra + bonos + deducciones para calcular el pago final.

---

## Ver también

Documentación relacionada:

- [Manual de configuración inicial de RRHH](../manual/06-rrhh-configuracion-inicial.md)
- [Turnos](../manual/07-rrhh-turnos.md)
- [Esquemas de pago](../manual/08-rrhh-esquemas-de-pago.md)
- [Empleados](../manual/09-rrhh-empleados.md)
- [Marcaciones y asistencias](../manual/10-rrhh-marcaciones-y-asistencias.md)
- [Prenómina](../manual/11-rrhh-prenomina.md)
- [Nómina](../manual/12-rrhh-nomina.md)
- [Vales de destajo](../manual/13-rrhh-vales-destajo.md)
- [Recibo de nómina](../manual/14-rrhh-recibo-nomina.md)
- [Bonos y deducciones](../manual/15-rrhh-bonos-y-deducciones.md)
- [Banco de horas](../manual/16-rrhh-banco-de-horas.md)
- [Ausencias](../manual/17-rrhh-ausencias.md)
- [Bonos distribuidos](../manual/19-rrhh-bonos-distribuidos.md)
- [Cálculo de tiempo visible](../manual/20-rrhh-calculo-tiempo-visible.md)
- [Descansos no marcados](../manual/rrhh-descansos-no-marcados.md)
- [Nómina — técnica (cálculo)](../manual/rrhh-nomina-tecnica.md)
- [Nómina — propuesta evolutiva (histórico)](../manual/rrhh-nomina-propuesta-evolutiva.md)
- [Permisos, descansos y banco de horas — plan técnico](../manual/rrhh-permisos-descansos-banco-horas-plan-tecnico.md)
- [Plan de resolución de vulnerabilidades de asistencia](../manual/rrhh-plan-resolucion-vulnerabilidades-asistencia.md)
- [Arquitectura ZKTeco (fases)](../rrhh-asistencia-zkteco-arquitectura-fases.md)
- [ZKTeco — puesta en marcha](../manuales/rrhh-asistencia-zkteco-puesta-en-marcha.md)
- [Marco legal de nómina](../conocimiento/nomina-marco-legal.md)
