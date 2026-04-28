# RRHH - Nómina técnica y mapa de reglas de cálculo

## Objetivo
Documentar cómo está implementado actualmente el flujo de nómina en `MundoVs`, qué componentes participan, qué datos gobiernan el cálculo y cómo se traducen las incidencias operativas a percepciones, deducciones y neto a pagar.

## Alcance actual
La implementación revisada cubre:
- carga de configuración de nómina por empresa y configuración maestra global;
- construcción de snapshot de prenómina desde asistencia, ausencias, esquemas de pago y vales de destajo;
- cálculo monetario de conceptos principales de nómina;
- persistencia de `Nomina` y `NominaDetalle`;
- armado del recibo con conceptos SAT base.

No se documenta exportación a JSON porque se descartó por solicitud del usuario.

## Componentes principales

### Servicios
- `MundoVs/Core/Services/NominaConfiguracionLoader.cs`
  - Resuelve la configuración efectiva de nómina.
  - Mezcla parámetros por empresa desde `AppConfig` con parámetros maestros desde `NominaConfiguracionGlobal`.
- `MundoVs/Core/Services/RrhhPrenominaSnapshotService.cs`
  - Construye el snapshot operativo del período.
  - Consolida asistencias, ausencias, destajo, esquemas de pago, banco de horas y sugerencias legales.
- `MundoVs/Core/Services/NominaLegalPolicyService.cs`
  - Centraliza reglas legales auxiliares: sueldo base del período, vacaciones disponibles y complemento a salario mínimo.
- `MundoVs/Core/Services/NominaCalculator.cs`
  - Ejecuta el cálculo monetario final de la nómina por empleado.
- `MundoVs/Core/Services/NominaReciboBuilder.cs`
  - Convierte `NominaDetalle` a percepciones y deducciones para recibo.

### Modelos y entidades clave
- `MundoVs/Core/Entities/NominaConfiguracion.cs`
- `MundoVs/Core/Entities/NominaConfiguracionGlobal.cs`
- `MundoVs/Core/Entities/Nomina.cs`
- `MundoVs/Core/Entities/NominaDetalle.cs`
- `MundoVs/Core/Interfaces/IRrhhPrenominaSnapshotService.cs`
- `MundoVs/Core/Services/NominaCalculationModels.cs`

### Pantallas de administración
- `MundoVs/Components/Pages/Admin/ConfiguracionNomina.razor`
  - Configuración por empresa.
- `MundoVs/Components/Pages/SuperAdmin/ConfiguracionNominaGlobal.razor`
  - Configuración maestra global para UMA, salarios mínimos, ISR y subsidio.

### Registro en DI
Los servicios de nómina se registran en `MundoVs/Program.cs`:
- `INominaCalculator`
- `INominaLegalPolicyService`
- `INominaReciboBuilder`
- `INominaResumenBuilder`
- `IRrhhPrenominaSnapshotService`
- `INominaSatCatalogInitializer`

## Persistencia y tablas involucradas

### Configuración maestra
- Tabla: `rrhh_nomina_configuracion_global`
- Entidad: `NominaConfiguracionGlobal`
- Uso: contiene valores globales compartidos entre empresas para:
  - `UmaDiaria`
  - `SalarioMinimoGeneral`
  - `SalarioMinimoFrontera`
  - `TablaIsrJson`
  - `TablaSubsidioJson`

### Configuración por empresa
- Fuente: `AppConfig`
- Uso: contiene factores operativos y reglas configurables por empresa.
- Claves principales:
  - `Nomina:FactorHoraExtra`
  - `Nomina:FactorHoraExtra:Triple`
  - `Nomina:FactorFestivoTrabajado`
  - `Nomina:FactorDescansoTrabajado`
  - `Nomina:BancoHoras:Habilitado`
  - `Nomina:BancoHoras:FactorAcumulacion`
  - `Nomina:BancoHoras:TopeHoras`
  - `Nomina:DiasBase:Semanal`
  - `Nomina:DiasBase:Quincenal`
  - `Nomina:DiasBase:Mensual`
  - `Nomina:HorasBase:Semanal`
  - `Nomina:HorasBase:Quincenal`
  - `Nomina:HorasBase:Mensual`
  - `Nomina:IMSS:TasaObrera`
  - `Nomina:IMSS:TasaPatronal`
  - `Nomina:IMSS:PrimaRiesgoTrabajo`
  - `Nomina:IMSS:TopeSbcEnUma`
  - `Nomina:PrimaVacacional:Minima`
  - `Nomina:Aguinaldo:DiasMinimos`
  - `Nomina:Vacaciones:TablaJson`
  - `Nomina:Prenomina:ReglasJson`
  - `Nomina:ISR:RetencionHabilitada`

