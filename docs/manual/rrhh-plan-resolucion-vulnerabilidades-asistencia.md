# Plan de resolución de vulnerabilidades en cálculo de asistencias

## Objetivo

Corregir las inconsistencias del cálculo de asistencias para que el sistema pueda explicar, validar y calcular correctamente:

- tiempo trabajado real,
- tiempo computable para nómina,
- descansos,
- salidas temporales,
- permisos,
- retardos,
- salida anticipada,
- tiempo extra detectado,
- tiempo extra aprobado para pago o banco de horas.

El objetivo principal es evitar resultados contradictorios como:

- muchas horas de tiempo extra y salida anticipada fuerte en el mismo día sin explicación clara,
- doble conteo de bloques,
- descansos interpretados como trabajo,
- trabajo interpretado como descanso,
- salidas anticipadas infladas por descansos no tomados,
- cálculos incorrectos cuando falta una marcación.

---

## Principio funcional propuesto

El cálculo debe dejar de depender principalmente de pares implícitos ambiguos y pasar a un modelo de segmentos de jornada.

Ejemplo conceptual:

| Segmento | Tipo sugerido | Descripción |
|---|---|---|
| 1 | Trabajo | Desde la entrada hasta la primera salida |
| 2 | Descanso | Desde salida a descanso hasta regreso |
| 3 | Trabajo | Desde regreso hasta siguiente salida |
| 4 | Descanso / salida temporal / permiso | Según horario, configuración o resolución manual |
| 5 | Trabajo | Último bloque laboral |

La alternancia base puede ser:

```text
Trabajo -> Descanso/Salida/Permiso -> Trabajo -> Descanso/Salida/Permiso -> Trabajo
```

Pero el sistema debe permitir que RRHH reclasifique cada segmento cuando el patrón automático no sea confiable.

---

## Problemas detectados

### 1. Doble conteo de tiempo extra

Actualmente el cálculo puede sumar al mismo tiempo:

- entrada anticipada,
- salida posterior,
- bloque previo al turno,
- bloque posterior al turno.

Riesgo:

```text
El mismo tiempo puede terminar contado como bloque adicional y también como entrada anticipada.
```

Solución propuesta:

- Calcular primero los segmentos reales del día.
- Clasificar cada segmento como trabajo, descanso, permiso, salida temporal, extra o ignorado.
- Calcular el extra únicamente a partir de segmentos de trabajo fuera de la jornada programada.
- Evitar sumar `entradaAnticipada + bloquePrevio` si representan el mismo intervalo.

---

### 2. Salida anticipada inflada

Actualmente la salida anticipada se calcula contra la salida real elegida:

```text
salida anticipada = salida programada - salida real
```

Si el sistema eligió como salida real una marcación que en realidad era descanso, permiso o salida temporal, se infla el descuento.

Solución propuesta:

Separar la salida anticipada en campos conceptuales:

```text
salidaAnticipadaBruta
salidaAnticipadaCubiertaPorDescansoNoTomado
salidaAnticipadaCubiertaPorPermiso
salidaAnticipadaCubiertaPorBancoHoras
salidaAnticipadaDescontable
```

La pantalla semanal debe mostrar principalmente la salida anticipada descontable, no solo la bruta.

---

### 3. Descanso no tomado contado como salida anticipada

Si un empleado no toma comida y sale antes, el sistema puede registrar salida anticipada completa aunque esa salida esté compensando el descanso no tomado.

Ejemplo:

```text
Turno: 09:00 - 18:00
Descanso no pagado: 60 min
Salida real: 17:00
```

No siempre significa 60 minutos descontables. Puede significar que trabajó corrido y salió una hora antes por el descanso no tomado.

Solución propuesta:

- Detectar descansos no marcados.
- Evaluar si la salida anticipada coincide con descanso no pagado.
- Solicitar confirmación cuando haya ambigüedad.
- Descontar de la salida anticipada efectiva los minutos cubiertos por descanso no tomado, si RRHH lo confirma o si la regla lo permite.

---

### 4. Selección incorrecta de jornada principal

El sistema selecciona una entrada y una salida principal usando traslape con el turno y cercanía al horario.

Esto puede fallar cuando hay:

- varias entradas y salidas,
- bloques previos,
- bloques posteriores,
- salidas temporales,
- permisos,
- marcas faltantes,
- descansos múltiples.

Solución propuesta:

- Construir todos los segmentos entre marcaciones consecutivas.
- Calcular su traslape con el turno.
- Identificar el conjunto de segmentos que mejor representa la jornada principal.
- Marcar como extra solo los segmentos de trabajo fuera de la jornada.
- Si hay varias interpretaciones posibles, dejar el día en revisión.

---

### 5. Emparejamiento frágil de marcas intermedias

Actualmente, cuando no hay clasificación manual, las marcas intermedias se emparejan por pares:

