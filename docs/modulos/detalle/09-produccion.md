# Módulo `Produccion`

## Objetivo
Este módulo concentra la vista unificada de inventario y operaciones generales de abastecimiento/movimiento interno.

## Páginas incluidas
- `MundoVs/Components/Pages/Produccion/Inventario.razor`

## Qué información maneja
- existencias de materias primas e insumos,
- tipos y categorías de inventario,
- movimientos de inventario,
- costo inventariable,
- stock mínimo y faltantes,
- altas rápidas de ítems.

## Fuentes técnicas principales
### `Inventario.razor`
Usa:
- `IDbContextFactory<CrmDbContext>`
- `AuthenticationStateProvider`
- `IAppConfigRepository`

Consulta directamente:
- `MateriasPrimas`
- `Insumos`
- `TiposInventario`
- `CategoriasInventario`
- `MovimientosInventario`

Además usa `IAppConfigRepository` para consecutivos o apoyos de alta rápida.

## Entidades y tablas relacionadas
Ver en `CrmDbContext.cs`:
- `CategoriasInventario`
- `TiposInventario`
- `MovimientoInventario`
- `MateriasPrimas`
- `Insumos`
- `AppConfigs`

## Dónde sale cada dato
### Existencia actual del item
- origen principal:
  - `MateriaPrima.Cantidad`
  - o `Insumo.Cantidad`

### Tipo del inventario
- origen principal: `TipoInventario`
- clasificación superior: `CategoriaInventario`

### Alertas de stock bajo
- origen principal:
  - cantidad actual,
  - `StockMinimo` del item.

### Costo del inventario
- origen principal:
  - cantidad actual,
  - `PrecioUnitario` del item.

### Historial de movimientos
- origen principal: `MovimientoInventario`

## Preguntas futuras y dónde buscar
### ¿Dónde ver si un material está bajo mínimo?
Buscar en:
1. `Produccion/Inventario.razor`
2. `MateriaPrima.StockMinimo` o `Insumo.StockMinimo`
3. cantidad actual del item

### ¿Dónde se define el tipo de un insumo o materia prima?
Buscar en:
1. `TipoInventario`
2. `CategoriaInventario`
3. relación del item con `TipoInventarioId`

### ¿Dónde se registran los movimientos?
Buscar en:
1. `Produccion/Inventario.razor`
2. entidad `MovimientoInventario`

## Dónde buscar primero
- `MundoVs/Components/Pages/Produccion/Inventario.razor`
- `MundoVs/Core/Entities/Inventario/TipoInventario.cs`
- `MundoVs/Core/Entities/Inventario/CategoriaInventario.cs`
- `MundoVs/Core/Entities/Inventario/MovimientoInventario.cs`
- `MundoVs/Core/Entities/Serigrafia/MateriaPrima.cs`
- `MundoVs/Core/Entities/Serigrafia/Insumo.cs`
- `MundoVs/Infrastructure/Data/CrmDbContext.cs`
