# Módulo `Ventas`

## Objetivo
Este módulo concentra la operación comercial: clientes, productos, asignación de productos por cliente, pedidos y cotizaciones.

## Páginas incluidas
- `MundoVs/Components/Pages/Ventas/Clientes.razor`
- `MundoVs/Components/Pages/Ventas/Productos.razor`
- `MundoVs/Components/Pages/Ventas/ProductosCliente.razor`
- `MundoVs/Components/Pages/Ventas/Pedidos.razor`
- `MundoVs/Components/Pages/Ventas/Presupuestos.razor`

## Qué información maneja
- catálogo de clientes,
- catálogo de productos,
- variantes y SKU por producto,
- relación cliente-producto,
- tallas disponibles por cliente,
- fracciones reutilizables por cliente,
- pedidos generales,
- cotizaciones por producto,
- costos productivos que alimentan precio final,
- pagos y seguimiento relacionados con pedidos.

## Fuentes técnicas principales
### `Clientes.razor`
Usa:
- `IClienteRepository`
- `IAppConfigRepository`

Datos principales:
- `Cliente`
- consecutivo `CLI-` desde `AppConfig`

### `Productos.razor`
Usa:
- `IProductoRepository`
- `IAppConfigRepository`

Datos principales:
- `Producto`
- `ProductoVariante`
- consecutivo `PROD-` desde `AppConfig`

Comportamiento actual:
- el producto se captura como producto base,
- puede marcar si usa variantes,
- puede generar SKU por talla, por color o por ambas dimensiones,
- si usa talla sin otra variación, cada talla queda como SKU,
- si usa color y talla, cada combinación color+talla queda como SKU.

### `ProductosCliente.razor`
Usa:
- `IClienteRepository`
- `IProductoRepository`
- `IProductoClienteRepository`

Datos principales:
- `ProductoCliente`
- `Cliente`
- `Producto`

Comportamiento actual:
- la asignación sigue siendo por producto base,
- las tallas permitidas y fracciones se administran por cliente en `Clientes.razor`,
- los SKU disponibles para el pedido se filtran después usando las tallas permitidas del cliente.

Regla importante del proyecto:
- cuando un cliente no tiene productos asignados, la lista debe quedar vacía y no cargar todos los productos como fallback.

### `Pedidos.razor`
Usa:
- `IPedidoRepository`
- `IClienteRepository`
- `IProductoRepository`
- `IRepository<PedidoDetalle>`

Datos principales:
- `Pedido`
- `PedidoDetalle`
- `PedidoDetalleTalla`
- `ProductoVariante`
- `Cliente`

Comportamiento actual:
- el pedido parte de un producto base asignado al cliente,
- si el producto usa variación por color, el pedido pide primero la variación,
- la fracción sigue siendo una plantilla por talla,
- cada talla del pedido intenta resolverse a un `ProductoVarianteId`,
- el sistema no permite guardar si falta resolver un SKU válido para una talla o variación.

### `Presupuestos.razor`
Usa:
- `IRepository<CotizacionSerigrafia>`
- `IRepository<CotizacionDetalle>`
- `IProductoRepository`
- `IRepository<MateriaPrima>`
- `IRepository<Insumo>`
- `IRepository<Posicion>`
- `IRepository<GastoFijo>`
- `IRepository<TipoProceso>`
- `IDbContextFactory<CrmDbContext>`

Datos principales:
- `CotizacionSerigrafia`
- `CotizacionDetalle`
- `CotizacionSerigrafiaProceso`
- `Producto`
- `MateriaPrima`
- `Insumo`
- `Posicion`
- `TipoProceso`
- `GastoFijo`

