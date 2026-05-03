# Plan robusto · Nota consolidada multipedido

## Objetivo
Mover la creación de notas de entrega consolidadas fuera de `PedidoSeguimiento` y llevarla a la pantalla de lista de pedidos (`PedidosSerigrafia` / lista operativa de pedidos), con un flujo por cliente que permita seleccionar varios pedidos del mismo cliente y generar una sola nota con trazabilidad operativa real.

## Visión funcional
El usuario entra a la lista de pedidos, presiona un botón tipo `Crear nota`, selecciona un cliente y ve únicamente los pedidos elegibles de ese cliente. Después puede elegir uno o varios pedidos, revisar qué productos/servicios están realmente entregables, usar una opción de `Cargar lo disponible`, previsualizar el documento y generar una sola nota consolidada.

## Decisión arquitectónica
La solución robusta no debe depender solo de `NotaEntrega.PedidoId`. Se conservará `PedidoId` como pedido principal por compatibilidad, pero la operación real debe apoyarse en relaciones y asignaciones explícitas.

## Modelo objetivo

### Entidades principales
- `NotaEntrega`
- `NotaEntregaPedido`
- `NotaEntregaAsignacion`

### Rol de cada entidad
- `NotaEntrega`: documento comercial principal.
- `NotaEntregaPedido`: relación entre la nota y todos los pedidos incluidos.
- `NotaEntregaAsignacion`: detalle exacto de qué se entregó, desde qué pedido y de qué línea salió.

## Nueva entidad propuesta: NotaEntregaAsignacion
Debe registrar la trazabilidad operativa real por línea entregada.

Campos sugeridos:
- `Id`
- `EmpresaId`
- `NotaEntregaId`
- `PedidoId`
- `PedidoDetalleId` nullable
- `PedidoDetalleTallaId` nullable
- `PedidoConceptoId` nullable
- `TipoOrigen` (`Producto`, `Servicio`)
- `Cantidad`
- `CantidadFgTomada`
- `PrecioUnitario`
- `Importe`
- `EsParcial`
- `CreatedAt`
- `CreatedBy`
- `IsActive`

## Regla clave de elegibilidad
No bloquear pedidos por el simple hecho de tener una nota previa.

Se debe bloquear solo cuando el pedido ya no tenga saldo entregable real.

### Un pedido es elegible si:
- pertenece al cliente seleccionado
- está activo
- no está cancelado
- tiene productos o servicios realmente entregables pendientes
- su disponibilidad no fue agotada por notas activas previas

### Un pedido no es elegible si:
- está cancelado
- está completamente entregado
- todo su FG disponible ya fue comprometido o entregado
- todos sus servicios ya fueron entregados
- no tiene saldo comercial entregable

## Motor de elegibilidad
Se recomienda crear un servicio dedicado, por ejemplo:
- `INotaEntregaConsolidadaService`
- `NotaEntregaConsolidadaService`

### Responsabilidades del servicio
1. cargar pedidos elegibles del cliente
2. calcular disponibilidad real por pedido
3. calcular servicios listos por pedido
4. calcular saldo entregable real
5. construir propuesta de asignaciones
6. validar la solicitud final antes de guardar
7. generar la nota dentro de una transacción

## Modelo de vista recomendado
Crear un modelo interno de trabajo por pedido relacionado, por ejemplo `PedidoEntregaConsolidadaVm`.

Debe contener:
- pedido
- estatus operativo
- piezas pedidas
- piezas ya entregadas
- piezas pendientes reales
- FG disponible por SKU/talla
- servicios listos
- elegible o no
- motivo de bloqueo
- selección del usuario

Y a nivel de línea:
- SKU
- talla
- cantidad pedida
- cantidad ya entregada
- cantidad disponible para nota
- cantidad a incluir en la nota
- pedido origen

## Pantalla objetivo
La creación de la nota consolidada debe vivir en la lista de pedidos, no en el seguimiento individual.

### Lugar sugerido
`PedidosSerigrafia` o la lista operativa de pedidos del módulo de producción/pedidos.

### Flujo UI sugerido
1. botón `Crear nota`
2. modal o drawer de creación de nota consolidada
3. seleccionar cliente
4. cargar pedidos elegibles del cliente
5. seleccionar pedidos
6. ver líneas entregables por pedido
7. usar `Cargar lo disponible` si se desea
8. ajustar cantidades manualmente
9. previsualizar resumen
10. generar nota

