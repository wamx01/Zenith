# 19. Cálculo del tiempo visible en asistencias

## Objetivo
Explicar, en términos del usuario final, cómo se pasa del conjunto de marcaciones del día al **tiempo visible** que se usa para evaluar la jornada, generar incidencias y alimentar prenómina y nómina.

## Alcance
Este documento cubre:
- conceptos básicos de jornada, descansos, tiempo trabajado y tiempo extra
- cómo se relacionan marcaciones, turno y reglas de descanso
- qué sucede cuando un descanso planeado no se toma
- qué entra, qué no entra y qué ajusta el tiempo visible final

No cubre:
- detalles de instalación de checadores
- configuración técnica de turnos
- cálculos legales de nómina

---

## Conceptos básicos

### Turno del día
Es la jornada programada que le corresponde al empleado para un día y una fecha específica. Define:
- hora de entrada programada
- hora de salida programada
- descansos planeados (por ejemplo, comida)
- si el día se trabaja o es de descanso

El turno puede venir de la asignación normal del empleado o de un cambio de turno aplicado manualmente para ese día.

### Marcaciones
Son los registros reales del checador o del mecanismo de captura. Indican los momentos en que el empleado físicamente marcó entrada, salida a comida, regreso de comida, salida final, etc.

### Bloques de tiempo
El sistema agrupa las marcaciones en bloques. Un bloque típico es:
- **trabajo**: tiempo entre una entrada y una salida
- **descanso**: tiempo entre una salida y la siguiente entrada
- **tiempo adicional**: bloques detectados fuera de la jornada principal que pueden ser tiempo extra

El bloque principal se forma entre la primera entrada real y la última salida real del día.

---

## Jornada programada

### Jornada bruta programada
Es la diferencia entre la hora de salida programada y la hora de entrada programada del turno, **sin descontar nada**.

> **Ejemplo**: si el turno es de 08:30 a 19:00, la jornada bruta programada es 10 horas y 30 minutos.

### Jornada neta programada
Es la jornada bruta programada **menos los descansos no pagados** que tiene configurados el turno.

> **Ejemplo**: si el turno de 10:30 h tiene un descanso no pagado de 1:30 h, la jornada neta programada es 9:00 h.

Los descansos pagados **no** restan de la jornada neta.

---

## Descansos

### Descansos programados
Son los descansos definidos en el turno. Cada uno tiene:
- número de orden
- hora de inicio
- hora de fin
- indicador de si es pagado o no pagado

### Descansos tomados
Son los descansos que realmente se detectan a partir de las marcaciones del empleado. Para que el sistema reconozca un descanso tomado, normalmente se espera ver:
- una salida
- un periodo de tiempo
- una entrada posterior

### Descansos aplicados
Cuando las marcaciones no dejan claro si se tomó un descanso, el sistema puede **aplicar** el descanso programado de todos modos, descontando su duración del tiempo trabajado, siempre que sea coherente con el resto de la jornada.

> **Ejemplo práctico**: si el turno contempla comida de 13:00 a 14:00 y el empleado no registra salida y entrada a comida, pero trabajó el resto del día completo, el sistema puede aplicar 1 hora de descanso no pagado para calcular el tiempo trabajado neto.

---

## Descanso no descontado

### Cuándo aplica
Cuando el empleado **no tomó un descanso programado** y la empresa decide que ese tiempo no debe descontarse de su jornada.

### Efecto
Al marcar un descanso como "no descontado", el sistema lo trata como tiempo de trabajo efectivo. Eso significa que:
- ya no resta de la jornada neta programada
- suma al tiempo trabajado neto
- puede cambiar si el día genera tiempo extra o faltante

### Reglas importantes
- Solo se puede marcar descansos que existan en el plan del turno.
- No se puede marcar un descanso como no descontado si el día ya fue cerrado de forma definitiva para nómina.
- Al guardar la marca, el sistema vuelve a procesar el día para reflejar el cambio.

---

## Descanso con duración diferente a la programada

### Descanso tomado más corto de lo programado
Si el empleado tomó su descanso pero salió antes o regresó antes de lo planeado, el sistema compara el tiempo real de descanso contra el tiempo programado:

- **Tiempo real menor al programado**: el sistema descuenta solo el tiempo real que el empleado estuvo fuera. La diferencia entre lo programado y lo real se cuenta como trabajo efectivo.
- **Tolerancia**: existe un margen de tolerancia configurable por la empresa. Si el exceso de descanso está dentro de la tolerancia, se considera normal y no genera incidencia.

