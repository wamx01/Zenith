# Submódulo `Produccion/Serigrafia`

## Objetivo
Este submódulo concentra la configuración productiva y el flujo operativo más completo del sistema: posiciones, procesos, materiales, pedidos productivos y seguimiento por talla/proceso.

## Páginas incluidas
- `MundoVs/Components/Pages/Produccion/Serigrafia/Actividades.razor`
- `MundoVs/Components/Pages/Produccion/Serigrafia/TiposProceso.razor`
- `MundoVs/Components/Pages/Produccion/Serigrafia/MateriasPrimas.razor`
- `MundoVs/Components/Pages/Produccion/Serigrafia/Insumos.razor`
- `MundoVs/Components/Pages/Produccion/Serigrafia/Pantallas.razor`
- `MundoVs/Components/Pages/Produccion/Serigrafia/GastosFijos.razor`
- `MundoVs/Components/Pages/Produccion/Serigrafia/PedidosSerigrafia.razor`
- `MundoVs/Components/Pages/Produccion/Serigrafia/PedidoSeguimiento.razor`

## Qué información maneja
- posiciones de trabajo,
- tarifa por minuto base,
- tipos de proceso,
- materias primas e insumos productivos,
- pantallas y gastos fijos,
- pedidos productivos,
- tallas y procesos por pedido,
- servicios/conceptos adicionales,
- pagos del pedido,
- seguimiento por talla y proceso,
- reparto de destajo por empleado.

## Fuentes técnicas principales
### `Actividades.razor`
Usa:
- `IRepository<Posicion>`

Fuente base:
- `Posicion`

### `TiposProceso.razor`
Usa:
- `IRepository<TipoProceso>`
- `IRepository<Posicion>`

Fuente base:
- `TipoProceso`
- `Posicion`

### `MateriasPrimas.razor`
Usa:
- `IRepository<MateriaPrima>`
- `IAppConfigRepository`
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`

Fuente base:
- `MateriaPrima`
- `TipoInventario`
- `CategoriaInventario`

### `Insumos.razor`
Usa:
- `IRepository<Insumo>`
- `IAppConfigRepository`
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`

Fuente base:
- `Insumo`
- `TipoInventario`
- `CategoriaInventario`

### `Pantallas.razor`
Usa:
- `IRepository<Pantalla>`

Fuente base:
- `Pantalla`

### `GastosFijos.razor`
Usa:
- `IRepository<GastoFijo>`

Fuente base:
- `GastoFijo`

### `PedidosSerigrafia.razor`
Usa:
- `IPedidoSerigrafiaRepository`
- `IClienteRepository`
- `IPedidoRepository`
- `IProductoRepository`
- `IProductoClienteRepository`
- `IRepository<TipoProceso>`
- `IRepository<PedidoDetalle>`
- `CrmDbContext`
- `AuthenticationStateProvider`

Fuente base:
- `PedidoSerigrafia`
- `Pedido`
- `PedidoDetalle`
- `PedidoDetalleTalla`
- `ProductoVariante`
- `PedidoSerigrafiaTalla`
- `PedidoSerigrafiaTallaProceso`
- `PedidoConcepto`
- `CotizacionSerigrafia`
- `PagoPedido`
- `ProductoCliente`

### `PedidoSeguimiento.razor`
Usa:
- `IPedidoRepository`
- `CrmDbContext`
- `AuthenticationStateProvider`

Fuente base:
- `Pedido`
- `PedidoSerigrafia`
- `PedidoDetalle`
- `PedidoDetalleTalla`
- `PedidoConcepto`
- `PedidoSerigrafiaTallaProceso`
- `RegistroDestajoProceso`
- `Empleado`

## Entidades y tablas relacionadas
Ver en `CrmDbContext.cs`:
- `Posiciones`
- `TiposProceso`
- `MateriasPrimas`
- `Insumos`
- `Pantallas`
- `GastosFijos`
- `Pedidos`
- `PedidoDetalles`
- `PedidosDetalleTalla`
- `ProductosVariantes`
- `PedidoConceptos`
- `PedidosSerigrafia`
- `PedidoSerigrafiaProcesoDetalles`
- `PedidoSerigrafiaTallas`
- `PedidoSerigrafiaTallaProcesos`
- `RegistroDestajoProceso`
- `CotizacionesSerigrafia`
- `PagosPedido`
- `ProductosClientes`
- `Empleados`

## Dónde sale cada dato
### Tarifa por minuto de una posición
- origen principal: `Posicion.TarifaPorMinuto`
- pantalla de mantenimiento: `Actividades.razor`

