# 08. Esquemas de pago en `RRHH`

## Objetivo
Este manual explica cómo configurar `Esquemas de pago` para que cada empleado tenga una regla clara de cálculo antes de operar `Prenóminas`, `Nóminas` o `Vales de destajo`.

## Alcance
Incluye:
- definición de esquemas de pago
- diferencia entre esquema y tarifa
- cuándo separar esquemas
- cuándo usar varias tarifas dentro del mismo esquema
- relación con empleados, posiciones y procesos

No incluye:
- cálculo final de nómina
- captura diaria de vales
- pago bancario o dispersión

## Ruta principal
- `RRHH > Esquemas de pago`

## Para qué sirve un `Esquema de pago`
Sirve para decirle a `Zenith` cuál es la lógica con la que se debe calcular el pago de un empleado.

En términos prácticos, el esquema responde preguntas como:
- si al empleado se le paga fijo, por pieza, por operación o mixto
- si necesita tarifas por proceso o por posición
- si puede generar componente variable por destajo
- si su cálculo depende de bono, meta o combinación de conceptos

Sin ese dato, el sistema puede saber quién es el empleado, pero no puede saber correctamente cómo debe calcularle el periodo.

## Por qué este catálogo es importante
El esquema de pago define la lógica base con la que se le paga al empleado.

No solo dice cuánto gana.
También define:
- cómo se calcula
- sobre qué concepto se calcula
- si depende de tarifa
- si depende de proceso
- si mezcla fijo, bono o destajo

Por eso se configura antes de terminar el alta operativa de empleados.

## Relación con `Prenómina`
La `Prenómina` usa el esquema de pago para empezar a construir el cálculo preliminar del empleado.

Le sirve para interpretar cosas como:
- si debe tomar sueldo base fijo
- si debe integrar destajo
- si debe considerar bonos o mezcla de conceptos
- si el empleado pertenece a una lógica distinta a la de otro grupo

En otras palabras:
- el esquema de pago le dice a la prenómina qué tipo de cálculo debe esperar
- las asistencias, vales e incidencias aportan los datos del periodo

Si el esquema está mal configurado, la prenómina puede salir mal aunque las asistencias o el destajo estén bien capturados.

## Relación con `Nómina`
La `Nómina` toma como base la revisión hecha en `Prenómina`, pero el esquema de pago sigue siendo la regla estructural que sostiene el cálculo final.

Le sirve para:
- confirmar la lógica de pago del empleado
- consolidar fijo, variable, bonos y deducciones bajo la regla correcta
- evitar mezclar empleados de distintas formas de pago como si fueran iguales

La relación correcta es esta:

`Esquema de pago -> define la lógica`

`Prenómina -> revisa el cálculo preliminar según esa lógica`

`Nómina -> cierra el cálculo final usando esa misma lógica ya validada`

Por eso, si un esquema está mal desde el origen, el error puede arrastrarse a prenómina y terminar impactando la nómina final.

## Tabla rápida de relación
| Concepto | Para qué sirve | Impacta `Prenómina` | Impacta `Nómina` |
| --- | --- | --- | --- |
| `Esquema de pago` | Define la lógica de cálculo del empleado | Sí, porque indica si el cálculo será fijo, por destajo, con bono o mixto | Sí, porque sostiene el cálculo final del periodo |
| `Tarifa` | Define el importe aplicable dentro del esquema | Sí, porque ayuda a calcular importes preliminares en destajo o variables | Sí, porque afecta el importe final a pagar |
| `Prenómina` | Revisa y consolida el cálculo preliminar del periodo | Es la etapa principal de revisión | Sirve como base previa para cerrar la nómina |
| `Nómina` | Formaliza el cálculo final del periodo | No es su función principal | Es la etapa final de cálculo y cierre |

## Lectura rápida de la tabla
- si falla el `Esquema de pago`, puede fallar tanto la `Prenómina` como la `Nómina`
- si falla la `Tarifa`, los importes variables pueden salir mal desde la revisión preliminar
- la `Prenómina` revisa
- la `Nómina` confirma y cierra

## Diferencia entre `Esquema de pago` y `Tarifa`
### `Esquema de pago`
Es la regla general.

Ejemplos:
- `Sueldo fijo semanal`
- `Destajo por pieza`
- `Destajo por operación`
- `Sueldo fijo + bono por meta`
- `Sueldo fijo + destajo`

### `Tarifa`
Es el valor o importe aplicable dentro de esa regla.

Ejemplos:
- una tarifa por `Corte`
- una tarifa por `Costura`
- una tarifa por `Empaque`
- una tarifa distinta según la `Posición`

### Regla práctica
- si cambia la `lógica`, normalmente debes crear otro esquema
- si solo cambia el `importe`, normalmente basta otra tarifa dentro del mismo esquema

## Tipos comunes de esquema

### 1. `Sueldo fijo`
Usarlo cuando el empleado gana un monto base por periodo.

Conviene cuando:
- la empresa paga por jornada o por periodo fijo
- no depende de piezas ni operaciones

Normalmente no requiere tarifas por proceso.

---