> **Ejemplo**: el descanso de comida programado es de 60 minutos (13:00 a 14:00). El empleado marca salida a las 13:05 y regreso a las 13:40. El descanso real fue de 35 minutos, no 60. El sistema descuenta 35 minutos y los 25 minutos restantes se suman como tiempo trabajado.

### Descanso tomado más largo de lo programado
Si el empleado tardó más de lo programado en su descanso (y la diferencia excede la tolerancia configurada), el exceso se registra como tiempo adicional fuera de la jornada y puede generar una incidencia o requerir corrección manual.

> **Ejemplo**: el descanso programado es de 30 minutos. El empleado marca salida a las 13:00 y regreso a las 14:00. Tomó 60 minutos en lugar de 30. El sistema descuenta los 30 minutos programados del descanso y los 30 minutos excedentes generan una incidencia o se clasifican como tiempo no trabajado.

### Descanso adicional no programado
Si el empleado tomó un tiempo de descanso que **no está en el plan del turno**, el sistema detecta ese bloque intermedio entre marcaciones y lo clasifica según su duración y las reglas de alternancia:

- Si el tiempo coincide con un descanso programado cercano, se asocia a ese descanso (con posible exceso).
- Si no coincide con ningún descanso programado, se clasifica como **salida temporal** o **descanso adicional** y se descuenta del tiempo trabajado como tiempo no productivo.

> **Ejemplo**: el turno solo tiene un descanso de comida de 13:00 a 14:00, pero el empleado marcó salida a las 10:30, entrada a las 10:45, y luego su comida normal. El bloque de 15 minutos no programado se clasifica como salida temporal o descanso adicional.

---

## Salida temporal

### Qué es
Una salida temporal es un bloque corto de tiempo donde el empleado salió y regresó durante la jornada, pero ese bloque no coincide con un descanso programado del turno.

### Cómo se detecta
El sistema identifica pares de marcaciones intermedias (salida y entrada) que no corresponden a un descanso configurado. Si la duración es breve, se clasifica como salida temporal; si es más larga, se evalúa como un descanso adicional.

### Efecto en el tiempo visible
Las salidas temporales se descuentan del tiempo trabajado bruto, igual que un descanso. Si la empresa decide que la salida fue justificada, se puede aplicar un permiso parcial con goce para recuperar ese tiempo.

---

## Jornada sin turno asignado

### Qué sucede
Si el empleado tiene marcaciones pero no tiene un turno asignado para ese día, el sistema:

1. No puede calcular jornada programada, descansos ni tiempo extra.
2. Registra la asistencia con estatus de **turno no asignado**.
3. Calcula el tiempo trabajado bruto a partir de las marcaciones reales.
4. No aplica reglas de retardo, salida anticipada ni descansos porque no hay referencia.

### Cómo corregir
Un usuario con permisos puede:
- Asignar un turno al empleado para ese día (cambio de turno manual).
- Reprocesar la asistencia para que el sistema recalcule con el turno correcto.

> **Nota**: sin turno, no hay jornada neta programada, no hay descansos programados y no se puede determinar si hubo retardo o salida anticipada. Siempre es necesario asignar un turno antes de evaluar la jornada.

---

## Anular marcajes

### Qué es
Anular un marcaje significa marcar una marcación como inválida para que no se considere en el cálculo de la jornada. Se usa cuando:
- el empleado marcó dos veces por error
- el checador registró una lectura incorrecta
- se detecta una marcación duplicada o imposible (por ejemplo, una entrada sin salida correspondiente)

### Efecto
Al anular un marcaje, el sistema:
- excluye esa marcación del análisis de bloques
- recalcula la jornada y las incidencias sin ese registro
- registra en la bitácora quién anuló el marcaje y por qué

### Recomendación
Siempre es preferible anular un marcaje erróneo en lugar de eliminarlo, para mantener la auditoría. El marcaje anulada sigue visible en el historial pero no afecta los cálculos.

---

## Tiempo extra

### Detección
El tiempo extra se detecta cuando el tiempo trabajado neto (después de descansos, retardos y salidas anticipadas) excede la jornada neta programada del turno.

> **Ejemplo**: la jornada neta es de 9:00 h y el empleado trabajó 9:45 h netas. Hay 45 minutos de tiempo extra detectado.

### Mínimo configurable
Existe un umbral mínimo configurable por la empresa (por ejemplo, 30 minutos). Si el tiempo extra detectado no alcanza ese mínimo, no se propone como tiempo extra.

