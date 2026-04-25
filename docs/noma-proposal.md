# Propuesta técnica por etapas — `RRHH` y `Nómina`

## Objetivo
Definir una ruta de implementación por partes para que una empresa pueda operar solo con los módulos de `RRHH` y `Nómina`, priorizando primero la base operativa y después el cumplimiento legal completo conforme al marco mexicano indicado en el repositorio.

---

## Enfoque propuesto
Trabajar el módulo en 5 bloques funcionales:

1. `Empleado`
2. `Marcajes`
3. `Vacaciones y permisos`
4. `Horas extra`
5. `Procesar nómina`

Este orden permite construir primero el dato maestro, después la operación diaria, luego las incidencias y al final el cálculo formal de nómina.

---

# 1. `Empleado`

## Lo que ya existe
Base actual en:
- `MundoVs/Core/Entities/Empleado.cs`
- `MundoVs/Components/Pages/RRHH/Empleados.razor`

Ya maneja:
- datos generales,
- `PeriodicidadPago`,
- `TipoNomina`,
- `AplicaImss`,
- `SueldoSemanal`,
- `TurnoBaseId`,
- `CodigoChecador`,
- esquema de pago vigente.

## Lo que falta
Para una nómina más sólida todavía faltaría agregar o formalizar:

### Identificación laboral
- `FechaBaja`
- `EstatusLaboral`
- `TipoContrato`
- `TipoJornada`
- `RegistroPatronalAplicable` opcional
- `ZonaSalarioMinimo` o bandera de zona fronteriza

### IMSS y fiscal
- `AplicaInfonavit`
- `NumeroCreditoInfonavit`
- `FactorDescuentoInfonavit`
- `SalarioDiarioIntegrado` calculado o cacheado
- `SalarioBaseCotizacion` calculado o cacheado

### Operación de asistencia
- historial de turno, no solo `TurnoBaseId`
- ideal futuro:
  - `rrhh_empleado_turno`

## Propuesta técnica
### Fase inicial
Mantener `Empleado` como maestro principal y agregar solo lo crítico:
- `TipoJornada`
- `AplicaInfonavit`
- `NumeroCreditoInfonavit`
- `FactorDescuentoInfonavit`
- `ZonaSalarioMinimo`

### Fase siguiente
Crear historial de turnos:
- `rrhh_empleado_turno`
  - `EmpleadoId`
  - `TurnoBaseId`
  - `VigenteDesde`
  - `VigenteHasta`

Eso permitirá:
- cambios de turno por fecha,
- recalcular asistencia histórica correctamente.

---

# 2. `Marcajes`

## Lo que ya existe
Ya quedó base técnica de backend:

### En el ERP
- `RrhhChecador`
- `RrhhMarcacion`
- `RrhhAsistencia`
- `RrhhLogChecador`
- endpoint `POST /api/rrhh/marcaciones/sync`

### En el worker
- `Zenith.Workers.Asistencia`
- lectura base por `ZkTecoMarcacionReader`
- envío por `HttpMarcacionSyncClient`
- orquestación en `AsistenciaSyncService`

## Lo que falta
### En el ERP
- pantalla de checadores,
- pantalla de marcaciones crudas,
- pantalla de asistencias,
- proceso que convierta `rrhh_marcacion` en `rrhh_asistencia`.

### En el worker
- persistencia local/offline,
- checkpoint real por checador,
- evitar reprocesar todo el log siempre,
- reintentos controlados.

## Propuesta técnica

### Pantallas
Crear:

#### `RRHH/Checadores.razor`
- alta/edición de `rrhh_checador`
- IP, puerto, número de máquina, ubicación, activo

#### `RRHH/Marcaciones.razor`
- vista cruda
- filtros por fecha, checador, empleado y código
- estatus:
  - sin empleado,
  - duplicada,
  - pendiente,
  - procesada

#### `RRHH/Asistencias.razor`
- por empleado y fecha
- entrada/salida programada vs real
- retardo
- extra
- revisión manual

### Procesamiento
Agregar un servicio en `MundoVs`, por ejemplo:
- `RrhhAsistenciaProcessor`

