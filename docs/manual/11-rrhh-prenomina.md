# 11. Prenómina en `RRHH`

## Objetivo
Este manual explica cómo usar `Prenóminas` como etapa de revisión y consolidación antes de generar la `Nómina` final en `Zenith`.

## Alcance
Incluye:
- propósito de la prenómina
- relación con asistencias, destajo e incidencias
- validaciones previas al cálculo
- revisión antes de cerrar a nómina

No incluye:
- timbrado
- dispersión bancaria
- recibo final de nómina

## Ruta principal
- `RRHH > Prenóminas`

## Qué es la prenómina
La prenómina es la etapa de trabajo previa a la nómina final.

Sirve para reunir y revisar información como:
- días trabajados
- asistencias
- retardos o incidencias
- horas extra
- vales de destajo aprobados
- bonos o ajustes preliminares
- deducciones iniciales

La idea es detectar errores antes de que se conviertan en pago formal.

## Por qué es importante
Sin prenómina, el cálculo final puede salir mal por causas como:
- empleado con turno incorrecto
- asistencia mal interpretada
- destajo faltante o duplicado
- periodicidad incorrecta
- incidencias no revisadas
- bonos o deducciones incompletas

La prenómina ayuda a revisar antes de cerrar.

## Base previa requerida
Antes de trabajar prenómina, ya debe existir:
- empleados correctamente capturados
- turnos correctos
- asistencias revisadas si aplican
- esquemas de pago correctos
- configuración base de nómina
- vales de destajo aprobados si aplican

## Fuentes típicas de información
La prenómina puede alimentarse de:
- `Empleados`
- `Asistencias`
- `Vales de destajo`
- configuración base de nómina
- capturas o incidencias del periodo

## Orden recomendado
La secuencia sugerida es:

`Revisar periodo -> cargar empleados -> integrar asistencia -> integrar destajo -> capturar incidencias -> validar importes -> cerrar revisión`

## Paso 1. Definir el periodo de prenómina
Antes de capturar o recalcular, validar:
- fecha inicial
- fecha final
- periodicidad correcta
- grupo de empleados al que aplica

Resultado esperado:
- la prenómina corresponde exactamente al periodo que se va a revisar

---

## Paso 2. Integrar base de empleados
Validar que entren solo los empleados correctos del periodo.

Revisar:
- estado del empleado
- esquema de pago
- periodicidad
- relación con empresa y operación

Resultado esperado:
- el universo de empleados de la prenómina queda bien definido

---

## Paso 3. Integrar asistencia si aplica
Si la empresa usa control horario, revisar que la prenómina considere correctamente:
- días trabajados
- faltas
- retardos
- horas extra
- incidencias del periodo

Resultado esperado:
- la información de tiempo trabajado ya quedó reflejada en la prenómina

---

## Paso 4. Integrar destajo si aplica
Si la empresa usa `Vales de destajo`, revisar que la prenómina tome solo:
- vales aprobados
- vales del periodo correcto
- importes que no estén duplicados

Resultado esperado:
- el componente variable de producción queda incluido correctamente

---

## Paso 5. Revisar bonos, deducciones e incidencias
Antes de cerrar la revisión, validar:
- bonos aplicables
- deducciones aplicables
- incapacidades
- vacaciones
- permisos
- ajustes manuales permitidos

Resultado esperado:
- la prenómina ya refleja la realidad operativa del periodo

---

## Paso 6. Revisar detalle por empleado
Antes de pasar a nómina, revisar por empleado:
- días pagables
- sueldo base
- destajo
- bonos
- horas extra
- deducciones
- total preliminar

Resultado esperado:
- los importes preliminares ya son revisables y defendibles

---

## Paso 7. Validar antes de generar nómina
Hacer una revisión final para detectar:
- empleados faltantes
- empleados sobrantes
- importes atípicos
- duplicidad de destajo
- incidencias no capturadas
- errores de periodicidad

Resultado esperado:
- la prenómina queda lista para convertirse en nómina final

## Qué debe salir de una buena prenómina
Una prenómina bien revisada debe dejar claro:
- cuánto se pagará a cada empleado
- por qué se pagará ese importe
- qué parte corresponde a fijo, variable o incidencia
- qué ajustes se hicieron antes del cierre

## Errores comunes a evitar
- usar prenómina sin revisar asistencias previas
- integrar destajo sin validar estatus de aprobación
- recalcular sin entender el origen del cambio
- trabajar con periodicidad incorrecta
- cerrar a nómina con incidencias pendientes

## Checklist rápido
Antes de cerrar esta etapa, validar:
- periodo correcto
- empleados correctos
- asistencia integrada si aplica
- destajo integrado si aplica
- incidencias revisadas
- importes preliminares revisados
- prenómina lista para nómina final

## Relación con manuales anteriores
Este manual ocurre después de:
- [08. Esquemas de pago en RRHH](./08-rrhh-esquemas-de-pago.md)
- [09. Alta y administración de empleados en RRHH](./09-rrhh-empleados.md)
- [10. Marcaciones y asistencias en RRHH](./10-rrhh-marcaciones-y-asistencias.md)

## Referencias relacionadas
- `../modulos/detalle/06-rrhh.md`
- `MundoVs/Components/Pages/RRHH/Prenominas.razor`

## Siguiente manual sugerido
El siguiente paso lógico es:
- `12-rrhh-nomina.md`