### Modos de sugerencia
El sistema puede sugerir el tiempo extra de dos formas:
- **Modo entrada/salida**: compara la entrada y salida real contra la programada y calcula el extra como la diferencia neta.
- **Modo neto vs. neto**: compara el tiempo trabajado neto contra la jornada neta programada.

El modo seleccionado puede cambiar qué minutos se sugieren como extra.

---

## Resolución de tiempo extra: banco de horas vs. pago en dinero

### Banco de horas
Cuando el tiempo extra se envía al banco de horas, los minutos aprobados se acumulan en el saldo del banco de horas del empleado. Posteriormente, ese saldo puede usarse para cubrir faltantes en otros días (permisos con goce, retardos, salidas anticipadas).

> **Ejemplo**: si el empleado tiene 45 minutos de tiempo extra y se envían al banco, su saldo del banco de horas aumenta en 45 minutos.

### Pago en dinero
Cuando el tiempo extra se paga, se registra como un concepto de nómina por pagar al empleado en el próximo período. No se acumula en el banco de horas.

> **Ejemplo**: si el empleado tiene 45 minutos de tiempo extra y se elige pagar, esos 45 minutos se convertirán en un monto a pagar en la nómina según el factor de pago configurado.

### Combinación
Es posible dividir el tiempo extra aprobado entre banco y pago. Por ejemplo, de 60 minutos extra, se pueden pagar 30 y enviar 30 al banco.

### Factor de tiempo extra
El factor de tiempo extra define cuántas veces se paga cada minuto extra respecto al minuto normal. El factor estándar es **2×** (doble pago), pero puede variar según la configuración de la empresa.

> **Ejemplo**: con factor 2×, 60 minutos de tiempo extra se pagan como 120 minutos normales. Con factor 1.5×, se pagarían como 90 minutos normales.

### Override de factor
Para un día específico, un usuario con permisos puede aplicar un factor de tiempo extra diferente al configurado por la empresa. Esto permite pagar el extra con un factor especial (por ejemplo, triple en día festivo) sin modificar la configuración general.

El override solo aplica para ese día y ese empleado; no cambia la configuración global.

---

## Tiempo trabajado

### Tiempo trabajado bruto
Es la suma del tiempo real que aparece en las marcaciones, sin aplicar reglas de descanso ni ajustes. Se calcula sobre el bloque principal del día.

> **Ejemplo**: el empleado marcó de 07:31 a 19:09. El tiempo trabajado bruto sería 11 horas y 38 minutos.

### Tiempo trabajado neto
Es el tiempo trabajado bruto **menos los descansos no pagados aplicados** y otros ajustes no computables (como permisos sin goce o tiempos fuera de la jornada que no se consideran trabajo).

> **Ejemplo**: si del bloque de 11:38 h se descuentan 1:30 h de descanso no pagado, el tiempo trabajado neto queda en 10:08 h.

### Tiempo visible
Es el tiempo que finalmente se reconoce como trabajo efectivo para el día. Se calcula como:

```text
tiempo visible = tiempo trabajado neto detectado
				 + compensación aprobada del mismo día
				 + tiempo extra aprobado
```

El tiempo visible no puede ser negativo. Si el empleado no cumple la jornada neta programada, el faltante se registra como retardo o salida anticipada, según corresponda.

---

## Incidencias que afectan el tiempo visible

### Retardo
Sucede cuando la entrada real es posterior a la entrada programada (con tolerancias configuradas por la empresa). El retardo resta del tiempo visible.

### Salida anticipada
Sucede cuando la salida real es anterior a la salida programada. También resta del tiempo visible.

### Tiempo extra
Es el tiempo que excede la jornada neta programada, después de aplicar descansos, retardos y salidas anticipadas. El tiempo extra detectado no se suma al tiempo visible hasta que se aprueba o resuelve.

### Tiempo extra aprobado
Una vez que el tiempo extra es aprobado (total o parcialmente), esa parte sí se suma al tiempo visible.

### Compensación del mismo día
Si se aprobó una compensación de tiempo para ese día específico, se suma al tiempo visible. Las compensaciones de otros días no suman aquí.

---

## Reglas de bloques y resolución

### Bloque principal
Es el tramo central de trabajo del día. Normalmente se compara contra la entrada y salida programadas para detectar retardo, salida anticipada y extra.

