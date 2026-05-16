# Plan técnico — permisos parciales, descansos dinámicos y banco de horas

## Objetivo
Corregir el manejo de permisos parciales que cruzan **n descansos dinámicos** para que el sistema use un **permiso neto aplicable** consistente en:

- modal de asistencias
- banco de horas
- retardo / salida anticipada
- faltante descontable
- resumen semanal
- snapshot de prenómina / nómina

## Principio funcional
El usuario puede capturar un permiso bruto, pero el sistema debe calcular:

- permiso bruto
- descansos no pagados cruzados
- permiso neto aplicable

### Regla central
Solo los **descansos no pagados** que se crucen con el rango del permiso:
- no se descuentan del banco
- no cuentan como permiso
- no cuentan como faltante
- no se suman como trabajo

Los descansos **pagados** no se excluyen del permiso.

## Fase 1. Modelo funcional y contratos
### 1.1 Definir conceptos formales
Agregar un contrato interno claro para trabajar con permisos del día:

- `PermisoBrutoMinutos`
- `MinutosDescansosNoPagadosCruzados`
- `PermisoNetoMinutos`
- `MinutosDescansosPagadosCruzados`
- `DescansosCruzadosDetalle`

### 1.2 Crear un resultado de evaluación
Crear un DTO/record interno, por ejemplo:

- `RrhhPermisoEvaluado`
- o `RrhhPermisoCruceDescansosResult`

con:
- rango permiso
- bruto
- neto
- descansos impactados
- observaciones

### 1.3 Mantener compatibilidad
No romper el modelo actual de `RrhhAusencia`.
Si no quieres tocar BD al inicio:
- seguir guardando `Horas`
- pero tratarlas como **netas**
- y calcular el bruto solo en UI / análisis

## Fase 2. Motor de cruce permiso–descansos
### 2.1 Crear servicio o helper dedicado
Crear una pieza única, por ejemplo:

- `IRrhhPermisoTurnoService`
- `RrhhPermisoTurnoService`

Responsable de:
- recibir turno + detalle del día + rango del permiso
- evaluar cruces con todos los descansos dinámicos

### 2.2 Soportar n descansos
Tomar `TurnoBaseDetalle.Descansos`
y recorrer todos los descansos activos ordenados por `Orden`.

### 2.3 Regla de intersección
Para cada descanso:
- calcular intersección entre:
  - rango del permiso
  - rango del descanso
- si no hay cruce: 0
- si hay cruce parcial: solo minutos parciales
- si hay cruce total: minutos completos

### 2.4 Excluir solo no pagados
Si `EsPagado == false`:
- restar intersección del permiso neto

Si `EsPagado == true`:
- no restar del permiso neto

### 2.5 Casos borde
Debe cubrir:
- permiso inicia antes del descanso y termina dentro
- permiso inicia dentro del descanso y termina después
- permiso cubre varios descansos
- permiso cubre todo el turno
- permiso menor al descanso
- días sin turno
- turnos con descanso inválido o incompleto

## Fase 3. Captura UX del permiso
### 3.1 Mantener el flujo actual simple
No reemplazar inmediatamente el input actual.
Primero agregar una evaluación previa al guardar.

### 3.2 Modal de confirmación de neteo
Si el permiso capturado cruza descansos no pagados:
mostrar modal de confirmación con:

- permiso bruto capturado
- descansos no pagados cruzados
- permiso neto sugerido
- detalle de descansos afectados

### 3.3 Acciones del modal
Botones:
- **Aplicar neto sugerido**
- **Conservar bruto capturado**
- **Cancelar y editar**

### 3.4 Recomendación funcional
La opción recomendada debe ser:
- **Aplicar neto sugerido**

### 3.5 Trazabilidad
Registrar en observaciones/bitácora algo como:
- bruto capturado
- descanso cruzado
- neto aplicado
- descansos afectados por orden

## Fase 4. Persistencia del permiso
### 4.1 Decidir qué se guarda
Recomendación mínima:
- seguir guardando en `RrhhAusencia.Horas` el **neto aplicado**

### 4.2 Opcional: conservar bruto como referencia
Sin tocar schema, se puede guardar en `Observaciones` o bitácora:
- bruto original
- neto final
- descansos excluidos

### 4.3 Reproceso del día
Después de guardar:
- reprocesar el día
- recalcular asistencia
- refrescar resumen local

## Fase 5. Banco de horas
### 5.1 Corregir consumo
`AplicarPermisoConGoceBancoHorasAsync(...)`
debe usar:
- **permiso neto aplicado**
no:
- bruto capturado

