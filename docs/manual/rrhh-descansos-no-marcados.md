# RRHH - Descansos no marcados en asistencia

## Objetivo
Definir cómo debe reaccionar el sistema cuando un operador no marca sus descansos dentro de la jornada.

## Problema operativo
Hay dos escenarios distintos cuando faltan marcaciones de descanso:

1. El operador sí tomó el descanso, pero no lo marcó.
2. El operador no tomó el descanso porque salió más temprano.

Si el sistema descuenta siempre el descanso, puede castigar doble al operador. Si nunca lo descuenta, puede inflar tiempo trabajado o tiempo extra.

## Política aplicada
El sistema compara el `tiempo trabajado bruto` contra la jornada del turno:

- `Jornada bruta programada`
- `Jornada neta programada`
- `Descanso no pagado programado`

### 1. Auto descuento
Si el tiempo bruto real se parece a la jornada bruta programada dentro de la tolerancia configurada, el sistema:

- asume que el descanso no pagado sí ocurrió,
- descuenta automáticamente esos minutos,
- evita registrar tiempo extra automático por esa diferencia menor,
- deja trazabilidad en observaciones.

### 2. Confirmación operativa
Si el tiempo bruto real se parece a la jornada neta programada dentro de la tolerancia configurada, el sistema:

- no descuenta en automático,
- interpreta que puede tratarse de una salida temprana en lugar del descanso,
- marca la asistencia como `requiere revisión`.

### 3. Zona ambigua
Si la jornada cae entre las tolerancias y el tope de zona ambigua, el sistema:

- no toma una decisión automática,
- bloquea tiempo extra automático,
- deja el día pendiente de confirmación.

## Valores por defecto
Por empresa, el sistema usa estos valores iniciales:

- Coincidencia con jornada bruta: `15 min`
- Coincidencia con jornada neta: `15 min`
- Zona ambigua hasta: `30 min`

## Ubicación de la configuración
La configuración se administra en:

- `/configuracion/asistencia`

Desde esa pantalla se pueden ajustar los límites por empresa.

## Resultado esperado
Con esta política:

- no se paga tiempo extra falso por descansos no marcados,
- no se descuenta automáticamente cuando el operador probablemente salió temprano en lugar de descansar,
- los casos dudosos quedan visibles para revisión manual.