### Bloques adicionales
Son tramos de trabajo detectados fuera del bloque principal. Pueden ser:
- entrada antes de la hora programada
- salida después de la hora programada
- bloques separados por descansos mal marcados

Cada bloque adicional se evalúa para determinar si es tiempo extra, descanso, permiso o un error de marcación.

### Resolución de segmentos
El usuario puede corregir manualmente la clasificación de un bloque de tiempo. Por ejemplo, puede indicar que un bloque marcado como descanso en realidad fue trabajo, o viceversa.

---

## Fórmula resumida del tiempo visible

```text
tiempo visible =
	tiempo trabajado neto
	+ tiempo extra aprobado
	+ compensación aprobada del día
```

Donde:
- **tiempo trabajado neto** = tiempo trabajado bruto - descansos no pagados aplicados - retardo visible - salida anticipada visible - ajustes no computables
- **tiempo extra aprobado** = porción del tiempo extra detectado que fue aprobada para pago o banco
- **compensación aprobada del día** = minutos de compensación aprobados para esa fecha específica

---

## Casos especiales

### Salida temprana que cubre un descanso
Si el empleado salió antes de la hora programada y ese tiempo no trabajado equivale aproximadamente a un descanso programado no tomado, el sistema puede sugerir confirmar que la salida temprana compensó el descanso. En ese caso:
- el descanso no se aplica como tiempo no trabajado
- la salida anticipada se mantiene como incidencia
- no se recomienda duplicar el ajuste usando permisos o banco de horas

> **Ejemplo**: el turno es de 08:00 a 18:00 con comida de 13:00 a 14:00. El empleado trabajó de 08:00 a 14:30 sin marcar comida. La salida anticipada (3:30 h) cubre parcialmente el descanso no tomado. El sistema sugiere confirmar esta regla para no descontar 1 hora de comida.

### Permiso parcial
Un permiso parcial cubre un tramo de la jornada. Puede:
- descontarse del banco de horas (si es con goce)
- no pagarse (si es sin goce)
- reducir el tiempo visible según corresponda

> **Ejemplo**: el empleado solicitó 2 horas de permiso con goce para trámite personal. El sistema registra esas 2 horas como permiso y las cubre del banco de horas si el permiso es con goce.

### Perdón manual
Un usuario con permisos puede perdonar pequeños retardos o salidas anticipadas por política interna. Esos minutos perdonados dejan de restar del tiempo visible.

> **Ejemplo**: la empresa tolera hasta 10 minutos de retardo sin incidencia. Si el empleado llegó 8 minutos tarde, un usuario con permisos puede perdonar esos minutos y el tiempo visible queda sin descuento.

### Día trabajado sin turno asignado
Si el empleado tiene marcaciones pero no tiene turno para ese día (por ejemplo, un día de descanso que sí trabajó), el sistema marca la asistencia como **descanso trabajado** o **turno no asignado**. Un usuario puede asignar un turno y reprocesar para calcular correctamente la jornada y el tiempo extra.

### Marcajes inconsistentes o incompletos
Cuando las marcaciones del día no forman pares completos (por ejemplo, una entrada sin salida, o tres marcaciones intermedias sin clasificación clara), el sistema:
- genera segmentos pendientes de clasificación
- sugiere una acción para cada segmento (trabajo, descanso, permiso, etc.)
- permite al usuario corregir manualmente la clasificación

### Exceso de descanso
Si el empleado tomó más tiempo de descanso del programado y el exceso supera la tolerancia configurada, el sistema puede:
- registrar el exceso como tiempo no trabajado
- sugerir clasificar el exceso como permiso sin goce
- generar una incidencia para revisión

---

## Validaciones finales

El sistema revisa que:
- no se paguen minutos de extra dos veces (banco y nómina)
- no se compense un faltante que ya fue absorbido por un descanso no tomado
- no se registre permiso por un tramo que ya fue cubierto por salida temprana
- el tiempo visible no exceda lo razonable dado el bloque de marcaciones real
- no se acumule en el banco más tiempo extra del que realmente se generó
- el factor de tiempo extra override no se aplique a días que no fueron autorizados
- los marcajes anulados no participen en ningún cálculo pero permanezcan visibles para auditoría
- un descanso adicional no programado no se cuente como descanso pagado a menos que se clasifique explícitamente

---

## Véase también
- Manual 07: Configuración de turnos
- Manual 10: Marcaciones y asistencias
- Manual 16: Banco de horas
- Manual 17: Ausencias
- Manual 12: Nómina
