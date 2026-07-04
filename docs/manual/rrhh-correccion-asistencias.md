# RRHH — Corrección de Asistencias y Resolución de Tiempo Extra

## Índice
- [Arquitectura del módulo](#arquitectura-del-módulo)
- [Secuencia de cálculo de bloques](#secuencia-de-cálculo-de-bloques)
- [Alternancia trabajo/descanso](#alternancia-trabajodescanso)
- [Resolución de tiempo extra](#resolución-de-tiempo-extra)
- [Caso sin turno](#caso-sin-turno)
- [Snapshot de prenómina](#snapshot-de-prenómina)
- [Cálculo de nómina](#cálculo-de-nómina)
- [Prevención de duplicación de tiempo visible](#prevención-de-duplicación-de-tiempo-visible)
- [Reglas funcionales clave](#reglas-funcionales-clave)

---

## Arquitectura del módulo

### Componentes principales

| Componente | Archivo | Responsabilidad |
|---|---|---|
| `RrhhAsistenciaProcessor` | `Core/Services/RrhhAsistenciaProcessor.cs` | Procesa marcaciones, calcula jornada, descansos y tiempo extra |
| `RrhhTiempoExtraResolutionService` | `Core/Services/RrhhTiempoExtraResolutionService.cs` | Aplica resoluciones de tiempo extra (pago/banco), permisos y compensaciones |
| `RrhhPrenominaSnapshotService` | `Core/Services/RrhhPrenominaSnapshotService.cs` | Construye snapshot de prenómina consolidando asistencias, ausencias y banco |
| `NominaCalculator` | `Core/Services/NominaCalculator.cs` | Calcula monto de horas extra, sueldo, ISR, IMSS y provisiones |
| `RrhhTiempoExtraPolicy` | `Core/Services/RrhhTiempoExtraPolicy.cs` | Políticas estáticas: minutos visibles, faltante, resolubles |
| `AsistenciasCorreccionModal` | `Components/Pages/RRHH/AsistenciasCorreccionModal.razor.cs` + partials | UI de corrección de asistencias |

### Partial classes del modal de corrección

| Archivo | Responsabilidad |
|---|---|
| `.razor.cs` | Lifecycle, inicialización, contexto del día, cierre, bitácora |
| `.Timeline.cs` | Clasificación y edición de segmentos, reconciliación, fórmulas de descanso |
| `.ResolucionTiempo.cs` | Resolución de tiempo extra, banco de horas, modos de sugerencia |
| `.Permisos.cs` | Permisos parciales, compensaciones, ausencias |
| `.Marcaciones.cs` | Marcaciones manuales, clasificación, anulación, turnos, descansos |
| `.Helpers.cs` | Formato, zona horaria, métricas, resumen visual, asesor, reglas |

---

## Secuencia de cálculo de bloques

El procesador (`AnalizarJornada`) sigue esta secuencia estricta:

1. **Jornada programada**: `salidaProgramada - entradaProgramada` → `MinutosJornadaProgramada`
2. **Descansos programados**: Suma de descansos configurados → `MinutosDescansoProgramado`
3. **Jornada neta programada**: `jornadaProgramada - descansosNoPagados` → `MinutosJornadaNetaProgramada`
4. **Selección de jornada principal**: `SeleccionarJornadaPrincipal` elige el par entrada/salida con mayor traslape con el turno configurado
5. **Bloques previos/posteriores**: Marcaciones fuera de la jornada principal → `minutosTrabajoAdicionalAntes/Despues`
6. **Validación de trabajo adicional**: Solo es válido si es automático (par de Entrada/Salida) y no hay retardo ni salida anticipada
7. **Minutos trabajados brutos**: `(salida - entrada) + adicionalAntes + adicionalDespues`
8. **Marcaciones intermedias**: Entre entrada y salida principal
9. **Detección de cortes internos**: Cortes antes de entrada programada o después de salida programada → bloquean extra automático
10. **Extracción de segmentos especiales**: `ExtraerSegmentosEspeciales` identifica salida temporal, permiso, no considerar, extra manual
11. **Exclusión de trabajo manual**: Segmentos con resolución manual de trabajo/extra se excluyen de descansos
12. **Clasificación de descansos**: Manual (por clasificación operativa) o por alternancia
13. **Cálculo de descansos aplicados**: `CalcularDescansosAplicados` aplica tolerancia, programado vs real, override
14. **Minutos trabajados netos**: `brutos - noPagados - salidaTemporal - permiso - noConsiderados`
15. **Cálculo de tiempo extra**: Según modo seleccionado (ver abajo)

---

## Alternancia trabajo/descanso

### Regla base

Los **pares de marcas intermedias** (entre la entrada y salida principal) representan siempre **salida-regreso del puesto**, es decir, **descansos**. El trabajo está implícito entre los pares.

**No hay alternancia pausa-trabajo entre pares**: todos los pares intermedios son descanso.

Ejemplo con turno 8:00-17:00, descansos D1 (10:00-10:30) y D2 (14:00-14:30):

```
Entrada 8:00 ──trabajo── 10:00 │ D1 10:00-10:30 │ 10:30 ──trabajo── 14:00 │ D2 14:00-14:30 │ 14:30 ──trabajo── 17:00 Salida
```

Marcaciones: 8:00, 10:00, 10:30, 14:00, 14:30, 17:00

Marcas intermedias: 10:00, 10:30, 14:00, 14:30

Pares intermedios:
- Par 1 (10:00-10:30) → **descanso** (D1)
- Par 2 (14:00-14:30) → **descanso** (D2)

### Detección de conflictos (`DetectarConflictosAlternancia`)

Si un par intermedio **no es** pausa (descanso, permiso o salida temporal), se marca:
- `RequiereRevision = true`
- `BloquearTiempoExtraAutomatico = true`
- Se agrega observación al día

### Caso sin turno

Sin turno, el procesador asigna **todos los pares intermedios** como descansos. No hay alternancia porque no hay descansos configurados de referencia. El modal muestra todos los pares como "Descanso inferido".

---

## Resolución de tiempo extra

### Modos de cálculo

| Modo | Fórmula | Cuándo se usa |
|---|---|---|
| `EntradaSalida` (default) | `entradaAnticipada + salidaPosterior + extraManual` | Cuando hay turno definido |
| `NetoVsNeto` | `Max(0, netoTrabajado - netoEsperado) + extraManual` | Cuando el usuario lo selecciona para el día |
| `SinTurno` | `MinutosExtraManual` (normalmente 0) | Automático cuando no hay turno; el usuario decide el extra |

**Importante**: En modo `NetoVsNeto`, el `MinutosPerdonadosManual` **NO** se incluye en el neto trabajado. Solo se usa `MinutosTrabajadosNetos`.

**Modo `SinTurno`**: El procesador lo asigna automáticamente cuando no hay turno configurado. No se puede cambiar manualmente. El procesador no auto-detecta extra; todo el tiempo trabajado se considera normal. El usuario decide manualmente cuánto del tiempo trabajado es extra.

### Secuencia de resolución (`AplicarResolucionAsync`)

1. Cargar contexto del empleado (saldo banco, configuración)
2. Determinar `factorTiempoExtra`: override manual si existe, sino configuración
3. Determinar `factorAcumulacionBanco`: **siempre** de configuración (el override de pago NO afecta banco)
4. Calcular:
   - `pago = pagoBase × factorTiempoExtra`
   - `banco = bancoBase × factorAcumulacionBanco` (si banco habilitado)
5. Validar:
   - `pagoBase + bancoBase ≤ extraResoluble`
   - `cubiertoBanco ≤ faltante`
   - `cubiertoBanco ≤ saldoDisponible + banco`
   - `saldoFinal ≤ topeBanco`
6. Persistir en asistencia: `MinutosExtraAutorizadosPago = pagoBase`, `MinutosExtraAutorizadosBanco = bancoBase` (valores BASE, no factorados)
7. Crear movimientos de banco con valores FACTORADOS (`banco` minutos → horas)

### Regla crítica: override de factor solo afecta pago

El `FactorTiempoExtraOverride` que el usuario captura manualmente en el modal **solo aplica al cálculo de pago**. La acumulación del banco de horas siempre usa `BancoHorasFactorAcumulacion` de la configuración, independientemente del override.

---

## Snapshot de prenómina

### Secuencia (`ConstruirResumenAsistencia`)

1. Sumar `MinutosExtraAutorizadosPago` (base)
2. Sumar `MinutosExtraAutorizadosBanco` (base)
3. Sumar `MinutosCubiertosBancoHoras`
4. `AjustarBancoHorasSnapshot`:
   - Si banco deshabilitado → reconvertir banco a pago, cero consumo
   - Aplicar `BancoHorasFactorAcumulacion` a banco acumulado
   - Aplicar tope de banco: excedente se paga como extra
5. Sumar descansos (tomado, pagado, no pagado)
6. Sumar retardos y salidas anticipadas (efectivos con permiso aplicado)
7. Calcular faltante descontable
8. Determinar `FactorTiempoExtraOverride` del día más reciente

### Construcción por empleado (`ConstruirSnapshotEmpleado`)

1. Clasificar ausencias: vacaciones, incapacidad, con goce, sin goce
2. Calcular días trabajados y pagados
3. Obtener ciclo vacacional y días disponibles
4. Calcular saldo de banco actual
5. Determinar factor override del período
6. Construir snapshot item con todos los valores

---

## Cálculo de nómina

### Secuencia (`NominaCalculator.Calculate`)

1. Sueldo diario y base del período
2. Percepciones: sueldo, destajo, bono, festivo, descanso trabajado, prima dominical, prima vacacional
3. **Horas extra legales**: separar en dobles (max 9h/semana) y triples
4. **Monto horas extra**: `horasDobles × sueldoHora × factorDobles + horasTriples × sueldoHora × factorTriples`
   - `factorDobles = FactorPagoTiempoExtra > 0 ? FactorPagoTiempoExtra : FactorHoraExtra`
   - `factorTriples = FactorPagoTiempoExtra > 0 ? FactorPagoTiempoExtra : FactorHoraExtraTriple`
5. Descuento por minutos (retardos, salidas anticipadas, faltante descontable)
6. Cuotas IMSS, INFONAVIT, ISR, subsidio
7. Provisiones (aguinaldo, prima vacacional)
8. Total a pagar y costo empresa

---

## Caso sin turno

Cuando un empleado no tiene turno asignado (`TurnoBaseId == null`):

1. **Clasificación**: Si hay 2+ marcaciones con entrada y salida, se paga como `AsistenciaNormal` sin requerir revisión
2. **Modo de cálculo**: El procesador asigna automáticamente `ModoSugerenciaExtra = "SinTurno"`. No se puede cambiar manualmente.
3. **Tiempo extra**: NO se auto-detecta tiempo extra. Todo el tiempo trabajado se considera normal. El usuario decide manualmente cuánto del tiempo trabajado es extra.
4. **Jornada neta**: `MinutosJornadaNetaProgramada = 0` (sin turno no hay jornada programada)
5. **Tiempo visible base**: `Max(0, MinutosTrabajadosNetosEfectivos - ExtraAprobado)` — se resta el extra aprobado para no duplicar
6. **Apartado de tiempo extra**: Siempre visible cuando hay tiempo trabajado, para que el usuario pueda aprobar extra

### Cálculo sin turno

```
MinutosExtra = 0  (procesador no auto-detecta)
```

El procesador asigna `ModoSugerenciaExtra = "SinTurno"` y `MinutosExtra = 0`.

### Validación de resolución sin turno

El servicio de resolución (`AplicarResolucionAsync`) valida `pagoBase + bancoBase ≤ extraResoluble`. Sin turno, `extraResoluble = MinutosTrabajadosNetos` (todo el tiempo trabajado), así que el usuario puede aprobar hasta el total trabajado.

### Extra aprobado sin turno

```
ExtraAprobado = MinutosExtraAutorizadosPago + MinutosExtraAutorizadosBanco
```

Sin turno, el extra aprobado **no se limita** por `MinutosExtra` (que es 0). El usuario aprueba lo que considere extra.

### Flujo del usuario sin turno

1. El empleado marca entrada y salida (ej. 8:50 a 18:41 = 9:51h trabajadas)
2. El procesador registra todo como tiempo normal (`MinutosExtra = 0`, `ModoSugerenciaExtra = "SinTurno"`)
3. El tiempo visible = tiempo trabajado (9:51h)
4. El apartado de "Tiempo extra" **siempre está visible** sin turno
5. El modal muestra el modo "Sin turno: el usuario decide el extra" (deshabilitado, no se puede cambiar)
6. La sugerencia automática es el excedente sobre 8h (480 min): 1:51h
7. El usuario captura cuánto quiere aprobar como extra (ej. "Pagar 2h" = 120 min)
8. El botón "Aplicar" se habilita si `120 ≤ 591` (MinutosTrabajadosNetos)
9. Se persiste: `MinutosExtraAutorizadosPago = 120`, `ResolucionTiempoExtra = "PagarTodo"`
10. El visible refleja: base (471 min = 7:51h normales) + extra aprobado (120 min = 2h) = 591 min (9:51h total)

### Cálculo de visibles sin turno

```
MinutosExtra = 0  (procesador no auto-detecta)
ExtraAprobado = MinutosExtraAutorizadosPago + MinutosExtraAutorizadosBanco  (sin limitar por detectados)
BaseVisibles = Max(0, MinutosTrabajadosNetos - ExtraAprobado)
visible = BaseVisibles + ExtraAprobado = MinutosTrabajadosNetos  (siempre coincide con el total trabajado)
```

**Ejemplo**: 9:51h trabajadas, usuario aprueba 2h como extra:
- `BaseVisibles = 591 - 120 = 471` (7:51h normales)
- `ExtraAprobado = 120` (2h extra)
- `visible = 471 + 120 = 591` (9:51h total, no duplica)

### Sugerencia sin turno

El modal sugiere el excedente sobre 8h (480 min) como referencia:
```
sugerido = Max(0, MinutosTrabajadosNetos - 480)
```

Pero el usuario puede capturar cualquier valor hasta el total trabajado.

---

## Prevención de duplicación de tiempo visible

El tiempo visible (`ObtenerMinutosTrabajadosVisibles`) se calcula así:

```
visible = BaseVisibles + permiso + compensacion + ExtraAprobado
```

### Con turno

`BaseVisibles = Min(netoEfectivo - extraDetectado, jornadaNetaProgramada)`

El extra detectado se **resta** del neto efectivo para evitar duplicación. Luego el extra aprobado se **suma** al visible. Si todo el extra detectado se aprueba:

```
visible = (netoEfectivo - extraDetectado) + extraDetectado = netoEfectivo
```

### Sin turno

`BaseVisibles = Max(0, MinutosTrabajadosNetos - ExtraAprobado)`

Sin turno, el extra detectado es 0 (el procesador no auto-detecta). En su lugar, se resta el **extra aprobado** por el usuario. Esto permite que el usuario "secciones" el tiempo: si aprueba 2h de extra sobre 10h trabajadas, el base visible = 8h y el visible total = 8h + 2h = 10h.

### Banco de horas

Los minutos enviados al banco (`MinutosExtraAutorizadosBanco`) están incluidos en `ExtraAprobado`:

```
ExtraAprobado = Min(pago + banco, extraDetectado)
```

El banco recibe esos minutos como horas acumuladas. En el visible, esos minutos ya están contados como tiempo trabajado (porque el empleado sí trabajó ese tiempo). No hay duplicación entre visible y banco: el visible refleja tiempo trabajado, el banco es un saldo acumulado para uso futuro.

### Nómina

En el cálculo de nómina:
- El **sueldo base** se paga por **días pagados**, no por horas trabajadas
- Las **horas extra** se pagan aparte con el factor (doble/triple)
- Las `HorasTrabajadasNetas` del snapshot son **informativas**, no se usan para calcular el sueldo
- No hay doble pago porque el sueldo por días y el monto de horas extra son conceptos independientes

---

## Reglas funcionales clave

1. **El override de factor de pago NO afecta la acumulación del banco de horas**
2. **El modo NetoVsNeto NO incluye perdón manual en el neto trabajado**
3. **Todos los pares de marcas intermedias son descansos**: no hay alternancia pausa-trabajo entre pares; el trabajo está implícito entre los descansos
4. **Los valores persistidos en la asistencia son BASE (sin factorar)**; el factor se aplica en cada capa según corresponda
5. **El banco deshabilitado convierte todo a pago**: los minutos autorizados para banco se suman al pago
6. **El tope de banco excedente se paga**: si al acumular se excede el tope, el excedente se paga como extra
7. **El faltante descontable**: `jornadaNetaProgramada - netoEfectivo`, reducido por permisos y compensaciones aprobadas
8. **Permiso parcial con goce descuenta banco de horas**; sin goce no lo consume
9. **Compensación aprobada** reduce faltante y permiso sugerido del día
10. **Salida anticipada puede cubrir descanso no marcado**: requiere confirmación en bitácora
11. **Sin turno, modo `SinTurno` asignado automáticamente**: no se puede cambiar manualmente; el procesador no auto-detecta extra
12. **Sin turno, el usuario decide el extra**: puede aprobar hasta el total trabajado como tiempo extra (pago, banco o ambos)
13. **Sin turno, el extra aprobado no se limita por detectados**: `ObtenerMinutosExtraAprobados` sin turno retorna el valor aprobado directamente
14. **Sin turno, el apartado de tiempo extra siempre está visible** cuando hay tiempo trabajado
15. **El tiempo visible nunca duplica extra**: `BaseVisibles` siempre resta el extra (detectado con turno, aprobado sin turno) antes de sumar `ExtraAprobado`
16. **`SaveChangesAsync` antes de reprocesar**: el modal guarda cambios pendientes antes de llamar al procesador para que vea las marcaciones actualizadas
17. **Reprocesamiento al abrir el modal**: el modal reproduce silenciosamente al abrir para corregir valores obsoletos de la BD