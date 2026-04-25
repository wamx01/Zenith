# Módulo `Contabilidad`

## Objetivo
Este módulo ofrece indicadores financieros, control de cobranza y estado de resultados.

## Páginas incluidas
- `MundoVs/Components/Pages/Contabilidad/Dashboard.razor`
- `MundoVs/Components/Pages/Contabilidad/CuentasPorCobrar.razor`
- `MundoVs/Components/Pages/Contabilidad/EstadoResultados.razor`

## Qué información maneja
- pedidos e ingresos por periodo,
- facturación y cobranza por cliente,
- pagos recibidos,
- saldos pendientes,
- costos operativos y utilidad,
- resumen financiero por periodos.

## Fuentes técnicas principales
### `Dashboard.razor`
Usa:
- `CrmDbContext`

Arma consultas directas para:
- pedidos del periodo,
- ingresos,
- piezas,
- pedidos pendientes,
- distribución por estado,
- ingresos por mes,
- top clientes.

### `CuentasPorCobrar.razor`
Usa:
- `CrmDbContext`

Arma consultas directas para:
- facturas por cliente,
- pagos asociados,
- saldos por pedido/factura,
- agrupación por cliente.

### `EstadoResultados.razor`
Usa:
- `CrmDbContext`

Arma consultas directas para:
- ingresos brutos y netos,
- descuentos,
- materia prima,
- insumos,
- mano de obra,
- gastos fijos,
- impuestos,
- utilidad bruta y neta.

## Entidades y tablas relacionadas
Según la pantalla, revisar en `CrmDbContext.cs`:
- `Pedidos`
- `PedidoDetalles`
- `PedidosSerigrafia`
- `PagosPedido`
- `CotizacionesSerigrafia`
- `CotizacionDetalles`
- `GastosFijos`
- `Clientes`
- `Empresas`

## Dónde sale cada dato
### Ingresos y pedidos del dashboard
- origen principal: `Pedido` y estructuras relacionadas del flujo comercial/productivo.

### Cobranza por cliente
- origen principal combinado:
  - `Pedido` o pedido productivo relacionado,
  - documento/factura asociada,
  - `PagoPedido` para pagos recibidos.

### Saldo pendiente por factura
- origen principal:
  - total facturado del pedido,
  - pagos acumulados en `PagosPedido`.

### Costos del estado de resultados
- origen principal combinado:
  - `CotizacionDetalle` para consumos y costos,
  - `GastoFijo` para gastos operativos,
  - datos de ventas/pedidos para ingresos.

### Datos de empresa mostrados en reportes
- origen principal: `Empresa`
- posibles campos: nombre, RFC y logo.

## Preguntas futuras y dónde buscar
### ¿De dónde sale el top de clientes?
Buscar en:
1. `Contabilidad/Dashboard.razor`
2. agrupaciones por cliente en pedidos/ingresos
3. entidad `Cliente`

### ¿Dónde se registra y consulta un pago recibido?
Buscar en:
1. `Contabilidad/CuentasPorCobrar.razor`
2. entidad `PagoPedido`

### ¿Cómo se calcula la utilidad neta?
Buscar en:
1. `Contabilidad/EstadoResultados.razor`
2. ingresos netos
3. costo de ventas
4. gastos operativos
5. impuestos estimados

## Dónde buscar primero
- `MundoVs/Components/Pages/Contabilidad/`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
- `MundoVs/Core/Entities/PagoPedido.cs`
- `MundoVs/Core/Entities/Serigrafia/CotizacionDetalle.cs`
- `MundoVs/Core/Entities/Serigrafia/GastoFijo.cs`