Responsabilidad:
- agrupar marcaciones por `EmpleadoId` y fecha local,
- tomar turno del día,
- clasificar:
  - entrada,
  - salida,
  - incompleta,
  - descanso trabajado,
  - turno no asignado.

### Persistencia local del worker
Más adelante crear `SQLite` local con algo tipo:
- `worker_marcacion`
- `worker_sync_log`
- `worker_checkpoint`

Ese bloque conviene dejarlo para otra fase.

---

# 3. `Vacaciones y permisos`

## Lo que ya existe
En prenómina ya hay base en:
- `Prenomina`
- `PrenominaDetalle`
- `Prenominas.razor`

Ya contempla:
- `DiasVacaciones`
- faltas
- incapacidades
- días trabajados
- vacaciones disponibles y restantes

También existe configuración base en:
- `NominaConfiguracion`
- `ConfiguracionNomina.razor`

## Lo que falta
### Vacaciones
No existe todavía un flujo completo de solicitud, autorización y saldo histórico.

Falta:
- bolsa anual por empleado,
- movimientos de vacaciones,
- permisos y justificaciones,
- desglose por evento.

### Permisos
No se ve una entidad formal para:
- permiso con goce,
- permiso sin goce,
- incapacidad documentada,
- licencia,
- falta justificada.

## Propuesta técnica
Crear:

### `rrhh_vacacion`
Cabecera de evento de vacaciones
- `EmpresaId`
- `EmpleadoId`
- `FechaInicio`
- `FechaFin`
- `Dias`
- `Estatus`
- `Observaciones`

### `rrhh_permiso`
Para permisos e incidencias manuales
- `EmpresaId`
- `EmpleadoId`
- `TipoPermiso`
- `FechaInicio`
- `FechaFin`
- `ConGoce`
- `AfectaPrenomina`
- `Observaciones`

### Catálogo o enum de tipos
- `Vacacion`
- `PermisoConGoce`
- `PermisoSinGoce`
- `Incapacidad`
- `FaltaJustificada`
- `Licencia`

## Integración con prenómina
La `Prenomina` no debería ser la captura maestra de vacaciones y permisos.
Debe ser el **resultado consolidado** del período.

Flujo recomendado:
1. capturar vacaciones y permisos en sus módulos,
2. al cargar prenómina, consolidar eventos del período,
3. permitir ajuste manual controlado si hace falta.

---

# 4. `Horas extra`

## Lo que ya existe
Base actual:
- `PrenominaDetalle.HorasExtra`
- `NominaDetalle.HorasExtra`
- `NominaDetalle.MontoHorasExtra`
- configuración en `NominaConfiguracion`
- UI en `Nominas.razor`

## Lo que falta
Conforme a ley, falta cerrar bien:

### Regla legal
Según `docs/conocimiento/nomina-marco-legal.md`:
- primeras 9 horas semanales: doble
- excedente: triple
- no solo un factor único

Hoy el modelo parece manejar un solo factor general.

### Relación con asistencia
Las horas extra deberían salir preferentemente de:
- turno programado,
- salida real,
- asistencia procesada,
no solo captura manual libre.

## Propuesta técnica

### Modelo
En lugar de solo `HorasExtra`, separar:

En `PrenominaDetalle`:
- `HorasExtraDobles`
- `HorasExtraTriples`
- `HorasExtraAutorizadas`
- `OrigenHorasExtra`

En `NominaDetalle`:
- `HorasExtraDobles`
- `HorasExtraTriples`
- `MontoHorasExtraDobles`
- `MontoHorasExtraTriples`

### Configuración
En `NominaConfiguracion`:
- `FactorHoraExtraDoble = 2`
- `FactorHoraExtraTriple = 3`
- `MaxHorasExtraDoblesSemana = 9`

### Origen
Primera fase:
- captura manual validada

Segunda fase:
- sugerencia automática desde `rrhh_asistencia`

---

# 5. `Procesar nómina`

## Lo que ya existe
Base actual:
- `Nomina`
- `NominaDetalle`
- `Nominas.razor`
- `Prenomina`
- `PrenominaDetalle`
- cálculo de sueldo base
- destajo
- prima vacacional
- IMSS básico
- bonos
- deducciones

## Lo que falta
Aquí está la parte más importante.