### Resultado de nómina
- Entidad `Nomina`
  - por convención EF se almacena en `Nominas`;
  - agrupa período, folio, número de nómina, estatus y vínculo opcional con prenómina.
- Entidad `NominaDetalle`
  - por convención EF se almacena en `NominaDetalles`;
  - persiste conceptos calculados por empleado.
- Índices relevantes:
  - `Nomina`: único por `EmpresaId + Folio`;
  - `Nomina`: único por `EmpresaId + Periodo + NumeroNomina`;
  - `NominaDetalle`: índices por `NominaId` y `EmpleadoId`.

### Tablas RRHH relacionadas que alimentan el snapshot
- `Prenominas`
- `PrenominaDetalles`
- `RrhhAsistencias`
- `RrhhAusencias`
- `ValesDestajo`
- `ValesDestajoDetalle`
- `EmpleadosEsquemaPago`
- `rrhh_banco_horas_movimiento`
- `FestivosRrhh`

## Precedencia de configuración
La configuración efectiva se arma en `NominaConfiguracionLoader.Build(...)`.

### Regla de precedencia
1. Se leen valores por empresa desde `AppConfig`.
2. Se obtiene la configuración maestra activa más vigente desde `rrhh_nomina_configuracion_global`.
3. Se construye `NominaConfiguracion` final.
4. Para `UMA`, `SalarioMinimoGeneral`, `SalarioMinimoFrontera`, `TablaIsrJson` y `TablaSubsidioJson`, la configuración global tiene prioridad si viene informada.
5. El resto de factores operativos queda por empresa.

### Vigencia global
La selección de la configuración global activa se ordena por:
1. `UpdatedAt` descendente, o `CreatedAt` cuando no hay actualización;
2. `CreatedAt` descendente.

La prueba `MundoVs.Tests/NominaConfiguracionLoaderTests.cs` valida esta prioridad.

## Flujo técnico de punta a punta

### 1. Configuración
El usuario administra:
- configuración por empresa en `/configuracion/nomina`;
- configuración maestra en `/superadmin/configuracion-nomina`.

### 2. Snapshot de prenómina
`RrhhPrenominaSnapshotService` arma un snapshot por empleado del período:
- empleados activos;
- esquema de pago vigente;
- monto de destajo aprobado por vales;
- vacaciones usadas históricas;
- ausencias del período;
- resumen de asistencias y tiempo extra;
- festivos del período.

### 3. Derivación operativa previa al cálculo
Para cada empleado se determina:
- `DiasTrabajados`;
- `DiasPagados`;
- `DiasVacaciones`;
- faltas justificadas e injustificadas;
- `DiasDescansoTrabajado`;
- `DiasDomingoTrabajado`;
- `DiasFestivoTrabajado`;
- `HorasExtraBase`;
- `HorasExtra` pagables;
- horas a banco, horas consumidas y descansos;
- `ComplementoSalarioMinimoSugerido`;
- notas de revisión.

### 4. Cálculo monetario
`NominaCalculator.Calculate(...)` recibe `NominaCalculationInput` y devuelve `NominaCalculationResult`.

### 5. Persistencia
Los importes terminan en `NominaDetalle` y el total del documento se agrega en `Nomina`.

### 6. Recibo
`NominaReciboBuilder` transforma el detalle persistido en listas de percepciones y deducciones con claves SAT base.

## Mapa de reglas de cálculo

### 1. Sueldo diario
**Fuente:** `NominaCalculator`

**Fórmula**
- `diasBase = configuracion.ObtenerDiasBase(periodicidad)`
- `sueldoDiario = sueldoReferencia / diasBase`

**Notas**
- `diasBase` depende de semanal, quincenal o mensual.
- Se protege con mínimo de `1` para evitar división entre cero.

### 2. Sueldo base del período
**Fuente:** `NominaCalculator` y `NominaLegalPolicyService`

**Fórmula**
- `sueldoBase = sueldoDiario * diasPagados`

**Precedencia de origen**
1. `SueldoBaseOverride` si viene informado.
2. Sueldo sugerido del esquema de pago.
3. `empleado.SueldoSemanal` como referencia final.

