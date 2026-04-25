# Checklist técnico — `RRHH` y `Nómina`

## Objetivo
Checklist ejecutable para implementar por etapas el módulo de `RRHH` y `Nómina`, separando lo obligatorio legal, lo operativo mínimo y lo deseable.

---

# 1. `Empleado`

## Obligatorio legal
- [ ] Agregar `TipoJornada` en `Empleado`
- [ ] Agregar `ZonaSalarioMinimo` o bandera de zona fronteriza
- [ ] Agregar `AplicaInfonavit`
- [ ] Agregar `NumeroCreditoInfonavit`
- [ ] Agregar `FactorDescuentoInfonavit`
- [ ] Validar que `FechaContratacion` sea obligatoria para cálculo de vacaciones, antigüedad y SDI

## Operativo mínimo
- [x] Mantener `TurnoBaseId`
- [x] Mantener `CodigoChecador`
- [ ] Mostrar `CodigoChecador` en `RRHH/Empleados.razor`
- [ ] Mostrar datos laborales clave en `RRHH/Empleados.razor`
- [ ] Permitir filtrar empleados activos e inactivos

## Deseable
- [ ] Crear historial `rrhh_empleado_turno`
- [ ] Agregar `FechaBaja`
- [ ] Agregar `EstatusLaboral`
- [ ] Agregar `TipoContrato`
- [ ] Cachear `SalarioDiarioIntegrado`
- [ ] Cachear `SalarioBaseCotizacion`

---

# 2. `Marcajes`

## Obligatorio legal
- [ ] Garantizar trazabilidad de marcación cruda por empleado, fecha y origen
- [ ] Mantener bitácora técnica por checador

## Operativo mínimo
- [x] Crear `rrhh_checador`
- [x] Crear `rrhh_marcacion`
- [x] Crear `rrhh_asistencia`
- [x] Crear `rrhh_logchecador`
- [x] Crear endpoint `POST /api/rrhh/marcaciones/sync`
- [x] Leer ZKTeco desde `Zenith.Workers.Asistencia`
- [ ] Crear pantalla `RRHH/Checadores.razor`
- [ ] Crear pantalla `RRHH/Marcaciones.razor`
- [ ] Crear pantalla `RRHH/Asistencias.razor`
- [ ] Procesar `rrhh_marcacion` a `rrhh_asistencia`
- [ ] Relacionar marcaciones con `Empleado` por `CodigoChecador`
- [ ] Actualizar navegación de RRHH para checadores, marcaciones y asistencias

## Deseable
- [ ] Persistencia local del worker con `SQLite`
- [ ] Cola offline del worker
- [ ] Checkpoint real por checador
- [ ] Reintentos automáticos de sincronización
- [ ] Detección de marcas duplicadas por evento externo además de hash
- [ ] Revisión manual de asistencia procesada

---

# 3. `Vacaciones y permisos`

## Obligatorio legal
- [ ] Crear control de saldo vacacional por antigüedad
- [ ] Registrar vacaciones disfrutadas por período
- [ ] Distinguir vacaciones, faltas, incapacidades y permisos

## Operativo mínimo
- [ ] Crear `rrhh_vacacion`
- [ ] Crear `rrhh_permiso`
- [ ] Agregar tipos de permiso base:
  - [ ] `Vacacion`
  - [ ] `PermisoConGoce`
  - [ ] `PermisoSinGoce`
  - [ ] `Incapacidad`
  - [ ] `FaltaJustificada`
  - [ ] `Licencia`
- [ ] Crear pantalla `RRHH/Vacaciones.razor`
- [ ] Crear pantalla `RRHH/Permisos.razor`
- [ ] Consolidar vacaciones y permisos al cargar la prenómina
- [ ] Evitar que `Prenomina` sea la captura maestra de vacaciones y permisos

## Deseable
- [ ] Flujo de autorización de vacaciones
- [ ] Historial de saldos por año
- [ ] Adjuntar comprobantes de incapacidad o permiso
- [ ] Alertas por saldo vacacional vencido

---

# 4. `Horas extra`

## Obligatorio legal
- [ ] Separar horas extra dobles y triples
- [ ] Aplicar regla legal:
  - [ ] primeras 9 horas semanales al doble
  - [ ] excedente al triple
- [ ] Evitar cálculo con un solo factor global