### Legal crítico
#### ISR
Falta implementar:
- tablas ISR por periodicidad
- subsidio al empleo
- ingreso gravable vs exento

#### INFONAVIT
Falta:
- aportación patronal
- descuento de crédito si aplica

#### SDI / SBC
Falta estructura más completa y trazable:
- aguinaldo proporcional
- prima vacacional proporcional
- topes UMA
- mínimo salario

#### Festivos / domingos
Falta:
- catálogo oficial
- prima dominical
- festivo trabajado

#### CFDI nómina
No se ve flujo de timbrado de nómina todavía.

### Funcional
Falta formalizar conceptos manuales.

#### Hoy existe
- `Bonos`
- `Deducciones`

#### Falta
Percepciones y deducciones con concepto explícito:
- bono manual
- ajuste manual
- premio
- ayuda
- descuento manual
- préstamo
- otros

## Propuesta técnica

### Modelo de conceptos
Crear:

#### `rrhh_nomina_concepto`
Catálogo
- `Codigo`
- `Nombre`
- `Tipo`
- `Naturaleza`
- `GravaIsr`
- `IntegraSdi`
- `EsManual`

#### `rrhh_nomina_detalle_concepto`
Detalle por empleado y período
- `NominaDetalleId`
- `ConceptoId`
- `Importe`
- `Cantidad`
- `Exento`
- `Gravado`
- `Observaciones`

Esto reemplaza la lógica plana de solo:
- `Bonos`
- `Deducciones`

y permite:
- bono manual,
- ajuste manual,
- otro ingreso manual,
- otro descuento manual.

### Motor de cálculo
Separar el cálculo en un servicio, por ejemplo:
- `NominaCalculator`

Capas:
1. sueldo base
2. incidencias
3. vacaciones y prima vacacional
4. horas extra
5. percepciones manuales
6. destajo
7. ISR
8. IMSS
9. INFONAVIT
10. neto final

### Flujo recomendado
1. consolidar `Prenomina`
2. cargar conceptos manuales
3. calcular percepciones
4. calcular deducciones
5. calcular impuestos y cuotas
6. generar resumen
7. aprobar
8. opcional: CFDI

---

# Propuesta por etapas

## Etapa A — Base operativa mínima
1. `Empleado`
   - cerrar campos laborales mínimos
2. `Checadores`
   - pantalla de checadores
   - pantalla de marcaciones
   - pantalla de asistencias
3. `Vacaciones y permisos`
   - tablas y pantalla básica
4. `Horas extra`
   - separar doble y triple
5. `Procesar nómina`
   - conceptos manuales

## Etapa B — Cumplimiento legal mínimo
6. ISR
7. subsidio al empleo
8. SDI / SBC formal
9. festivos y prima dominical
10. INFONAVIT

## Etapa C — Automatización
11. marcajes a asistencia
12. asistencia a prenómina
13. prenómina a nómina sugerida

---

# Resumen ejecutivo

## Ya está
- empleados
- turnos
- prenómina
- nómina base
- destajo
- bono manual simple
- deducción manual simple
- IMSS básico
- backend de checadores, marcaciones y asistencia

## Falta
- UI de checadores, marcajes y asistencias
- vacaciones y permisos formales
- horas extra dobles y triples
- conceptos manuales formales
- ISR
- subsidio al empleo
- INFONAVIT
- festivos y prima dominical
- motor de nómina más estructurado

---

# Recomendación de arranque
Si se quiere hacerlo por partes, el mejor orden es:

1. `Empleado`
2. `Marcajes`
3. `Vacaciones y permisos`
4. `Horas extra`
5. `Procesar nómina`

Y dentro de eso:

## Sprint 1
- cerrar `Empleado`
- UI de `Checadores`
- UI de `Marcaciones`
- UI de `Asistencias`

## Sprint 2
- `Vacaciones`
- `Permisos`
- integración a prenómina

## Sprint 3
- horas extra doble y triple
- festivos
- descanso trabajado

## Sprint 4
- percepciones manuales y deducciones manuales con catálogo
- procesamiento de nómina más formal

## Sprint 5
- ISR
- subsidio
- INFONAVIT
- cierre legal mínimo
