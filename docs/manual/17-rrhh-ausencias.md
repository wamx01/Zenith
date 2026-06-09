# 17. Ausencias en `RRHH`

## Objetivo
Este manual explica para qué sirven las `Ausencias`, qué tipos maneja el sistema y cómo se relacionan con `Asistencias`, `Prenómina` y `Nómina` en `Zenith`.

## Alcance
Incluye:
- propósito del registro de ausencias
- tipos de ausencia disponibles
- reglas de goce de pago por tipo
- estatus de la ausencia
- impacto en prenómina y nómina

No incluye:
- cálculo fiscal detallado
- dispersión bancaria
- reglas externas específicas fuera del flujo funcional del sistema

## Ruta principal
- `RRHH > Ausencias`

## Para qué sirve una `Ausencia`
La ausencia sirve para registrar formalmente que el empleado no laborará o no laboró en un rango determinado bajo una causa controlada.

En este flujo, la pantalla maneja:
- `Vacaciones`
- `Permisos` (con goce, sin goce, genérico)
- `Capacitaciones`
- `Incapacidades`
- `Días económicos`
- `Permisos de paternidad/maternidad`
- `Faltas injustificadas`
- `Suspensiones`

Con historial por empleado.

## Qué información registra
Una ausencia puede guardar:
- empleado
- tipo
- estatus
- fecha inicio
- fecha fin
- días
- horas
- si es con goce de pago
- motivo
- observaciones
- aprobación

## Tipos de ausencia
El sistema maneja los siguientes tipos con reglas específicas de goce de pago:

### `Vacaciones`
- Se usan para registrar periodos vacacionales del empleado.
- **Siempre con goce de pago** (no se puede cambiar).
- Consumen días del saldo vacacional del empleado.
- Se consideran días pagados en nómina.
- Se restan de días trabajados (el empleado no laboró físicamente).

### `Permiso` (genérico)
- Ausencia autorizada donde el usuario elige si es con goce o sin goce.
- **Único tipo donde el checkbox de goce es editable**.
- El usuario debe definir explícitamente si conserva o no el pago.
- Si es con goce: no descuenta días pagados.
- Si es sin goce: descuenta días pagados en nómina.

### `Permiso con goce`
- Permiso que siempre conserva el pago del empleado.
- **No se puede cambiar a sin goce**.
- No descuenta días pagados en nómina.
- Ejemplos: permiso por lactancia, permiso sindical, etc.

### `Permiso sin goce`
- Permiso que siempre reduce el pago del empleado.
- **No se puede cambiar a con goce**.
- Descuenta días pagados en nómina.
- Ejemplos: asuntos particulares sin pagar, permiso personal, etc.

### `Capacitación`
- Ausencia por actividades de formación o adiestramiento.
- **Siempre con goce de pago** (no se puede cambiar).
- No descuenta días pagados en nómina.
- Registra la razón de la capacitación en el campo motivo.

### `Incapacidad`
- Ausencia por razones médicas documentadas.
- **Siempre con goce de pago** en el sistema (no se puede cambiar).
- No descuenta días pagados en nómina.
- El IMSS puede reembolsar según corresponda.
- Se contabiliza como `DiasIncapacidad` en prenómina.

### `Falta injustificada`
- Ausencia no autorizada del empleado.
- **Siempre sin goce de pago** (no se puede cambiar).
- Descuenta días pagados en nómina.
- No requiere aprobación formal.

### `Suspensión`
- Suspensión disciplinaria del empleado.
- **Siempre sin goce de pago** (no se puede cambiar).
- Descuenta días pagados en nómina.

### `Días económicos`
- Días de descanso económico del empleado.
- **Siempre con goce de pago** (no se puede cambiar).
- No descuenta días pagados en nómina.

### `Permiso de paternidad`
- Ausencia por nacimiento de hijo.
- **Siempre con goce de pago** (no se puede cambiar).
- No descuenta días pagados en nómina.
- Según LFT art. 132-Bis.

### `Permiso de maternidad`
- Ausencia por maternidad.
- **Siempre con goce de pago** (no se puede cambiar).
- No descuenta días pagados en nómina.
- Según LFT art. 170.

## Tabla de reglas de goce de pago
| Tipo | Goce de pago | ¿Editable? | Impacto en días pagados |
| --- | --- | --- | --- |
| Vacaciones | Con goce | ✗ | No descuenta |
| Permiso (genérico) | Según captura | ✓ | Según elección |
| Permiso con goce | Con goce | ✗ | No descuenta |
| Permiso sin goce | Sin goce | ✗ | Descuenta |
| Capacitación | Con goce | ✗ | No descuenta |
| Incapacidad | Con goce | ✗ | No descuenta |
| Falta injustificada | Sin goce | ✗ | Descuenta |
| Suspensión | Sin goce | ✗ | Descuenta |
| Días económicos | Con goce | ✗ | No descuenta |
| Permiso paternidad | Con goce | ✗ | No descuenta |
| Permiso maternidad | Con goce | ✗ | No descuenta |