### 2. `Destajo por pieza`
Usarlo cuando el pago depende de cuántas piezas produce el empleado.

Conviene cuando:
- la unidad principal de pago es la pieza
- el cálculo se basa en cantidad terminada

Puede requerir tarifas si hay variaciones por tipo de pieza o condición de trabajo.

---

### 3. `Destajo por operación`
Usarlo cuando el pago depende de la operación realizada.

Conviene cuando:
- cada proceso tiene una tarifa distinta
- el empleado puede trabajar en distintas operaciones
- el pago depende del proceso realmente ejecutado

Aquí sí es común usar varias tarifas dentro del mismo esquema.

---

### 4. `Bono por meta`
Usarlo cuando existe un incentivo por cumplimiento.

Conviene cuando:
- el empleado o grupo recibe un bono por objetivo
- el bono se suma a otra base de pago

Normalmente no se usa solo; suele acompañar a otro esquema.

---

### 5. `Esquema mixto`
Usarlo cuando el pago combina varias reglas.

Ejemplos:
- sueldo fijo más destajo
- sueldo fijo más bono
- sueldo fijo más incentivos variables

Conviene cuando la empresa necesita una parte garantizada y otra variable.

## Cuándo NO crear un esquema nuevo
No conviene crear otro esquema solo porque:
- cambió una tarifa pequeña
- cambió el importe de una operación
- dos procesos pagan diferente dentro de la misma lógica
- dos posiciones tienen diferente tarifa en la misma regla

### Ejemplo correcto
Un solo esquema:
- `Destajo por operación`

Dentro del esquema:
- `Corte` = tarifa A
- `Costura` = tarifa B
- `Empaque` = tarifa C

Eso sigue siendo un solo esquema, porque la lógica es la misma.

## Cuándo SÍ crear un esquema nuevo
Sí conviene separar cuando cambie la forma de cálculo.

Ejemplos:
- un grupo se paga por pieza y otro por operación
- un grupo gana solo fijo y otro fijo más bono
- un grupo usa metas y otro usa pago por operación
- una parte del personal tiene pago mixto y otra no

### Ejemplo correcto
Esquemas separados:
- `Destajo por pieza`
- `Destajo por operación`
- `Sueldo fijo semanal`
- `Sueldo fijo + bono`

## Relación con `Posiciones`
En muchos casos el esquema puede convivir con tarifas por `Posición`.

Esto ayuda cuando:
- la misma operación paga distinto según perfil
- la empresa diferencia entre operador base y supervisor
- la tarifa depende de habilidad o nivel del puesto

Pero la `Posición` no sustituye al esquema.

La lógica correcta es:
- el esquema define cómo se paga
- la posición ayuda a resolver qué tarifa aplica

## Relación con procesos
En destajo por operación, la tarifa puede depender de:
- proceso
- posición
- ambos

Esto permite que un mismo esquema sirva para muchos casos sin crear catálogos duplicados.

## Cómo decidir correctamente
Hazte estas preguntas:

1. ¿Cambió la lógica de cálculo?
- si sí, crea otro esquema
- si no, sigue evaluando

2. ¿Solo cambió el importe o tarifa?
- si sí, usa otra tarifa dentro del mismo esquema

3. ¿Se paga por proceso, por pieza o por mezcla?
- eso sí define el tipo de esquema

## Recomendación operativa
Mantén pocos esquemas y muchas tarifas bien organizadas.

Eso ayuda a:
- administrar mejor el catálogo
- evitar duplicados
- asignar empleados más rápido
- recalcular con más claridad
- reducir errores en prenómina y nómina

## Ejemplos rápidos
### Caso A
`Operador A` y `Operador B` trabajan en `Costura`, pero uno gana más.

Solución recomendada:
- mismo esquema
- distinta tarifa por posición o perfil

### Caso B
`Operador C` cobra por pieza y `Operador D` por operación.

Solución recomendada:
- dos esquemas distintos

### Caso C
`Supervisor` gana fijo más bono, mientras `Operación` gana por destajo.

Solución recomendada:
- esquemas separados

## Checklist rápido
Antes de cerrar esta etapa, validar:
- esquemas creados por familia de pago
- tarifas capturadas donde aplique
- separación correcta entre lógica y tarifa
- empleados listos para vincularse al esquema correcto
- relación con procesos y posiciones revisada

## Errores comunes a evitar
- crear un esquema por cada empleado
- crear un esquema por cada tarifa pequeña
- mezclar fijo, destajo y bono dentro de un mismo esquema sin control
- usar posición como reemplazo del esquema
- asignar empleados sin revisar si su lógica real de pago coincide

## Relación con manuales anteriores
Este manual ocurre después de:
- [06. Configuración inicial de RRHH](./06-rrhh-configuracion-inicial.md)
- [07. Configuración de turnos en RRHH](./07-rrhh-turnos.md)

## Referencias relacionadas
- `../modulos/detalle/06-rrhh.md`
- `MundoVs/Components/Pages/RRHH/EsquemasPago.razor`

## Siguiente manual sugerido
El siguiente paso lógico es:
- `09-rrhh-empleados.md`