**Regla adicional**
- Si el esquema de pago no incluye sueldo base y no es `SueldoFijo`, la política legal puede devolver `0` como sueldo base del período.

### 3. Destajo
**Fuente:** `RrhhPrenominaSnapshotService` + `NominaCalculator`

**Regla**
- El snapshot suma importes de `ValesDestajo` aprobados del período.
- El cálculo final usa ese monto como `MontoDestajo`.
- En `NominaDetalle` existe también `MontoDestajoLegacy`, pero es informativo y se ignora para el total real actual.

### 4. Bono
**Fuente:** `NominaCalculator` y `NominaReciboBuilder`

**Regla**
- `MontoBono` entra directo al cálculo.
- Si existen bonos estructurados, el recibo desglosa por rubro; si no, usa el bono agregado.

### 5. Festivo trabajado
**Fuente:** `NominaCalculator`

**Fórmula**
- `montoFestivoTrabajado = diasFestivoTrabajado * sueldoDiario * factorFestivo`

**Regla de factor**
- puede venir específico en el input;
- si no, debe usarse el factor general de configuración.

### 6. Descanso trabajado
**Fuente:** `NominaCalculator`

**Fórmula**
- `montoDescansoTrabajado = diasDescansoTrabajado * sueldoDiario * FactorDescansoTrabajado`

### 7. Prima dominical
**Fuente:** `NominaCalculator`

**Fórmula**
- `montoPrimaDominical = diasDomingoTrabajado * sueldoDiario * 0.25`

**Observación**
- La prima dominical se calcula aparte del descanso trabajado.

### 8. Prima vacacional pagada
**Fuente:** `NominaCalculator`

**Fórmula**
- `montoPrimaVacacional = sueldoDiario * diasVacaciones * PrimaVacacionalMinima`

**Política por empresa**
- `AlTomarVacaciones`: se paga en la nómina del período cuando existen días de vacaciones.
- `Anual` o `Manual`: no se paga automáticamente al tomar vacaciones; la provisión sigue acumulándose aparte.

### 8.1 Ciclo vacacional reconocido
**Fuente:** `NominaLegalPolicyService`

**Opciones configurables**
- `AniversarioContratacion`
- `CorteFijoAnual`

**Campos configurables por empresa**
- `MesInicioCicloVacacional`
- `DiaInicioCicloVacacional`
- `MesesMinimosPrimerAnioVacacional`
- `PoliticaPrimerCicloVacacional = Completo | Proporcional`

**Ejemplo base**
- fecha de ingreso: `28/04/2026`
- corte anual: `01/01`

#### Escenario A: corte `01/01`, mínimo `12` meses, primer ciclo `Completo`

| Período | Año reconocido | Días de vacaciones |
|---------|----------------|--------------------|
| 01/01/2027 - 31/12/2027 | Año 0 | 0 |
| 01/01/2028 - 31/12/2028 | Año 1 | 12 |
| 01/01/2029 - 31/12/2029 | Año 2 | 14 |
| 01/01/2030 - 31/12/2030 | Año 3 | 16 |

#### Escenario B: corte `01/01`, mínimo `6` meses, primer ciclo `Completo`

| Período | Año reconocido | Días de vacaciones |
|---------|----------------|--------------------|
| 01/01/2027 - 31/12/2027 | Año 1 | 12 |
| 01/01/2028 - 31/12/2028 | Año 2 | 14 |
| 01/01/2029 - 31/12/2029 | Año 3 | 16 |

#### Escenario C: corte `01/01`, mínimo `6` meses, primer ciclo `Proporcional`

| Período | Año reconocido | Días de vacaciones |
|---------|----------------|--------------------|
| 01/01/2027 - 31/12/2027 | Año 1 proporcional | ~8.15 |
| 01/01/2028 - 31/12/2028 | Año 2 | 14 |
| 01/01/2029 - 31/12/2029 | Año 3 | 16 |

**Observación**
- En modo `Proporcional`, el primer ciclo reconocido usa días equivalentes del año 1 según el tramo realmente trabajado antes del corte; esos días equivalentes impactan saldo, prima vacacional, provisión e integración a IMSS.

### 9. Horas extra legales
**Fuente:** `NominaCalculator`

**Regla de partición**
- si ya vienen `HorasExtraDobles` o `HorasExtraTriples`, se respetan;
- en caso contrario:
  - primeras `9` horas legales: dobles;
  - excedente: triples.

