# Módulo `RRHH`

## Objetivo
Este módulo administra empleados, esquemas de pago, destajo, prenómina e integración final de nómina.

## Páginas incluidas
- `MundoVs/Components/Pages/RRHH/Empleados.razor`
- `MundoVs/Components/Pages/RRHH/Turnos.razor`
- `MundoVs/Components/Pages/RRHH/Marcaciones.razor`
- `MundoVs/Components/Pages/RRHH/Asistencias.razor`
- `MundoVs/Components/Pages/RRHH/ControlTiempo.razor`
- `MundoVs/Components/Pages/RRHH/EsquemasPago.razor`
- `MundoVs/Components/Pages/RRHH/ValesDestajo.razor`
- `MundoVs/Components/Pages/RRHH/Prenominas.razor`
- `MundoVs/Components/Pages/RRHH/Nominas.razor`

## Qué información maneja
- expediente laboral del empleado,
- posición y departamento,
- turnos por vigencia del empleado,
- configuración diaria de jornada y descansos,
- marcaciones crudas del reloj,
- asistencias interpretadas por día,
- tiempo trabajado bruto y neto,
- sueldo base y periodicidad,
- afiliación IMSS por empleado,
- esquema de pago vigente e historial,
- tarifas por proceso,
- vales de destajo,
- prenóminas e incidencias del período,
- vacaciones, faltas, incapacidades y días trabajados,
- integración de destajo a nómina,
- bonos, horas extra, deducciones y total a pagar.

## Fuentes técnicas principales
### `Empleados.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`

Consulta directamente:
- `Empleados`
- `Posiciones`
- `TurnosBase`
- `RrhhEmpleadosTurno`
- `EsquemasPago`
- `EmpleadosEsquemaPago`

### `Turnos.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`

Consulta directamente:
- `TurnosBase`
- `TurnosBaseDetalle`

### `Marcaciones.razor`
Consulta directamente:
- `RrhhMarcaciones`
- `RrhhChecadores`
- `Empleados`

### `Asistencias.razor`
Consulta directamente:
- `RrhhAsistencias`
- `TurnosBase`
- `Empleados`

Además permite:
- reprocesar asistencias por rango,
- reintentar religado de marcaciones por `CodigoChecador`.

### `ControlTiempo.razor`
Consulta directamente:
- `RrhhChecadores`
- `RrhhMarcaciones`
- `RrhhAsistencias`

### `EsquemasPago.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`

Consulta directamente:
- `EsquemasPago`
- `EsquemasPagoTarifa`
- `TiposProceso`
- `Posiciones`

### `ValesDestajo.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`

Consulta directamente:
- `ValesDestajo`
- `ValeDestajoDetalle`
- `Empleados`
- `TiposProceso`
- `Pedidos`
- `Posiciones`
- `EsquemasPago`
- configuración de nómina

### `Prenominas.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`

Consulta directamente:
- `Prenominas`
- `PrenominaDetalles`
- `Empleados`
- `ValesDestajo`
- configuración de nómina

### `Nominas.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `IAppConfigRepository`
- `IAuditService`
- `AuthenticationStateProvider`

Consulta directamente:
- `Nominas`
- `NominaDetalles`
- `Empleados`
- `ValesDestajo`
- `Prenominas`
- `PrenominaDetalles`
- `EmpleadosEsquemaPago`
- configuración de nómina desde `AppConfig`

## Entidades y tablas relacionadas
Ver en `CrmDbContext.cs`:
- `Empleados`
- `TurnosBase`
- `TurnosBaseDetalle`
- `RrhhEmpleadoTurno`
- `RrhhMarcaciones`
- `RrhhAsistencias`
- `Posiciones`
- `EsquemasPago`
- `EsquemasPagoTarifa`
- `EmpleadosEsquemaPago`
- `ValesDestajo`
- `ValesDestajoDetalle`
- `Prenominas`
- `PrenominaDetalles`
- `Nominas`
- `NominaDetalles`
- `AppConfigs`
- `TiposProceso`
- `Pedidos`

## Dónde sale cada dato
### Información principal del empleado
- origen principal: `Empleado`
- complementos:
  - `PosicionId`
  - `PeriodicidadPago`
  - `Departamento`
  - `SueldoSemanal`

### Turno vigente e histórico
- origen principal: `RrhhEmpleadoTurno`
- fallback actual visible en empleado: `Empleado.TurnoBaseId`
- catálogo base: `TurnoBase`
- detalle diario del turno: `TurnoBaseDetalle`

### Jornada diaria y descansos
- origen principal: `TurnoBaseDetalle`
- por día se define:
  - entrada,
  - salida,
  - si labora,
  - `0`, `1` o `2` descansos,
  - horario de cada descanso,
  - si cada descanso es pagado o no.

### Asistencia diaria interpretada
- origen principal: `RrhhAsistencia`
- datos operativos actuales:
  - entrada y salida programada,
  - entrada y salida real,
  - total de marcaciones,
  - tiempo trabajado bruto,
  - tiempo trabajado neto,
  - descansos programados y tomados,
  - retardo,
  - salida anticipada,
  - tiempo extra,
  - observaciones de revisión.

### Esquema de pago vigente
- origen principal: `EmpleadoEsquemaPago`
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
  - `AppConfigs` de nómina
