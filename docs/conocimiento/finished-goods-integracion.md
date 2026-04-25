# Finished Goods (`FG`) – propuesta e integración inicial

## Objetivo
Separar el cumplimiento productivo del cumplimiento logístico:

- `Producido` significa que la manufactura terminó.
- La entrega sigue naciendo desde el pedido y su nota de entrega.
- `Finished goods` representa existencia física disponible por `SKU`.

## Principios de negocio
1. La producción se captura y cierra desde `PedidoSeguimiento`.
2. La entrega se registra desde el pedido, no desde una pantalla genérica de inventario.
3. Cuando un `SKU` termina todos sus procesos, puede ingresar a `FG`.
4. `FG` no sustituye al pedido; solo controla existencia física.
5. Un pedido producido pero no entregado debe poder mostrar una observación como `Entrega pendiente`.

## Liga entre pedido y `FG`
La relación operativa se propone así:

- `Pedido`
  - compromiso comercial
- `PedidoDetalleTalla`
  - detalle vendible por talla / `SKU`
- `InventarioFinishedGood`
  - existencia agregada por cliente + `SKU`
- `MovimientoFinishedGood`
  - trazabilidad de entradas, ajustes y futuras salidas por nota

### Trazabilidad mínima
`MovimientoFinishedGood` ya deja listos campos opcionales para ligar:

- `PedidoId`
- `PedidoSerigrafiaId`
- `PedidoDetalleTallaId`
- `NotaEntregaId`

Con esto se puede evolucionar a un flujo donde:

- producción terminada -> entrada a `FG`
- nota de entrega -> salida de `FG`

## Modelo inicial implementado
### `InventarioFinishedGood`
Existencia agregada por:

- empresa
- cliente
- producto
- variante
- `SKU`
- talla
- color

Campos base:

- `CantidadDisponible`
- `CantidadReservada`
- `Ubicacion`
- `Observaciones`

### `MovimientoFinishedGood`
Movimiento de auditoría para `FG`.

Tipos contemplados:

- `Ajuste`
- `IngresoProduccion`
- `SalidaNotaEntrega`
- `CancelacionNota`

## Pantalla integrada
Se agregó `Inventario FG` con ruta:

- `/inventario/finished-goods`

### Alcance actual
- filtro por cliente
- filtro por `SKU`
- búsqueda libre por producto, talla, color o ubicación
- ajuste manual de existencia
- sin movimientos de entrega desde la pantalla

### Regla intencional
Desde esta página:

- sí se puede `ajustar`
- no se puede `entregar`

La salida por entrega debe seguir ocurriendo desde el pedido.

## Capacidades agregadas
### Inventario FG
- `inventario.fg.ver`
- `inventario.fg.ajustar`
- `inventario.fg.ingresar`

### Producción cerrada
- `pedidos.produccion.reabrir`

Esta última permite reabrir checks / fechas de proceso cuando el pedido ya está en estado productivo cerrado.

## Ajuste aplicado en producción
En `PedidoSeguimiento`:

- si el pedido está `Producido`, `Entregado`, `Facturado` o `Pagado`, los checks y fechas de proceso quedan bloqueados
- solo se habilitan si el usuario tiene `pedidos.produccion.reabrir`
- si el pedido está `Producido` y todavía hay piezas pendientes, se muestra observación `Producción completada · Entrega pendiente`

## Ingreso inicial desde producción
En la pantalla de seguimiento:

- se detectan los `SKU` que ya completaron todos sus procesos
- si aún no tienen movimiento `IngresoProduccion`, se listan como pendientes de ingreso a `FG`
- el usuario con capacidad `inventario.fg.ingresar` puede:
  - ingresar uno por uno
  - ingresar todos los pendientes

### Regla aplicada
La detección usa `PedidoDetalleTalla` como unidad operativa del `SKU` vendido y `ProductoVarianteId` como llave para consolidar existencia en `InventarioFinishedGood`.

### Efecto del ingreso
Al ingresar un `SKU` completo:

1. se crea o actualiza `InventarioFinishedGood`
2. se incrementa `CantidadDisponible`
3. se registra `MovimientoFinishedGood` con tipo `IngresoProduccion`
4. se liga el movimiento con:
   - `PedidoId`
   - `PedidoSerigrafiaId`
   - `PedidoDetalleTallaId`

## Flujo operativo recomendado
### Fase actual
1. producir pedido
2. cerrar procesos
3. visualizar observación de entrega pendiente
4. consultar / ajustar `FG` manualmente si hace falta
5. entregar desde el pedido

### Fase siguiente recomendada
1. detectar cuando un `PedidoDetalleTalla` complete todos sus procesos
2. preguntar si se desea ingresar a `FG`
3. crear entrada automática en `InventarioFinishedGood`
4. registrar `MovimientoFinishedGood` tipo `IngresoProduccion`

## Integración con nota de entrega
La nota sigue creándose desde `PedidoSeguimiento`, pero al guardarse ahora hace también:

1. validación de existencia en `FG` por `SKU`
2. descuento de inventario terminado
3. registro de `MovimientoFinishedGood` tipo `SalidaNotaEntrega`
4. reversa del descuento si la generación del PDF falla

### Regla aplicada
- si una talla seleccionada para entrega no tiene `FG` suficiente, la nota no se genera
- la validación usa `PedidoDetalleTalla` + `ProductoVarianteId`
- la captura de entrega muestra la columna `FG disp.` para apoyar el surtido

### Pendiente posterior
- agregar reversa automática cuando una nota emitida se cancele después
- evaluar si habrá salida por lotes/origen productivo en vez de salida agregada por `SKU`

## Consideraciones pendientes
1. definir si `FG` se manejará agregado por `SKU` o también por lote/origen productivo
2. definir si habrá `reservas` antes de la nota
3. decidir si el ingreso a `FG` será automático, manual o mixto
4. agregar una tabla puente si se requiere trazabilidad por lote/origen de pedido
5. implementar cancelación de nota con reingreso explícito a `FG`

## Siguiente fase sugerida
Implementar ingreso automático a `FG` cuando un `SKU` termine todos sus procesos y preparar la salida automática desde la nota de entrega del pedido.