## Entidades y tablas relacionadas
Ver en `CrmDbContext.cs`:
- `Clientes`
- `Productos`
- `ProductosVariantes`
- `ProductosClientes`
- `CatalogoTallasCalzado`
- `ClientesTallasCalzado`
- `ClientesFraccionesCalzado`
- `ClientesFraccionesCalzadoDetalle`
- `Pedidos`
- `PedidoDetalles`
- `PedidosDetalleTalla`
- `PedidoConceptos`
- `CotizacionesSerigrafia`
- `CotizacionDetalles`
- `CotizacionSerigrafiaProcesos`
- `PagosPedido`
- `MateriasPrimas`
- `Insumos`
- `Posiciones`
- `GastosFijos`
- `TiposProceso`

## Dónde sale cada dato
### Código de cliente o producto
- origen principal: `AppConfigs`
- acceso técnico: `IAppConfigRepository.GetNextCodeAsync`

### Datos comerciales del cliente
- origen principal: `Cliente`
- acceso técnico: `IClienteRepository`

### Datos del catálogo de producto
- origen principal: `Producto`
- detalle vendible por SKU: `ProductoVariante`
- acceso técnico: `IProductoRepository`

### Productos permitidos por cliente
- origen principal: `ProductoCliente`
- acceso técnico: `IProductoClienteRepository.GetByClienteAsync`

### Pedido general
- origen principal: `Pedido`
- detalle complementario: `PedidoDetalle`
- desglose por talla: `PedidoDetalleTalla`
- SKU resuelto: `ProductoVariante`
- acceso técnico: `IPedidoRepository`

### Tallas y fracciones por cliente
- origen principal:
  - `ClienteTallaCalzado`
  - `ClienteFraccionCalzado`
  - `ClienteFraccionCalzadoDetalle`
- uso actual:
  - filtrar tallas válidas del cliente,
  - prellenar talla base,
  - convertir una fracción en cantidades por talla dentro del pedido.

### Precio sugerido o costo de cotización
- origen principal combinado:
  - `CotizacionSerigrafia`
  - `CotizacionDetalle`
  - `MateriaPrima`
  - `Insumo`
  - `TipoProceso`
  - `Posicion`
  - `GastoFijo`

### Precio contado y crédito
- se calculan en la pantalla de cotizaciones con base en costos acumulados y utilidad.

## Preguntas futuras y dónde buscar
### ¿De dónde salen los productos que puede elegir un cliente?
Buscar en:
1. `Ventas/ProductosCliente.razor`
2. `IProductoClienteRepository`
3. entidad `ProductoCliente`

### ¿Dónde se generan y guardan los SKU?
Buscar en:
1. `Ventas/Productos.razor`
2. entidad `ProductoVariante`
3. `CrmDbContext.cs` en `ProductosVariantes`

### ¿Dónde se resuelve la variación elegida a SKU dentro del pedido?
Buscar en:
1. `Ventas/Pedidos.razor`
2. `PedidoDetalle.VariacionValor`
3. `PedidoDetalleTalla.ProductoVarianteId`
4. `ProductoVariante`

### ¿Dónde se genera el consecutivo de clientes o productos?
Buscar en:
1. `IAppConfigRepository`
2. `AppConfigRepository`
3. tabla `AppConfigs`

### ¿Dónde se calcula el precio final de una cotización?
Buscar en:
1. `Ventas/Presupuestos.razor`
2. `CotizacionSerigrafia`
3. `CotizacionDetalle`
4. procesos, posiciones, insumos y gastos fijos involucrados

### ¿Dónde se obtiene la lista de pedidos?
Buscar en:
1. `Ventas/Pedidos.razor`
2. `IPedidoRepository`
3. entidad `Pedido`

## Dónde buscar primero
- `MundoVs/Components/Pages/Ventas/`
- `MundoVs/Core/Interfaces/IClienteRepository.cs`
- `MundoVs/Core/Interfaces/IProductoRepository.cs`
- `MundoVs/Core/Interfaces/IProductoClienteRepository.cs`
- `MundoVs/Core/Interfaces/IPedidoRepository.cs`
- `MundoVs/Core/Entities/ProductoVariante.cs`
- `MundoVs/Core/Entities/PedidoDetalleTalla.cs`
- `MundoVs/Infrastructure/Repositories/AppConfigRepository.cs`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