**Fórmula base**
- `sueldoHora = sueldoReferencia / horasBase`
- `montoDobles = horasDobles * sueldoHora * FactorHoraExtra`
- `montoTriples = horasTriples * sueldoHora * FactorHoraExtraTriple`
- `montoHorasExtra = montoDobles + montoTriples`

### 10. Banco de horas
**Fuente:** `RrhhPrenominaSnapshotService`

**Reglas**
- si el banco está deshabilitado:
  - el tiempo autorizado a banco se reconvierte a pago;
  - el consumo de banco queda en cero.
- si está habilitado:
  - se aplica `BancoHorasFactorAcumulacion`;
  - se aplica `BancoHorasTopeHoras`;
  - el excedente al tope se manda a pago, no a acumulación.

### 11. Descuento por minutos
**Fuente:** `NominaCalculator`

**Fórmula**
- `minutosJornadaBase = (horasBase / diasBase) * 60`
- `montoDescuentoMinutos = sueldoDiario * (minutosDescuento / minutosJornadaBase)`

**Origen del minuto a descontar**
- retardo;
- salida anticipada;
- descuento manual.

### 12. IMSS
**Fuente:** `NominaCalculator`

**Reglas previas**
- solo aplica si `AplicaImss = true`;
- requiere `sueldoDiario > 0` y `diasPagados > 0`.

**Construcción de SBC**
1. Se obtienen vacaciones por antigüedad.
2. Se calcula factor de aguinaldo proporcional diario.
3. Se calcula factor de vacaciones/prima vacacional proporcional diaria.
4. `sdi = sueldoDiario + factorAguinaldo + factorVacaciones`
5. Se topa por `TopeSbcEnUma * UmaDiaria`.
6. Se sube al salario mínimo aplicable si queda por debajo.

**Fórmulas**
- `basePeriodo = sbc * diasPagados`
- `obrera = basePeriodo * TasaImssObrera`
- `patronal = basePeriodo * (TasaImssPatronal + PrimaRiesgoTrabajo)`

### 13. Infonavit
**Fuente:** `NominaCalculator`

**Reglas**
- si `empleado.AplicaInfonavit` es falso o el factor es `<= 0`, no aplica.
- si `FactorDescuentoInfonavit <= 1`, se trata como porcentaje del sueldo base del período.
- si `FactorDescuentoInfonavit > 1`, se trata como monto fijo.

### 14. ISR y subsidio al empleo
**Fuente:** `NominaCalculator`

**Tabla default actual del repositorio**
- La tabla `ISR` default ya quedó actualizada a valores `2026` para pagos mensuales.
- El cálculo actual sigue tomando esa tabla mensual y la escala por factor para semanal y quincenal.
- Aún no usa directamente las tablas periódicas específicas del SAT para semanal, quincenal o diaria.

**Base gravable utilizada hoy**
Se suma:
- sueldo base;
- destajo;
- bono;
- festivo trabajado;
- descanso trabajado;
- prima dominical;
- prima vacacional;
- complemento salario mínimo;
- horas extra;
- percepciones manuales.

**Regla actual importante**
- el comentario del código indica que las percepciones manuales se asumen gravables; si alguna fuera exenta, hoy debe capturarse por otro tratamiento.

**Escalamiento de tarifa al período**
- semanal: factor `7/30`
- quincenal: factor `15/30`
- mensual: factor `30/30`

**Fórmula resumida**
1. localizar tramo ISR proporcional;
2. `isrCausado = cuotaFijaPeriodo + excedente * tasaExcedente`;
3. localizar tramo de subsidio proporcional;
4. si `isrCausado >= subsidio`, retener diferencia;
5. si `subsidio > isrCausado`, no retener ISR y pagar diferencia como subsidio al empleo.

### 15. Provisiones patronales
**Fuente:** `NominaCalculator`

**Aguinaldo proporcional**
- `(DiasAguinaldoMinimo * sueldoDiario / 365) * diasPagados`

**Prima vacacional proporcional**
- `(diasVacacionesAntiguedad * PrimaVacacionalMinima * sueldoDiario / 365) * diasPagados`

**Regla**
- estas provisiones no afectan el neto del trabajador ni el recibo.

### 16. Complemento a salario mínimo
**Fuente:** `NominaLegalPolicyService`

**Fórmula**
- `ingresoBasePeriodo = sueldoBasePeriodo + montoDestajo`
- `minimoPeriodo = salarioMinimoAplicable * diasPagados`
- `complemento = max(0, minimoPeriodo - ingresoBasePeriodo)`