### 5.2 Bitácora de banco
Registrar:
- bruto
- neto
- descansos excluidos
- saldo previo
- saldo final

### 5.3 Remoción consistente
Al quitar o editar permiso:
- eliminar consumo previo
- recalcular con el nuevo neto

## Fase 6. Política operativa del día
### 6.1 Retardo
Definir si el permiso neto se aplica primero a:
- retardo de entrada
- luego salida anticipada

Recomendación:
- mantener esa prioridad, pero usando **neto**

### 6.2 Faltante descontable
`faltante descontable = faltante neto - permiso neto - compensación`

### 6.3 Visible
Visible puede seguir siendo:
- base trabajada
- + permiso neto
- + compensación aprobada
- + extra aprobada

### 6.4 Descuento
Descuento debe seguir representando solo:
- faltante descontable real
- + descuento manual

No volver a mezclar:
- retardo
- salida anticipada

## Fase 7. Modal de asistencias
### 7.1 Resumen local
Después de guardar permiso:
recalcular con el **permiso neto aplicado**:
- retardo visible
- salida anticipada visible
- faltante remanente
- tiempo visible
- sugerido restante

### 7.2 Timeline / descansos aplicados
Agregar un renglón o nota:
- “D1 cubierto por permiso”
- “D2 no afectado”
- “15 min excluidos del permiso por descanso no pagado”

### 7.3 Header
El header debe mostrar siempre:
- retardo efectivo
- salida anticipada efectiva
- no los valores crudos

## Fase 8. Resumen semanal
### 8.1 Visible por día
Usar permiso neto aplicado.

### 8.2 Retardo y salida
Usar valores efectivos después de permiso neto.

### 8.3 Descuento
Usar solo faltante descontable real.

### 8.4 Tooltip
En tooltip diario agregar:
- permiso bruto
- descanso no pagado excluido
- permiso neto aplicado

## Fase 9. Snapshot de prenómina
### 9.1 Snapshot único
El snapshot debe basarse siempre en:
- permiso neto aplicado por día

### 9.2 Campos impactados
Actualizar coherentemente:
- `MinutosRetardo`
- `MinutosSalidaAnticipada`
- `MinutosFaltanteDescontable`
- `HorasBancoConsumidas`

### 9.3 Notas de revisión
Agregar una nota opcional:
- “Permiso neto ajustado por descansos programados: X min excluidos”

## Fase 10. Prenómina y nómina
### 10.1 Prenómina editable
Mostrar separado:
- retardo
- salida anticipada
- faltante descontable
- descuento manual

### 10.2 No duplicar conceptos
El total descontado debe salir solo de:
- faltante descontable
- descuento manual

### 10.3 Nómina
Revisar builders que convierten prenómina a nómina para asegurar que no vuelvan a recombinar:
- retardo
- salida
- faltante

## Fase 11. Pruebas
### 11.1 Unit tests del motor de cruce
Casos:
1. permiso sin cruce
2. permiso cruza 1 descanso no pagado
3. permiso cruza 1 descanso pagado
4. permiso cruza 2 descansos mixtos
5. permiso cruza parcialmente un descanso
6. permiso cubre varios descansos

### 11.2 Tests de policy
Casos:
- permiso neto cubre retardo completo
- permiso neto cubre parte del retardo
- permiso neto cubre retardo y sobra para salida
- faltante descontable queda en cero

### 11.3 Tests de snapshot
Casos:
- prenómina refleja permiso neto
- banco consumido usa neto
- descuento no duplica retardo/salida

### 11.4 Pruebas manuales clave
1. llegada tardía con descanso dentro del permiso
2. salida anticipada con descanso dentro del permiso
3. permiso con 2 descansos no pagados
4. permiso con un descanso pagado
5. permiso con goce + banco
6. permiso sin goce

## Implementación recomendada por orden
### Etapa A
- servicio de cruce permiso–descansos
- pruebas unitarias del cruce

### Etapa B
- modal de confirmación neto/bruto
- persistencia de permiso neto

### Etapa C
- banco de horas con neto
- retardo/faltante/visible usando neto

### Etapa D
- semanal
- snapshot
- prenómina
- nómina

### Etapa E
- bitácora / auditoría
- mejoras UX finales

## Recomendación final
La forma más segura es:

1. **no automatizar silenciosamente**
2. evaluar cruce con **n descansos**
3. mostrar **modal de confirmación**
4. guardar y consumir banco con **permiso neto**
5. usar ese mismo neto en todos los cálculos posteriores