- reflejo final:
  - `NominaDetalle`

### Días trabajados, vacaciones e incidencias
- origen principal:
  - `PrenominaDetalle`
- catálogos/reglas:
  - `AppConfigs`

### Aplicación de IMSS
- origen principal:
  - `Empleado` (`AplicaIMSS`)
- parámetros base:
  - `AppConfigs`

### Total a pagar en nómina
- origen principal: `NominaDetalle`
- componentes típicos:
  - sueldo base,
  - destajo,
  - días trabajados y vacaciones,
  - bono,
  - horas extra,
  - bonos adicionales,
  - deducciones,
  - IMSS cuando aplique.

## Preguntas futuras y dónde buscar
### ¿Cómo saber qué esquema de pago tiene un empleado?
Buscar en:
1. `RRHH/Empleados.razor`
2. `EmpleadosEsquemaPago`
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
2. `ValeDestajo`
3. `ValeDestajoDetalle`
4. integración en `RRHH/Nominas.razor`

### ¿De dónde salen vacaciones, faltas y días trabajados?
Buscar en:
1. `RRHH/Prenominas.razor`
2. `Prenomina`
3. `PrenominaDetalle`
4. integración en `RRHH/Nominas.razor`

### ¿Cómo saber si a un empleado se le calcula IMSS?
Buscar en:
1. `RRHH/Empleados.razor`
2. `Empleado`
3. configuración de nómina en `AppConfigs`

### ¿Dónde se configuran horas base y factor de horas extra?
Buscar en:
1. `Admin/ConfiguracionNomina.razor`
2. `AppConfigs`
3. `RRHH/Nominas.razor`

### ¿Dónde se configuran turnos, descansos y jornada diaria?
Buscar en:
1. `RRHH/Turnos.razor`
2. `TurnoBase`
3. `TurnoBaseDetalle`

### ¿Dónde se cambia el turno de un empleado con vigencia?
Buscar en:
1. `RRHH/Empleados.razor`
2. `RrhhEmpleadoTurno`
3. `Empleado.TurnoBaseId`

### ¿Dónde se ve tiempo trabajado diario y revisión de descansos?
Buscar en:
1. `RRHH/Asistencias.razor`
2. `RRHH/ControlTiempo.razor`
3. `RrhhAsistencia`

### ¿Cómo se procesan descansos a partir de marcaciones?
Buscar en:
1. `MundoVs/Core/Services/RrhhAsistenciaProcessor.cs`
2. `RrhhMarcacion`
3. `RrhhAsistencia`

## Dónde buscar primero
- `MundoVs/Components/Pages/RRHH/`
- `MundoVs/Core/Entities/Empleado.cs`
- `MundoVs/Core/Entities/EsquemaPago.cs`
- `MundoVs/Core/Entities/EsquemaPagoTarifa.cs`
- `MundoVs/Core/Entities/EmpleadoEsquemaPago.cs`
- `MundoVs/Core/Entities/ValeDestajo.cs`
- `MundoVs/Core/Entities/ValeDestajoDetalle.cs`
- `MundoVs/Core/Entities/Prenomina.cs`
- `MundoVs/Core/Entities/PrenominaDetalle.cs`
- `MundoVs/Core/Entities/Nomina.cs`
- `MundoVs/Core/Entities/NominaDetalle.cs`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`

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
  - validación de IMSS.

Referencia técnica:
- `PrenominaDetalle.cs`
- `Prenominas.razor`

### 7. Nómina
- la nómina exige una prenómina cerrada del mismo período,
- al sincronizar:
  - toma incidencias desde `PrenominaDetalle`,
  - toma destajo real desde `ValesDestajo` aprobados,
  - suma `TotalPiezas` y `MontoDestajo`,
  - liga cada vale al `NominaDetalle`,
  - cambia el vale a `EnNomina`.

Referencia técnica:
- `Nominas.razor` → `GuardarNomina()`
- `Nominas.razor` → `SincronizarNominaAsync(...)`

## Observaciones funcionales vigentes

### Lo que ya sí funciona
- la cotización sí alimenta el tiempo estándar por proceso,
- el destajo sí llega a nómina desde vales aprobados,
- la prenómina sí queda ligada a la nómina por `PrenominaId`,
- el cálculo de nómina ya combina incidencias del período + destajo real.

### Huecos o diferencias respecto al objetivo funcional
1. `RegistroDestajoProceso` sigue siendo parte central del flujo operativo de producción; todavía no está completamente sustituido por `ValeDestajo`.
2. En `ValesDestajo.razor`, para resolver tarifa hoy se requiere pedido; aunque la propuesta funcional habla de pedido opcional, la implementación actual devuelve error de resolución si no hay pedido seleccionado.
3. En `PedidoSeguimiento.razor`, la tarifa del registro sigue siendo editable manualmente en pantalla; eso mantiene un riesgo de captura fuera de la lógica auditada.

## Resumen operativo
- `Cotización` define tiempos de mano de obra por proceso.
- `Pedido` hereda esa cotización seleccionada.
- `Seguimiento` registra producción por talla/proceso/empleado.
- `ValeDestajo` agrupa y formaliza el destajo del período.
- `Prenómina` concentra incidencias laborales.
- `Nómina` consolida incidencias + vales aprobados para calcular el pago final.