### 17. Vacaciones por antigüedad
**Fuente:** `NominaConfiguracion`

**Regla**
- se usa `TablaVacacionesJson` si es válida;
- si no, se usa tabla default.

**Tabla default actual**
- año 1: `12`
- año 2: `14`
- año 3: `16`
- año 4: `18`
- años 5 a 9: `20`
- años 10 a 14: `22`
- años 15 a 19: `24`
- años 20 a 24: `26`

### 18. Total neto a pagar
**Fuente:** `NominaCalculator` y `NominaDetalle.TotalPagar`

**Fórmula actual**

Percepciones:
- sueldo base
- destajo
- bono
- festivo trabajado
- descanso trabajado
- prima dominical
- prima vacacional
- complemento salario mínimo
- horas extra
- percepciones manuales o bonos
- subsidio al empleo

Menos deducciones:
- deducciones manuales/estructuradas
- descuento por minutos
- IMSS obrero
- Infonavit
- ISR retenido

## Mapeo a recibo
`NominaReciboBuilder` traduce el detalle a conceptos visibles.

### Percepciones principales
- sueldo base -> `PercepcionSueldos`
- destajo -> `PercepcionComisiones`
- bono producción o bonos estructurados -> clave configurada o `PercepcionOtrosIngresosSalarios`
- festivo trabajado -> `PercepcionDescansoObligatorioLaborado`
- prima dominical -> `PercepcionPrimaDominical`
- prima vacacional -> `PercepcionPrimaVacacional`
- complemento salario mínimo -> `PercepcionOtrosIngresosSalarios`
- horas extra -> `PercepcionHorasExtra`
- subsidio al empleo -> `PercepcionOtrosIngresosSalarios`

### Deducciones principales
- ISR retenido -> `DeduccionIsr`
- IMSS obrero -> `DeduccionSeguridadSocial`
- Infonavit -> `DeduccionCreditoInfonavit`
- descuento por retardos/minutos -> `DeduccionAusentismo`
- deducciones manuales o estructuradas -> clave configurada o `DeduccionAusentismo`

## Reglas operativas de prenómina configurables
Se almacenan en `ReglasPrenominaJson` y se exponen en `ConfiguracionNomina.razor`.

### Banderas actuales
- `PermitirHorasExtraManual`
- `ValidarDiasPagadosContraPeriodo`
- `RequierePrenominaCerradaParaNomina`

## Supuestos relevantes del código actual
- La configuración maestra solo gobierna `UMA`, salarios mínimos, ISR y subsidio.
- El resto del comportamiento es por empresa.
- La base gravable ISR actual no distingue percepciones exentas en el cálculo base estándar.
- El banco de horas puede transformar tiempo autorizado a banco en tiempo pagable cuando está deshabilitado o topado.
- `NominaDetalle.TotalPagar` es propiedad calculada, no columna persistida.
- `Nomina.TotalNomina` también es calculado y se ignora en EF.

## Riesgos o puntos a vigilar
- `TablaIsrJson`, `TablaSubsidioJson` y `TablaVacacionesJson` son texto libre; si el JSON es inválido, el sistema cae a valores default.
- El comentario de ISR reconoce una simplificación: percepciones manuales se asumen gravables.
- El cálculo de horas extra legales usa un corte fijo de `9` horas; cualquier política distinta requeriría ajuste de código.
- La tabla default ISR/subsidio está anotada como vigente `2024`; debe revisarse al cambiar ejercicio fiscal.

## Referencias internas revisadas
- `MundoVs/Core/Services/NominaConfiguracionLoader.cs`
- `MundoVs/Core/Entities/NominaConfiguracion.cs`
- `MundoVs/Core/Entities/NominaConfiguracionGlobal.cs`
- `MundoVs/Core/Services/RrhhPrenominaSnapshotService.cs`
- `MundoVs/Core/Services/NominaLegalPolicyService.cs`
- `MundoVs/Core/Services/NominaCalculator.cs`
- `MundoVs/Core/Services/NominaReciboBuilder.cs`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
- `MundoVs/Components/Pages/Admin/ConfiguracionNomina.razor`
- `MundoVs/Components/Pages/SuperAdmin/ConfiguracionNominaGlobal.razor`
- `MundoVs.Tests/NominaConfiguracionLoaderTests.cs`