### Proceso productivo y su posición
- origen principal: `TipoProceso`
- relación: `TipoProceso.PosicionId`

### Material o insumo de producción
- origen principal:
  - `MateriaPrima`
  - `Insumo`
- clasificación: `TipoInventario`

### Pantallas disponibles o su estado
- origen principal: `Pantalla`
- campos clave:
  - código,
  - malla,
  - dimensiones,
  - estado,
  - usos,
  - diseño para.

### Gasto fijo usado en costeo
- origen principal: `GastoFijo`

### Pedido productivo principal
- origen principal combinado:
  - `Pedido`
  - `PedidoDetalle`
  - `PedidoDetalleTalla`
  - `PedidoSerigrafia`

Regla actual:
- el pedido define primero el producto base,
- si el producto usa color como variación, la combinación de color queda fijada en el detalle,
- las tallas del pedido se resuelven a `ProductoVariante` usando esa variación y las tallas permitidas del cliente,
- la producción sigue trabajando por talla, pero el origen comercial del renglón ya puede rastrearse al SKU correcto.

### Producto permitido para capturar en un pedido
- origen principal: `ProductoCliente`
- acceso técnico: `IProductoClienteRepository.GetByClienteAsync`

### Cotización base usada por el pedido
- origen principal: `CotizacionSerigrafia`
- vinculación técnica: `PedidoDetalle.CotizacionSerigrafia`

### Tallas del pedido
- origen principal: `PedidoSerigrafiaTalla`

Relación operativa:
- el pedido comercial puede venir de una fracción por talla,
- cada talla comercial del detalle puede apuntar a `PedidoDetalleTalla.ProductoVarianteId`,
- `PedidoSerigrafiaTalla` mantiene la cantidad operativa por talla para producción y seguimiento.

### Avance por talla y proceso
- origen principal: `PedidoSerigrafiaTallaProceso`
- campos útiles:
  - si está completado,
  - fecha de paso,
  - empleado asignado.

### Servicios o conceptos adicionales del pedido
- origen principal: `PedidoConcepto`

### Pagos del pedido
- origen principal: `PagoPedido`

### Destajo repartido por proceso
- origen principal: `RegistroDestajoProceso`
- se relaciona con:
  - `PedidoSerigrafiaTallaProceso`
  - `Empleado`

## Preguntas futuras y dónde buscar
### ¿De dónde sale la tarifa base de un proceso?
Buscar en:
1. `TiposProceso.razor`
2. `TipoProceso`
3. `Posicion.TarifaPorMinuto`

### ¿Dónde ver el avance de una talla por proceso?
Buscar en:
1. `PedidoSeguimiento.razor`
2. `PedidoSerigrafiaTallaProceso`

### ¿Dónde se guarda quién trabajó una parte del proceso?
Buscar en:
1. `PedidoSeguimiento.razor`
2. `RegistroDestajoProceso`
3. `Empleado`

### ¿Dónde salen los pagos del pedido?
Buscar en:
1. `PedidosSerigrafia.razor`
2. `PagoPedido`

### ¿Dónde ver qué cotización alimenta un pedido?
Buscar en:
1. `PedidoDetalle`
2. `CotizacionSerigrafia`
3. `PedidosSerigrafia.razor`
4. `PedidoSeguimiento.razor`

### ¿Dónde se amarra la talla del pedido con el SKU real?
Buscar en:
1. `PedidoDetalleTalla.ProductoVarianteId`
2. `ProductoVariante`
3. `Ventas/Pedidos.razor`

## Dónde buscar primero
- `MundoVs/Components/Pages/Produccion/Serigrafia/`
- `MundoVs/Core/Interfaces/IPedidoSerigrafiaRepository.cs`
- `MundoVs/Infrastructure/Repositories/PedidoSerigrafiaRepository.cs`
- `MundoVs/Core/Entities/Serigrafia/TipoProceso.cs`
- `MundoVs/Core/Entities/Serigrafia/RegistroDestajoProceso.cs`
- `MundoVs/Core/Entities/Serigrafia/PedidoSerigrafia.cs`
- `MundoVs/Core/Entities/Serigrafia/PedidoSerigrafiaTalla.cs`
- `MundoVs/Core/Entities/Serigrafia/PedidoSerigrafiaTallaProceso.cs`
- `MundoVs/Core/Entities/PedidoDetalle.cs`
- `MundoVs/Core/Entities/PedidoDetalleTalla.cs`
- `MundoVs/Core/Entities/ProductoVariante.cs`
- `MundoVs/Core/Entities/PedidoConcepto.cs`
- `MundoVs/Core/Entities/PagoPedido.cs`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