## Operativo mínimo
- [ ] Agregar en `PrenominaDetalle`:
  - [ ] `HorasExtraDobles`
  - [ ] `HorasExtraTriples`
  - [ ] `HorasExtraAutorizadas`
  - [ ] `OrigenHorasExtra`
- [ ] Agregar en `NominaDetalle`:
  - [ ] `HorasExtraDobles`
  - [ ] `HorasExtraTriples`
  - [ ] `MontoHorasExtraDobles`
  - [ ] `MontoHorasExtraTriples`
- [ ] Agregar configuración en `NominaConfiguracion`:
  - [ ] `FactorHoraExtraDoble`
  - [ ] `FactorHoraExtraTriple`
  - [ ] `MaxHorasExtraDoblesSemana`
- [ ] Ajustar `Nominas.razor` para captura y visualización separada

## Deseable
- [ ] Sugerir horas extra desde `rrhh_asistencia`
- [ ] Requerir autorización para horas extra
- [ ] Alertar excedentes fuera de ley

---

# 5. `Procesar nómina`

## Obligatorio legal
- [ ] Implementar tablas ISR por periodicidad
- [ ] Implementar subsidio al empleo
- [ ] Calcular ingreso gravable y exento
- [ ] Implementar SDI y SBC completos
- [ ] Aplicar topes de UMA
- [ ] Aplicar salario mínimo base cuando corresponda
- [ ] Implementar festivos y prima dominical
- [ ] Implementar INFONAVIT si aplica
- [ ] Evaluar CFDI nómina si el sistema también timbrará

## Operativo mínimo
- [ ] Crear `rrhh_nomina_concepto`
- [ ] Crear `rrhh_nomina_detalle_concepto`
- [ ] Agregar conceptos manuales:
  - [ ] bono manual
  - [ ] ajuste manual
  - [ ] otro ingreso manual
  - [ ] otro descuento manual
- [ ] Crear servicio `NominaCalculator`
- [ ] Separar cálculo por capas:
  - [ ] sueldo base
  - [ ] incidencias
  - [ ] vacaciones
  - [ ] horas extra
  - [ ] percepciones manuales
  - [ ] destajo
  - [ ] ISR
  - [ ] IMSS
  - [ ] INFONAVIT
  - [ ] neto final
- [ ] Ajustar `Nominas.razor` para capturar conceptos manuales con catálogo

## Deseable
- [ ] Simulación previa al cierre
- [ ] Aprobación de nómina por estatus
- [ ] Bitácora de cambios por detalle de nómina
- [ ] Exportación detallada por empleado

---

# 6. Priorización sugerida

## Sprint 1
- [ ] Cerrar bloque `Empleado`
- [ ] Crear `RRHH/Checadores.razor`
- [ ] Crear `RRHH/Marcaciones.razor`
- [ ] Crear `RRHH/Asistencias.razor`

## Sprint 2
- [ ] Crear `rrhh_vacacion`
- [ ] Crear `rrhh_permiso`
- [ ] Crear UI de vacaciones y permisos
- [ ] Consolidar vacaciones y permisos a prenómina

## Sprint 3
- [ ] Separar horas extra dobles y triples
- [ ] Agregar festivos y descanso trabajado
- [ ] Ajustar prenómina y nómina para horas extra legales

## Sprint 4
- [ ] Crear catálogo de conceptos de nómina
- [ ] Agregar percepciones y deducciones manuales estructuradas
- [ ] Crear `NominaCalculator`

## Sprint 5
- [ ] Implementar ISR
- [ ] Implementar subsidio al empleo
- [ ] Implementar INFONAVIT
- [ ] Cerrar cumplimiento legal mínimo

---

# 7. Estado actual resumido

## Ya implementado
- [x] Empleados base
- [x] Turnos base
- [x] Prenómina base
- [x] Nómina base
- [x] Destajo
- [x] Bono manual simple
- [x] Deducción manual simple
- [x] IMSS básico
- [x] Backend de checadores, marcaciones y asistencia
- [x] Worker de sincronización base

## Pendiente crítico
- [ ] UI de checadores, marcaciones y asistencias
- [ ] Vacaciones y permisos formales
- [ ] Horas extra dobles y triples
- [ ] ISR
- [ ] Subsidio al empleo
- [ ] INFONAVIT
- [ ] Festivos y prima dominical
- [ ] Motor de nómina estructurado
