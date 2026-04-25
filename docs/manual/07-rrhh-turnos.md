# 07. Configuración de turnos en `RRHH`

## Objetivo
Este manual explica cómo configurar los turnos base para que `Zenith` pueda interpretar jornadas, descansos, asistencias y control de tiempo de forma consistente.

## Alcance
Incluye:
- alta de turnos base
- definición de detalle por día
- descansos programados
- validación previa a la asignación a empleados

No incluye:
- alta de empleados
- asignación histórica de turnos por empleado
- procesamiento de marcaciones
- cálculo de prenómina o nómina

## Ruta principal
- `RRHH > Turnos`

## Cuándo debe hacerse
Este paso debe realizarse antes de:
- dar de alta empleados con turno operativo
- interpretar asistencias
- calcular retardos, salidas anticipadas y tiempo extra

## Qué representa un turno
Un turno define la jornada esperada del empleado, incluyendo:
- hora de entrada
- hora de salida
- días que labora
- descansos
- si cada descanso es pagado o no

## Orden recomendado
La secuencia sugerida es:
1. crear el turno base
2. definir el detalle por día
3. capturar descansos
4. validar jornada completa
5. usarlo después en empleados

## Paso 1. Crear el turno base
Capturar al menos:
- nombre del turno
- estado
- descripción operativa si aplica

Ejemplos comunes:
- `Turno matutino`
- `Turno vespertino`
- `Turno nocturno`
- `Turno administrativo`

Resultado esperado:
- existe un catálogo inicial de turnos reutilizables

---

## Paso 2. Definir detalle por día
Para cada día de la semana, revisar:
- si el empleado labora o no
- hora de entrada
- hora de salida
- duración esperada de la jornada

Resultado esperado:
- cada turno queda completamente definido por día y no solo como nombre general

---

## Paso 3. Configurar descansos
Por cada día laborable, definir si existen:
- `0` descansos
- `1` descanso
- `2` descansos

Para cada descanso, revisar:
- hora de inicio
- hora de fin
- si el descanso es pagado o no

Resultado esperado:
- el sistema puede distinguir tiempo bruto y tiempo neto trabajado con mayor precisión

---

## Paso 4. Validar congruencia operativa
Antes de usar el turno, validar:
- que los horarios correspondan a la operación real
- que los descansos reflejen la práctica real de planta u oficina
- que no existan traslapes o horarios imposibles
- que los días no laborables estén marcados correctamente

Resultado esperado:
- el turno ya sirve como base confiable para asistencia y control de tiempo

---

## Paso 5. Preparar asignación a empleados
Una vez validados los turnos, ya se pueden usar en:
- `RRHH > Empleados`
- historial o vigencia de turnos por empleado cuando aplique

Resultado esperado:
- los empleados pueden quedar ligados a un turno base correcto

## Qué impacta un turno bien configurado
Un turno correcto ayuda a interpretar mejor:
- retardos
- salidas anticipadas
- horas trabajadas brutas
- horas trabajadas netas
- tiempo extra
- observaciones de asistencia

## Errores comunes a evitar
- crear turnos solo con nombre y sin detalle diario
- no capturar descansos reales
- usar un mismo turno para personal con jornadas distintas
- marcar como laborable un día que realmente es descanso
- configurar horarios provisionales y dejarlos en operación

## Checklist rápido
Antes de cerrar esta etapa, validar:
- turnos creados
- detalle diario completo
- descansos revisados
- días laborables correctos
- horarios congruentes con la operación
- turnos listos para asignarse a empleados

## Relación con manuales anteriores
Este manual ocurre después de:
- [06. Configuración inicial de RRHH](./06-rrhh-configuracion-inicial.md)

## Referencias relacionadas
- `../modulos/detalle/06-rrhh.md`
- `MundoVs/Components/Pages/RRHH/Turnos.razor`

## Siguiente manual sugerido
El siguiente paso lógico es:
- `08-rrhh-esquemas-de-pago.md`
- o `08-rrhh-empleados.md` si la empresa ya tiene bien definidos sus esquemas