## Reglas UI
- no mostrar pedidos cancelados
- no mostrar pedidos completamente entregados
- no mostrar pedidos agotados por notas activas
- mostrar motivo cuando un pedido no sea elegible
- mostrar un resumen compacto por pedido: fecha, total, piezas pendientes, FG disponible, servicios listos
- permitir expandir para ver detalle por línea

## Cálculo de disponibilidad real
La nota consolidada debe trabajar por línea y no solo por pedido completo.

### Productos
Para cada `PedidoDetalleTalla`:
- cantidad pedida
- cantidad entregada en notas activas
- cantidad FG disponible
- cantidad máxima tomable = mínimo entre pendiente y FG disponible

### Servicios
Para cada `PedidoConcepto`:
- verificar si está completado
- verificar si ya fue entregado en notas activas
- permitir incluirlo solo si sigue pendiente de entrega

## Opción `Cargar lo disponible`
Debe llenar automáticamente las cantidades tomables por línea usando la disponibilidad real.

### Regla
- productos: tomar solo lo realmente disponible en FG y pendiente por entregar
- servicios: tomar solo los conceptos completos y no entregados

## Construcción del documento
La nota debe generarse desde asignaciones consolidadas, no desde un único pedido.

### Encabezado
- cliente
- pedidos relacionados
- observaciones
- condición comercial

### Detalle
Opciones:
1. detalle agrupado por pedido
2. detalle agrupado por SKU/servicio con referencia de pedido origen

Recomendación inicial:
- mantener referencia visible del pedido origen para trazabilidad

## Salidas de finished goods
Cada salida FG debe ligarse a:
- `NotaEntregaId`
- `PedidoId`
- `PedidoDetalleTallaId`
- opcionalmente `NotaEntregaAsignacionId`

Esto permitirá saber exactamente qué cantidad salió de qué pedido dentro de una nota consolidada.

## Estados de pedido
No actualizar estados por heurística simple.

El estado debe derivarse de:
- producción completa o parcial
- servicios completos o parciales
- asignaciones ya entregadas
- facturación relacionada
- pagos

## Transacción de guardado
La generación de nota consolidada debe ejecutarse en una sola transacción:
1. recalcular elegibilidad
2. recalcular FG y servicios disponibles
3. crear `NotaEntrega`
4. crear `NotaEntregaPedido`
5. crear `NotaEntregaAsignacion`
6. registrar movimientos FG
7. actualizar estados derivados
8. guardar PDF

Si algo falla, hacer rollback completo.

## Compatibilidad
Durante la transición:
- conservar `NotaEntrega.PedidoId` como pedido principal
- seguir mostrando pedido principal donde aún se necesite
- extender listados y PDF con pedidos relacionados
- migrar gradualmente la lógica desde seguimiento individual al nuevo flujo

## Fases de implementación

### Fase 1
Crear base de dominio completa.
- `NotaEntregaAsignacion`
- mapeo EF
- migración

### Fase 2
Construir servicio de elegibilidad y consolidación.
- pedidos elegibles
- FG disponible
- servicios listos
- saldo entregable real

### Fase 3
Crear UI nueva en lista de pedidos.
- botón `Crear nota`
- selección por cliente
- selección de pedidos
- vista de líneas entregables

### Fase 4
Generar nota consolidada real.
- asignaciones
- salidas FG por pedido
- validación transaccional

### Fase 5
Actualizar reportes y documento.
- PDF
- notas de entrega
- CxC y trazabilidad

## Criterios de aceptación
- una nota puede incluir varios pedidos del mismo cliente
- no se pueden mezclar clientes
- no se muestran pedidos cancelados o totalmente entregados
- el sistema no bloquea un pedido solo por tener nota previa; solo si ya no tiene saldo entregable
- `Cargar lo disponible` respeta FG y servicios realmente listos
- cada salida FG queda trazada al pedido origen
- la nota y el PDF muestran pedidos relacionados
- el estado del pedido sigue siendo consistente

## Siguiente paso recomendado
Implementar primero `NotaEntregaAsignacion` y el servicio de elegibilidad consolidada. Esa es la base para que el resto del flujo sea robusto y mantenible.