## Estatus de la ausencia
El estatus ayuda a distinguir si la ausencia:
- apenas fue solicitada
- ya fue aprobada
- fue rechazada
- ya fue aplicada
- fue cancelada

Esto es importante porque no toda ausencia capturada debería impactar igual de inmediato el cálculo del periodo.

| Estatus | Significado |
| --- | --- |
| `Solicitada` | Pendiente de revisión |
| `Aprobada` | Autorizada, impacta cálculo |
| `Rechazada` | No impacta cálculo |
| `Aplicada` | Ya procesada en nómina |
| `Cancelada` | Anulada, no impacta |

Solo las ausencias con estatus **Aprobada** o **Aplicada** se consideran en prenómina.

## Relación con `Asistencias`
La ausencia no sustituye a la asistencia, pero sí la explica o la complementa.

Por ejemplo:
- si el empleado no aparece con jornada normal
- la ausencia puede justificar ese comportamiento
- evita tratar automáticamente el día como si fuera solo falta injustificada

Los permisos con goce de pago se consideran al validar retardos y salidas anticipadas, reduciendo el tiempo esperado según las horas del permiso.

### Regla práctica
- `Asistencias` muestra lo que pasó en tiempo y marcaciones
- `Ausencias` explica por qué el empleado no laboró o no debía laborar en ese rango

## Relación con `Prenómina`
La `Prenómina` debe considerar las ausencias para revisar correctamente conceptos como:
- días trabajados
- días pagables
- vacaciones
- permisos con/sin goce
- incapacidades
- incidencias del periodo

### Cómo se clasifican las ausencias en prenómina
El sistema clasifica automáticamente las ausencias aprobadas del periodo:

1. **Vacaciones** → se contabilizan como `DiasVacaciones`
2. **Incapacidades** → se contabilizan como `DiasIncapacidad`
3. **Ausencias con goce** (permisos con goce, capacitaciones, días económicos, paternidad/maternidad) → se restan de días trabajados pero **NO** de días pagados
4. **Ausencias sin goce** (permisos sin goce, faltas injustificadas, suspensiones) → se contabilizan como `DiasFaltaJustificada` y **SÍ** se descuentan de días pagados

### Fórmula de días pagados
```
diasPagados = diasPeriodo - diasSinGoce - faltasInjustificadas
```

Ejemplos:
- Periodo de 7 días con 2 días de capacitación: `7 - 0 - 0 = 7` días pagados
- Periodo de 7 días con 2 días de permiso sin goce: `7 - 2 - 0 = 5` días pagados
- Periodo de 7 días con 1 falta injustificada: `7 - 0 - 1 = 6` días pagados
- Periodo de 7 días con 1 incapacidad y 1 suspensión: `7 - 1 - 0 = 6` días pagados

Aquí es donde conviene validar si la ausencia:
- corresponde al periodo
- tiene estatus correcto
- debe afectar pago o no
- tiene goce o no

## Relación con `Nómina`
La `Nómina` toma la información ya validada del periodo.

Eso significa que una ausencia correctamente revisada puede afectar el cálculo final según su naturaleza.

Ejemplos:
- vacaciones pagadas → no descuenta días pagados
- permiso con goce → no descuenta días pagados
- capacitación → no descuenta días pagados
- permiso sin goce → descuenta días pagados, reduce sueldo base
- falta injustificada → descuenta días pagados, reduce sueldo base
- suspensión → descuenta días pagados, reduce sueldo base

La idea correcta es:

`Ausencia -> registro controlado de la incidencia`

`Prenómina -> revisión de su impacto`

`Nómina -> aplicación final al periodo`

## Notas en prenómina
El sistema genera notas descriptivas para cada ausencia que aparecen en la columna `Notas` de la prenómina. El formato es:
```
<Tipo> [c/goce|s/goce] <rango> (<días> d[, <horas> h])
```

Ejemplos:
- `Vacaciones 01/06-05/06 (5 d)`
- `Permiso c/goce 10/06 (1 d, 4 h)`
- `Permiso s/goce 12/06-13/06 (2 d)`
- `Capacitación 15/06 (1 d)`
- `Incapacidad 20/06-22/06 (3 d)`
- `Falta injustificada 25/06 (1 d)`