```text
marca 1 - marca 2 = descanso
marca 3 - marca 4 = descanso
```

Si falta una marca, todo el cálculo se puede desplazar.

Solución propuesta:

- Crear segmentos consecutivos entre todas las marcaciones.
- Clasificar cada segmento con reglas de prioridad:
  1. resolución manual vigente,
  2. clasificación operativa de marcaciones,
  3. coincidencia con descansos configurados,
  4. coincidencia con permisos,
  5. salida temporal probable,
  6. trabajo probable,
  7. requiere revisión.

---

### 6. Turnos nocturnos o cruzando medianoche

Las fórmulas actuales pueden fallar cuando la salida programada es menor que la entrada programada.

Ejemplo:

```text
Entrada: 22:00
Salida: 06:00
```

Solución propuesta:

- Normalizar horarios programados a rangos `DateTime` completos.
- Si la salida programada es menor o igual que la entrada programada, mover salida al día siguiente.
- Calcular traslapes y diferencias usando fechas completas, no solo `TimeSpan`.

---

### 7. Falta de explicación visible para RRHH

El resumen semanal muestra totales, pero no explica claramente cómo llegó a ellos.

Solución propuesta:

Agregar diagnóstico por día:

```text
Entrada usada: 08:54
Salida usada: 15:56
Segmento 1: Trabajo 08:54-12:00 = 186 min
Segmento 2: Descanso 12:00-12:30 = 30 min
Segmento 3: Trabajo 12:30-15:56 = 206 min
Extra detectado: 0 min
Retardo: 0 min
Salida anticipada bruta: 124 min
Salida anticipada descontable: 64 min
Motivo: 60 min cubiertos por descanso no tomado
```

---

## Modelo de segmentos propuesto

### Entidad lógica

Crear un modelo interno de cálculo, no necesariamente una tabla al inicio:

```text
SegmentoAsistencia
- Inicio
- Fin
- DuracionMinutos
- Tipo
- OrigenClasificacion
- Confianza
- RequiereRevision
- Observacion
```

### Tipos de segmento

```text
Trabajo
Descanso
Permiso
SalidaTemporal
Extra
NoConsiderar
Ambiguo
```

### Origen de clasificación

```text
Manual
MarcacionClasificada
ReglaDescanso
ReglaPermiso
ReglaHorario
Inferido
Ambiguo
```

---

## Reglas de clasificación sugeridas

### Regla 1: resolución manual manda

Si RRHH ya clasificó un tramo, esa clasificación debe tener prioridad.

Ejemplo:

```text
11:00-11:25 = salida temporal
```

No debe recalcularse como descanso en el siguiente reproceso.

---

### Regla 2: descanso configurado

Si un segmento cae cerca del descanso configurado, clasificarlo como descanso.

Debe considerar tolerancias:

```text
Descanso configurado: 13:00-14:00
Segmento real: 12:55-13:58
Clasificación: descanso probable
```

---

### Regla 3: trabajo dentro de jornada

Los segmentos que caen dentro de la jornada programada y no son descanso, permiso o salida temporal deben clasificarse como trabajo.

---

### Regla 4: extra fuera de jornada

Los segmentos de trabajo antes de la entrada programada o después de la salida programada deben clasificarse como extra candidato.

Pero solo deben ser extra automático si cumplen el mínimo configurado.

Ejemplo:

```text
Mínimo extra: 30 min
Trabajo antes del turno: 20 min
Resultado: no computable como extra automático
```

---

### Regla 5: ambigüedad requiere revisión

Debe requerir revisión cuando:

- falta una marca,
- hay número impar de marcaciones,
- un segmento puede ser descanso o salida temporal,
- hay extra y salida anticipada fuerte en el mismo día,
- hay retardo y extra posterior que podría compensarlo,
- hay salidas fuera de horario sin clasificación,
- la jornada calculada supera límites razonables.

---

## Cambios técnicos propuestos

### Fase 1: diagnóstico sin cambiar cálculo final

Objetivo: entender mejor los casos reales antes de cambiar nómina.

Tareas:

1. Crear un generador de segmentos en `RrhhAsistenciaProcessor`.
2. Guardar o mostrar un diagnóstico textual por día.
3. Agregar al modal de corrección una vista compacta de segmentos.
4. Comparar cálculo actual contra cálculo por segmentos.
5. Marcar diferencias sospechosas.

Resultado esperado:

```text
El sistema sigue calculando igual, pero RRHH ya puede ver por qué salen los números.
```

---

### Fase 2: cálculo paralelo por segmentos

Objetivo: implementar el nuevo cálculo sin romper el actual.

Tareas:

1. Crear un servicio interno de análisis por segmentos.
2. Calcular:
   - minutos trabajados reales,
   - minutos computables,
   - descansos,
   - permisos,
   - salidas temporales,
   - extra candidato,
   - extra automático,
   - salida anticipada bruta,
   - salida anticipada descontable.
