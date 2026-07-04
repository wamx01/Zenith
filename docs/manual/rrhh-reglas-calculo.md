# RRHH — Reglas de Cálculo: Tiempo Visible, Tiempo Extra y Métodos

## Índice
1. [Campos de la asistencia](#campos-de-la-asistencia)
2. [Cálculo de bloques (procesador)](#cálculo-de-bloques-procesador)
3. [Cálculo de descansos](#cálculo-de-descansos)
4. [Tiempo trabajado neto](#tiempo-trabajado-neto)
5. [Tiempo extra: métodos de cálculo](#tiempo-extra-métodos-de-cálculo)
6. [Tiempo visible](#tiempo-visible)
7. [Resolución de tiempo extra](#resolución-de-tiempo-extra)
8. [Banco de horas](#banco-de-horas)
9. [Faltante y permiso](#faltante-y-permiso)
10. [Snapshot de prenómina](#snapshot-de-prenómina)
11. [Cálculo de nómina](#cálculo-de-nómina)
12. [Descuentos efectivos](#descuentos-efectivos)
13. [Matriz de reglas por modo](#matriz-de-reglas-por-modo)

---

## Campos de la asistencia

| Campo | Tipo | Origen | Descripción |
|---|---|---|---|
| `MinutosJornadaProgramada` | int | Procesador | `salidaProgramada − entradaProgramada` |
| `MinutosJornadaNetaProgramada` | int | Procesador | `jornadaProgramada − descansosNoPagados` |
| `MinutosTrabajadosBrutos` | int | Procesador | `(salidaReal − entradaReal) + adicionalAntes + adicionalDespues − margenNoComputable` |
| `MinutosTrabajadosNetos` | int | Procesador | `brutos − descansoNoPagado − salidaTemporal − permisoSegmento − noConsiderados` |
| `MinutosDescansoTomado` | int | Procesador | Suma de minutos aplicados de descansos |
| `MinutosDescansoPagado` | int | Procesador | Suma de descansos pagados |
| `MinutosDescansoNoPagado` | int | Procesador | `descansoTomado − descansoPagado` |
| `MinutosRetardo` | int | Procesador | `Max(0, entradaReal − entradaProgramada)` |
| `MinutosSalidaAnticipada` | int | Procesador | `Max(0, salidaProgramada − salidaReal)` |
| `MinutosExtra` | int | Procesador | Extra detectado (ver métodos de cálculo) |
| `MinutosPerdonadosManual` | int | Usuario | Minutos perdonados manualmente |
| `MinutosExtraAutorizadosPago` | int | Usuario/Resolución | Base aprobada para pago |
| `MinutosExtraAutorizadosBanco` | int | Usuario/Resolución | Base aprobada para banco |
| `MinutosCubiertosBancoHoras` | int | Usuario/Resolución | Minutos cubiertos desde banco |
| `FactorTiempoExtraAplicado` | decimal? | Usuario/Resolución | Override de factor persistido |
| `ModoSugerenciaExtra` | string? | Procesador/Usuario | `"EntradaSalida"`, `"NetoVsNeto"` o `"SinTurno"` |
| `ResolucionTiempoExtra` | string? | Usuario/Resolución | Tipo de resolución aplicada |
| `TurnoBaseId` | Guid? | Procesador | Null = sin turno |

---

## Cálculo de bloques (procesador)

### Secuencia estricta en `AnalizarJornada`

```
1. MinutosJornadaProgramada = Max(0, salidaProgramada − entradaProgramada)
2. MinutosDescansoProgramado = Suma descansos configurados
3. MinutosJornadaNetaProgramada = Max(0, jornadaProgramada − descansosNoPagados)
4. SeleccionarJornadaPrincipal → entrada/salida con mayor traslape con el turno
5. Marcas previas (antes de entrada) → minutosTrabajoAdicionalAntes
6. Marcas posteriores (después de salida) → minutosTrabajoAdicionalDespues
7. Validar trabajo adicional automático (par Entrada/Salida, sin retardo/salida anticipada)
8. Bloquear trabajo adicional si hay retardo o salida anticipada
9. MinutosTrabajadosBrutos = (salida−entrada) + adicionalAntes + adicionalDespues
10. Extraer segmentos especiales (temporal, permiso, ignorar, extra manual)
11. Excluir segmentos trabajo manual (no se toman como descanso)
12. Clasificar descansos: manual (clasificación operativa) o por pares alternados
13. CalcularDescansosAplicados: tolerancia, programado vs real, override
14. MinutosTrabajadosNetos = Max(0, brutos − noPagado − temporal − permiso − noConsiderados)
15. CalcularMinutosExtra según modo (ver abajo)
```

### Selección de jornada principal

Elige el par entrada/salida con:
- Mayor **traslape** con el turno programado
- Menor **desviación** (suma de diferencias de entrada y salida)
- Menor **diferencia de duración** vs jornada programada

### Bloques previos/posteriores

- Se calculan con pares de marcaciones fuera de la jornada principal
- Solo son válidos si son automáticos (par Entrada/Salida, sin clasificación manual)
- Se bloquean si el día presenta retardo o salida anticipada

### Margen no computable

Minutos por debajo del umbral mínimo de tiempo extra que se restan del neto:
- Entrada anticipada < umbral → no computable
- Salida posterior < umbral → no computable
- Trabajo adicional antes < umbral → no computable
- Trabajo adicional después < umbral → no computable

```
MinutosTrabajadosNetos = Max(0, brutos − noPagado − temporal − permiso − noConsiderados − margenNoComputable)
```

---

## Cálculo de descansos

### Asignación de descansos

1. **Manual**: si hay ≥2 marcaciones con clasificación `InicioDescanso`/`FinDescanso`
2. **Por pares**: se asignan pares consecutivos de marcas intermedias como descansos
3. **Emparejamiento**: branch-and-bound que minimiza diferencia temporal con descansos configurados

### Descansos aplicados

Para cada descanso:

```
Si EsPagado:
	minutosAplicados = minutosReales

Si NoPagado:
	Si real < programado:
		minutosAplicados = programado  (se respeta el planeado)
	Si real > programado + tolerancia:
		minutosAplicados = real  (se castiga el exceso)
	Si real ≤ programado + tolerancia:
		minutosAplicados = programado  (dentro de tolerancia)

Si override manual existe:
	minutosAplicados = override
```

### Descanso no marcado

Si el descanso no tiene marcación:
- Se aplican minutos programados (descuento automático)
- Si hay salida anticipada que cubre el descanso → se marca "requiere confirmación"
- Si hay permiso parcial → no se descuenta
- Si hay resolución "no descontar" → no se descuenta (cuenta como trabajo)

### Descansos adicionales

Si se detectan más descansos que los configurados, se registran como adicionales no programados.

---

## Tiempo trabajado neto

```
MinutosTrabajadosNetos = Max(0,
	MinutosTrabajadosBrutos
	− MinutosDescansoNoPagado
	− MinutosSalidaTemporal
	− MinutosPermisoSegmento
	− MinutosNoConsiderados
)

MinutosTrabajadosNetosEfectivos = Max(0, MinutosTrabajadosNetos + MinutosPerdonadosManual)
```

**Perdón manual**: se suma al neto efectivo (aumenta el tiempo reconocido como trabajado).

---

## Tiempo extra: métodos de cálculo

### Orden de evaluación en `CalcularMinutosExtra`

```
1. Si BloquearTiempoExtraAutomatico:
   → extra = manual + salidaPostBloqueada + entradaAnticipadaBloqueada
   (solo extra objetivo e inequívoco, con umbral mínimo)

2. Si AutoDescuentoDescansoNoMarcado:
   → excedente = Max(0, neto − jornadaNeta − margen)
   → extra = (excedente > tolerancia ? excedente : 0) + manual

3. Si ModoSinTurno:
   → extra = MinutosExtraManual  (normalmente 0, usuario decide)

4. Si ModoNetoVsNeto:
   → extra = Max(0, neto − jornadaNeta) + manual
   → Si extra > 0 && extra < umbral → extra = 0

5. Sin turno (fallback):
   → extra = MinutosExtraManual

6. Modo EntradaSalida (default con turno):
   → entradaAnticipada = Max(0, entradaProgramada − entradaReal)
   → salidaPosterior = Max(0, salidaReal − salidaProgramada)
   → Si entradaAnticipada < umbral → 0
   → Si salidaPosterior < umbral → 0
   → extra = entradaAnticipada + salidaPosterior + manual
```

### Modo SinTurno

| Característica | Valor |
|---|---|
| Asignación | Automática cuando `TurnoBaseId == null` |
| Modificable | No (dropdown deshabilitado) |
| MinutosExtra | `MinutosExtraManual` (normalmente 0) |
| Auto-detección | No |
| Usuario decide | Sí, captura manual en "Tiempo extra" |
| Máximo aprobable | `MinutosTrabajadosNetos` (todo el tiempo) |
| Sugerencia | `Max(0, neto − 480)` (excedente sobre 8h) |

### Modo EntradaSalida

| Característica | Valor |
|---|---|
| Asignación | Default con turno |
| MinutosExtra | `entradaAnticipada + salidaPosterior + manual` |
| Umbral mínimo | Aplicado a cada componente por separado |
| Margen no computable | Se resta del neto (bloques < umbral) |

### Modo NetoVsNeto

| Característica | Valor |
|---|---|
| Asignación | Usuario selecciona |
| MinutosExtra | `Max(0, neto − jornadaNeta) + manual` |
| Perdón manual | NO incluido en el neto trabajado |
| Umbral mínimo | Aplicado al total |

### MinutosExtraManual

Proviene de:
- Segmentos intermedios con resolución "extra"
- Bloques previos/posteriores con resolución "extra"
- Trabajo que cruza la salida programada (parte posterior = extra)

**Sin turno**: `MinutosExtraManual` ya está incluido en `MinutosTrabajadosNetos`. NO se suma aparte al excedente.

**Con turno**: `MinutosExtraManual` viene de bloques fuera de la jornada principal. Se suma aparte a `entradaAnticipada + salidaPosterior`.

---

## Tiempo visible

### Fórmula general

```
visible = BaseVisibles + permisoAplicado + compensacionAprobada + ExtraAprobado
```

### BaseVisibles

**Con turno:**
```
BaseVisibles = Min(Max(0, netoEfectivo − extraDetectado), jornadaNeta)
```

**Sin turno:**
```
BaseVisibles = Max(0, netoEfectivo − extraAprobado)
```

### ExtraAprobado

**Con turno:**
```
ExtraAprobado = Min(pago + banco, extraDetectado)
```
Limitado por el extra detectado por el procesador.

**Sin turno:**
```
ExtraAprobado = pago + banco
```
No limitado por detectados (porque `MinutosExtra = 0`).

### Verificación de no duplicación

| Caso | BaseVisibles | ExtraAprobado | visible |
|---|---|---|---|
| Con turno, extra detectado=60, aprobado=60 | neto−60 | 60 | neto |
| Con turno, extra al banco=60, aprobado=60 | Min(neto−60, jornada) | 60 | neto |
| Sin turno, neto=591, aprobado=120 | 591−120=471 | 120 | 591 |
| Sin turno, sin aprobación | 591 | 0 | 591 |

**Regla**: el visible siempre coincide con el tiempo total trabajado. El extra aprobado se resta del base y se suma al visible, por lo que nunca duplica.

---

## Resolución de tiempo extra

### Secuencia `AplicarResolucionAsync`

```
1. Cargar contexto (saldo banco, configuración)
2. factorTiempoExtra = override ?? configuración
3. factorAcumulacionBanco = configuración (NUNCA override)
4. extraResoluble = ObtenerMinutosExtraResolubles(asistencia, factor)
   → Con turno: Max(0, MinutosExtra)
   → Sin turno: Max(0, MinutosTrabajadosNetos)
5. pagoBase = MinutosBasePago ?? MinutosPago
6. bancoBase = MinutosBaseBanco ?? MinutosBanco
7. pago = pagoBase × factorTiempoExtra (factorado)
8. banco = bancoBase × factorAcumulacionBanco (factorado, si banco habilitado)
9. Validar:
   a. pagoBase + bancoBase ≤ extraResoluble
   b. cubiertoBanco ≤ faltante
   c. cubiertoBanco ≤ saldoDisponible + banco
   d. saldoFinal ≤ topeBanco
10. Persistir en asistencia:
	- MinutosExtraAutorizadosPago = pagoBase (BASE, no factorado)
	- MinutosExtraAutorizadosBanco = bancoBase (BASE, no factorado)
	- MinutosCubiertosBancoHoras = cubiertoBanco
	- ResolucionTiempoExtra = tipo
	- FactorTiempoExtraAplicado = override ?? null
11. Crear movimientos de banco con valores FACTORADOS
```

### Valores BASE vs FACTORADOS

| Campo | Tipo | Dónde se usa |
|---|---|---|
| `MinutosExtraAutorizadosPago` | BASE | Persistencia, snapshot, visible |
| `MinutosExtraAutorizadosBanco` | BASE | Persistencia, snapshot |
| `pago` (factorado) | FACTORADO | Cálculo de monto en nómina |
| `banco` (factorado) | FACTORADO | Movimiento de banco de horas |
| `RrhhBancoHorasMovimiento.Horas` | FACTORADO | Saldo real del banco |

### Override de factor

- `FactorTiempoExtraOverride` solo afecta al **pago**
- `factorAcumulacionBanco` siempre usa `BancoHorasFactorAcumulacion` de configuración
- El override se persiste en `FactorTiempoExtraAplicado`

---

## Banco de horas

### Ajuste en snapshot (`AjustarBancoHorasSnapshot`)

```
Si banco deshabilitado:
	minutosExtraPago += minutosBancoAcumulados
	minutosBancoAcumulados = 0
	minutosBancoConsumidos = 0

Si banco habilitado:
	Si factorAcumulacion > 0 && ≠ 1:
		minutosBancoAcumulados = Round(minutosBancoAcumulados × factor)
	Si tope > 0 && acumulados > tope:
		excedente = acumulados − tope
		acumulados = tope
		minutosExtraPago += excedente
```

### Saldo de banco

```
SaldoBanco = Suma(RrhhBancoHorasMovimientos.Horas) × 60  (en minutos)
```

El saldo usa los movimientos reales (valores factorados). No se recalcula desde los base.

---

## Faltante y permiso

### Faltante banco

```
FaltanteBanco = Max(0, MinutosJornadaNetaProgramada − MinutosTrabajadosNetosEfectivos)
```

Sin turno: `jornadaNeta = 0`, por lo que `FaltanteBanco = 0` (no hay faltante).

### Faltante descontable

```
FaltanteDescontable = Max(0, FaltanteBanco − permisoAplicado − compensacionAprobada)
```

### Permiso sugerido

```
PermisoSugerido = Max(0, FaltanteBanco − compensacionAprobado)
```

### Descanso no pagado excluido del permiso

```
DescansoNoPagadoExcluido = Min(descansoNoPagadoProgramado, Max(0, ausenciaBruta − permisoSugerido))
```

---

## Snapshot de prenómina

### Construcción por empleado (`ConstruirSnapshotEmpleado`)

```
1. Clasificar ausencias (vacaciones, incapacidad, conGoce, sinGoce)
2. diasTrabajados = resumen.DiasTrabajados (si hay asistencias) o cálculo por días
3. diasPagados = Max(0, diasPeriodo − diasSinGoce − faltasInjustificadas)
4. cicloVacacional = ObtenerCicloVacacional
5. diasVacacionesDisponibles = CalcularDiasVacacionesDisponibles
6. saldoBancoActual = ObtenerSaldoBancoHorasAsync / 60
7. factorOverride = resumen.FactorTiempoExtraOverride (último día del período)
8. FactorPagoTiempoExtra = factorOverride > 0 ? factorOverride : configuracion
```

### Resumen de asistencias (`ConstruirResumenAsistencia`)

```
1. minutosExtraBase = Suma(MinutosExtra)  (detectado)
2. minutosExtraPago = Suma(MinutosExtraAutorizadosPago)  (base)
3. minutosBancoAcumulados = Suma(MinutosExtraAutorizadosBanco)  (base)
4. minutosBancoConsumidos = Suma(MinutosCubiertosBancoHoras)
5. AjustarBancoHorasSnapshot(pago, banco, consumo, configuracion)
6. minutosDescansoTomado = Suma(MinutosDescansoTomado)
7. minutosRetardo = Suma(ObtenerMinutosRetardoEfectivos(a, permiso))
8. minutosSalidaAnticipada = Suma(ObtenerMinutosSalidaAnticipadaEfectivos(a, permiso))
9. minutosFaltanteDescontable = Suma(ObtenerMinutosFaltanteDescontable(a, permiso, 0))
10. HorasTrabajadasNetas = Suma(ObtenerMinutosTrabajadosNetosEfectivos(a)) / 60
11. HorasExtra = minutosExtraPago / 60  (después de ajuste de banco)
12. FactorTiempoExtraOverride = último día con FactorTiempoExtraAplicado > 0
```

---

## Cálculo de nómina

### `NominaCalculator.Calculate`

```
1. sueldoDiario = sueldoReferencia / diasBase
2. sueldoBase = sueldoDiario × diasPagados
3. Horas extra legales:
   a. horasBase = HorasExtraBase > 0 ? HorasExtraBase : HorasExtra
   b. horasPagables = HorasExtra
   c. horasLegales = Min(horasBase, horasPagables)
   d. horasDobles = Min(9, horasLegales)
   e. horasTriples = Max(0, horasLegales − horasDobles)
4. montoHorasExtra = horasDobles × sueldoHora × factorDobles
				   + horasTriples × sueldoHora × factorTriples
   donde:
   factorDobles = FactorPagoTiempoExtra > 0 ? FactorPagoTiempoExtra : FactorHoraExtra
   factorTriples = FactorPagoTiempoExtra > 0 ? FactorPagoTiempoExtra : FactorHoraExtraTriple
5. montoDescuentoMinutos = descuento × sueldoDiario
6. Cuotas IMSS, INFONAVIT, ISR, subsidio
7. Provisiones (aguinaldo, prima vacacional)
8. TotalPagar = sueldoBase + destajo + bono + festivo + descansoTrabajado
			  + primaDominical + primaVacacional + complemento + montoHorasExtra
			  + percepcionesManuales + subsidio
			  − deducciones − descuentoMinutos − imssObrera − infonavit − isr
```

### Nota clave

El sueldo se paga por **días pagados**, no por horas trabajadas. Las horas extra son un concepto adicional. No hay doble pago.

---

## Descuentos efectivos

### Retardo efectivo

```
retardoDespuesPerdon = Max(0, retardo − Min(retardo, perdonManual))
retardoEfectivo = Max(0, retardoDespuesPerdon − Min(retardoDespuesPerdon, permisoAplicado))
```

### Salida anticipada efectiva

```
1. perdonRestante = perdon − Min(retardo, perdon)
2. permisoRestante = permiso − Min(retardoDespuesPerdon, permiso)
3. salidaDespuesPerdon = Max(0, salidaAnticipada − Min(salidaAnticipada, perdonRestante))
4. Si salidaTempranaCompensaDescanso Y descansoNoPagadoPendiente > 0:
   salidaDespuesDescanso = Max(0, salidaDespuesPerdon − Min(salidaDespuesPerdon, descansoPendiente))
   salidaEfectiva = Max(0, salidaDespuesDescanso − Min(salidaDespuesDescanso, permisoRestante))
5. Si no:
   salidaEfectiva = salidaDespuesPerdon
```

### Descuento total

```
DescuentoTotal = retardoEfectivo + salidaAnticipadaEfectiva + faltanteDescontable + descuentoManual
```

---

## Matriz de reglas por modo

| Regla | EntradaSalida | NetoVsNeto | SinTurno |
|---|---|---|---|
| **Asignación** | Default con turno | Usuario selecciona | Automático sin turno |
| **MinutosExtra** | entradaAnticipada + salidaPosterior + manual | Max(0, neto − jornadaNeta) + manual | MinutosExtraManual (0) |
| **Umbral mínimo** | Por componente | Al total | N/A |
| **Perdón manual en neto** | No | No | No |
| **ExtraResoluble** | MinutosExtra | MinutosExtra | MinutosTrabajadosNetos |
| **ExtraAprobado limitado por** | Min(aprobado, detectado) | Min(aprobado, detectado) | Aprobado (sin límite) |
| **BaseVisibles resta** | extraDetectado | extraDetectado | extraAprobado |
| **Apartado tiempo extra** | Si hay extra detectado | Si hay extra detectado | Siempre si hay trabajo |
| **Modificable** | Sí | Sí | No |
| **Sugerencia** | extra del procesador | neto − jornada | neto − 480 |

---

## Reglas de no duplicación

| Componente | Cómo se evita la duplicación |
|---|---|
| Tiempo visible vs extra | `BaseVisibles` resta el extra antes de sumar `ExtraAprobado` |
| Tiempo visible vs banco | El banco es saldo acumulado, no tiempo pagado. No afecta el visible. |
| Sueldo vs horas extra | El sueldo se paga por días, no por horas. Las horas extra son adicionales. |
| Extra manual vs excedente sin turno | Sin turno, el excedente ya incluye manual. No se suma aparte. |
| Snapshot vs movimientos | Snapshot usa base + factor. Movimientos usan factorado. Ambos coinciden. |
| Descanso vs salida anticipada | Si la salida cubre el descanso, no se descuenta doble. |

---

## Reglas funcionales consolidadas

1. El override de factor de pago **NO** afecta la acumulación del banco de horas
2. El modo NetoVsNeto **NO** incluye perdón manual en el neto trabajado
3. Todos los pares de marcas intermedias son **descansos**: el trabajo está implícito entre ellos
4. Los valores persistidos son **BASE** (sin factorar); el factor se aplica en cada capa
5. Banco deshabilitado → todo se convierte a **pago**
6. Tope de banco excedente → se **paga** como extra
7. Permiso con goce → **descuenta** banco de horas; sin goce → no consume
8. Compensación aprobada → reduce faltante y permiso sugerido
9. Salida anticipada puede **cubrir** descanso no marcado (requiere confirmación)
10. Sin turno → modo **SinTurno** automático, no modificable
11. Sin turno → **no se auto-detecta** extra; el usuario decide
12. Sin turno → extra aprobado **no se limita** por detectados
13. Sin turno → apartado de tiempo extra **siempre visible**
14. Tiempo visible **nunca duplica** extra
15. **SaveChangesAsync** antes de reprocesar (modal ve cambios del ChangeTracker)
16. **Reprocesamiento al abrir** el modal corrige valores obsoletos