## Qué significa `Con goce de pago`
Este campo ayuda a distinguir si la ausencia:
- conserva pago
- o reduce pago

Es clave para no mezclar:
- vacaciones pagadas
- permisos pagados
- permisos sin goce
- capacitaciones pagadas
- incapacidades pagadas

El sistema aplica automáticamente la regla de goce según el tipo de ausencia. Solo el tipo "Permiso" genérico permite al usuario elegir.

## Qué debe revisarse antes de aplicarla
Antes de considerar una ausencia lista para impacto operativo, revisar:
- empleado correcto
- tipo correcto (usar tipo específico en lugar de "Permiso" genérico cuando aplica)
- rango correcto
- días y horas correctos
- estatus correcto
- motivo claro
- goce de pago correcto (el sistema lo aplica automáticamente según el tipo, salvo "Permiso" genérico)

## Errores comunes a evitar
- capturar ausencia sin rango correcto
- usar "Permiso" genérico sin definir si tiene goce (mejor usar "Permiso con goce" o "Permiso sin goce" directamente)
- duplicar ausencias del mismo periodo
- dejar estatus ambiguo y aun así usarla en cálculo
- tratar todas las ausencias como si impactaran igual el pago
- asumir que incapacidades descuentan días (no lo hacen; son con goce)

## Tabla rápida de relación
| Concepto | Para qué sirve | Impacta `Asistencias` | Impacta `Prenómina` | Impacta `Nómina` |
| --- | --- | --- | --- | --- |
| `Ausencia` | Registra una incidencia formal del empleado | Sí, como explicación del día o rango | Sí, porque afecta revisión del periodo | Sí, porque puede modificar días pagables |
| `Con goce de pago` | Define si la ausencia conserva pago | Indirectamente | Sí, no descuenta días pagados | Sí, el sueldo se mantiene |
| `Sin goce de pago` | Define si la ausencia reduce pago | Indirectamente | Sí, descuenta días pagados | Sí, el sueldo se reduce |
| `Vacaciones` | Registra descanso vacacional | Sí | Sí, suma a días vacaciones | Sí, paga días de vacaciones |
| `Permiso con goce` | Ausencia autorizada con pago | Sí | Sí, no descuenta días pagados | Sí, mantiene sueldo |
| `Permiso sin goce` | Ausencia autorizada sin pago | Sí | Sí, descuenta días pagados | Sí, reduce sueldo |
| `Capacitación` | Formación o adiestramiento | Sí | Sí, no descuenta días pagados | Sí, mantiene sueldo |
| `Incapacidad` | Ausencia médica documentada | Sí | Sí, no descuenta días pagados | Sí, mantiene sueldo |
| `Falta injustificada` | Ausencia no autorizada | Sí | Sí, descuenta días pagados | Sí, reduce sueldo |
| `Suspensión` | Suspensión disciplinaria | Sí | Sí, descuenta días pagados | Sí, reduce sueldo |

## Consideraciones legales
Los tipos de ausencia se alinean con el marco normativo del ERP:
- **LFT** art. 132-Bis: permiso de paternidad (con goce)
- **LFT** art. 170: permiso de maternidad (con goce)
- **LFT** art. 76: vacaciones (con goce)
- **LFT** art. 51: incapacidades (con goce, IMSS reembolsa)
- **CFF** art. 28-29-A: documentación comprobatoria
- **LISR**: tratamiento fiscal según tipo de percepción/deducción
- **LSS/IMSS**: cuotas obrero-patronales según tipo de incapacidad

## Checklist rápido
Antes de cerrar esta etapa, validar:
- ausencia capturada con empleado correcto
- tipo correcto (usar tipo específico en lugar de "Permiso" genérico cuando aplica)
- rango correcto
- goce de pago correcto (el sistema lo aplica automáticamente salvo "Permiso" genérico)
- estatus correcto
- impacto revisado en prenómina
- impacto final entendido para nómina

## Relación con manuales anteriores
Este manual ocurre después de:
- [10. Marcaciones y asistencias en RRHH](./10-rrhh-marcaciones-y-asistencias.md)
- [11. Prenómina en RRHH](./11-rrhh-prenomina.md)
- [12. Nómina en RRHH](./12-rrhh-nomina.md)

## Referencias relacionadas
- `MundoVs/Components/Pages/RRHH/Ausencias.razor`
- `MundoVs/Core/Entities/RrhhAusencia.cs`
- `MundoVs/Core/Services/RrhhPrenominaSnapshotService.cs`

## Siguiente manual sugerido
El siguiente paso lógico es:
- `18-admin-configuracion-bonos.md`