3. Comparar el resultado nuevo contra `RrhhAsistencia` actual.
4. Registrar diferencias en observaciones o logs técnicos.

Resultado esperado:

```text
El sistema puede validar si el cálculo nuevo resuelve casos inconsistentes.
```

---

### Fase 3: corrección de salida anticipada efectiva

Objetivo: evitar descuentos inflados.

Tareas:

1. Separar salida anticipada bruta y efectiva.
2. Restar descansos no tomados confirmados.
3. Restar permisos aprobados aplicables.
4. Restar banco de horas aprobado si corresponde.
5. Mostrar en semanal la salida anticipada efectiva.

Resultado esperado:

```text
La salida anticipada ya no se infla por descansos o permisos.
```

---

### Fase 4: corrección de tiempo extra

Objetivo: evitar doble conteo.

Tareas:

1. Calcular extra desde segmentos de trabajo fuera del turno.
2. Eliminar suma duplicada de entrada anticipada y bloque previo cuando se traslapan.
3. Aplicar mínimo de tiempo extra por segmento o por día, según configuración.
4. Mantener el extra en estado pendiente hasta aprobación si hay ambigüedad.

Resultado esperado:

```text
El extra detectado representa tiempo real fuera de jornada y no intervalos duplicados.
```

---

### Fase 5: soporte robusto para turnos nocturnos

Objetivo: evitar cálculos con jornada cero o negativos.

Tareas:

1. Normalizar turno a rango de fecha/hora.
2. Ajustar salida al día siguiente cuando cruce medianoche.
3. Buscar marcaciones en ventana extendida según turno.
4. Asignar la asistencia al día operativo correcto.
5. Agregar pruebas de turno nocturno.

Resultado esperado:

```text
Turnos 22:00-06:00 y similares calculan jornada, extra y salidas correctamente.
```

---

### Fase 6: UI de revisión por segmentos

Objetivo: que RRHH pueda corregir sin pelearse con el cálculo.

Tareas:

1. Mostrar timeline compacto del día.
2. Mostrar segmentos alternados:

```text
Trabajo | Descanso | Trabajo | Salida temporal | Trabajo | Extra
```

3. Permitir cambiar tipo de segmento.
4. Permitir marcar descanso aplicado o no aplicado.
5. Permitir confirmar salida anticipada descontable.
6. Permitir aprobar extra a pago o banco.

Resultado esperado:

```text
RRHH puede resolver casos raros desde una sola vista clara.
```

---

### Fase 7: pruebas automatizadas

Casos mínimos de prueba:

1. Jornada normal sin descansos marcados.
2. Jornada normal con descanso marcado.
3. Entrada anticipada menor al mínimo de extra.
4. Entrada anticipada mayor al mínimo de extra.
5. Bloque extra antes del turno.
6. Bloque extra después del turno.
7. Entrada tarde con salida tarde compensatoria.
8. Salida anticipada por descanso no tomado.
9. Salida temporal.
10. Permiso parcial.
11. Marca faltante.
12. Número impar de marcaciones.
13. Dos descansos.
14. Descanso excedido.
15. Día no laborable trabajado.
16. Festivo trabajado.
17. Turno nocturno.
18. Reproceso con resoluciones manuales existentes.

---

## Criterios de aceptación

El cambio se considera correcto cuando:

- El sistema no doble cuenta extra.
- La salida anticipada descontable se distingue de la bruta.
- Los descansos no tomados no inflan descuentos sin revisión.
- Los segmentos ambiguos requieren revisión.
- La UI explica cómo se calculó cada día.
- El resumen semanal muestra valores coherentes.
- Los reprocesos no pierden resoluciones manuales vigentes.
- Los turnos nocturnos no generan jornadas en cero.
- Hay pruebas automatizadas para los casos principales.

---

## Prioridad recomendada

### Alta

1. Diagnóstico por segmentos.
2. Evitar doble conteo de extra.
3. Separar salida anticipada bruta y descontable.
4. Preservar resoluciones manuales en reprocesos.

### Media

1. UI de timeline por segmentos.
2. Reglas de descanso no tomado.
3. Validación de casos ambiguos.

### Baja

1. Persistir segmentos como tabla histórica.
2. Reportes avanzados de auditoría.
3. Configuración fina por empresa para tolerancias por tipo de segmento.

---

## Recomendación final

No conviene corregir solo una fórmula aislada. El problema viene de interpretación de jornada completa.

La solución robusta es introducir un cálculo por segmentos, primero como diagnóstico y después como fuente principal del cálculo.

El flujo final debería ser:

```text
Marcaciones -> Segmentos -> Clasificación -> Cálculo -> Revisión RRHH -> Nómina
```

Esto permite que la asistencia sea explicable, corregible y auditable.
