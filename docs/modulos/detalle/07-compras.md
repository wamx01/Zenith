# Módulo `Compras`

## Objetivo
Este módulo administra proveedores y cuentas por pagar.

## Páginas incluidas
- `MundoVs/Components/Pages/Compras/Proveedores.razor`
- `MundoVs/Components/Pages/Compras/CuentasPorPagar.razor`

## Qué información maneja
- catálogo de proveedores,
- documentos por pagar,
- importes, vencimientos y estatus,
- pagos parciales y totales,
- saldo pendiente por documento.

## Fuentes técnicas principales
### `Proveedores.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`

Consulta directamente:
- `Proveedores`

### `CuentasPorPagar.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `IAuditService`
- `AuthenticationStateProvider`

Consulta directamente:
- `CuentasPorPagar`
- `Proveedores`
- `PagosCxP`

## Entidades y tablas relacionadas
Ver en `CrmDbContext.cs`:
- `Proveedores`
- `CuentasPorPagar`
- `PagosCxP`
- `AuditLogs`

## Dónde sale cada dato
### Datos del proveedor
- origen principal: `Proveedor`
- campos clave:
  - código,
  - nombre,
  - razón social,
  - RFC,
  - contacto,
  - teléfono,
  - correo,
  - activo.

### Documento por pagar
- origen principal: `CuentaPorPagar`
- campos clave:
  - proveedor,
  - tipo de documento,
  - número,
  - fechas,
  - subtotal,
  - impuestos,
  - total,
  - estatus.

### Pagos aplicados a la cuenta
- origen principal: `PagoCxP`
- se relaciona con `CuentaPorPagar`
- el saldo se obtiene restando pagos acumulados al total del documento.

## Preguntas futuras y dónde buscar
### ¿Dónde sale el saldo pendiente de una cuenta?
Buscar en:
1. `Compras/CuentasPorPagar.razor`
2. `CuentaPorPagar.Total`
3. suma de `PagosCxP.Monto`

### ¿Dónde se registra un pago a proveedor?
Buscar en:
1. `Compras/CuentasPorPagar.razor`
2. entidad `PagoCxP`

### ¿Dónde se consulta el catálogo de proveedores?
Buscar en:
1. `Compras/Proveedores.razor`
2. entidad `Proveedor`

## Dónde buscar primero
- `MundoVs/Components/Pages/Compras/`
- `MundoVs/Core/Entities/Proveedor.cs`
- `MundoVs/Core/Entities/CuentaPorPagar.cs`
- `MundoVs/Core/Entities/PagoCxP.cs`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
