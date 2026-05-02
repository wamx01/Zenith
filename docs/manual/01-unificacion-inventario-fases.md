# Unificación de inventario fase por fase

## Objetivo
Unificar `MateriasPrimas` e `Insumos` en un solo catálogo de inventario escalable, manteniendo compatibilidad operativa durante la transición y evitando romper cotizaciones, pedidos, movimientos y cobranza.

## Alcance
Este plan cubre:
- modelo de datos
- migración de datos
- compatibilidad temporal
- actualización de referencias en cotizaciones y pedidos
- saneamiento final del modelo viejo

No cubre en esta primera fase:
- reportes históricos rediseñados
- reentrenamiento de usuarios
- optimizaciones visuales fuera del alcance de inventario/costos

---

## Fase 0. Preparación
### Objetivo
Levantar inventario actual de dependencias antes de tocar base de datos.

### Tareas
- Identificar todas las entidades que usan `MateriaPrimaId`.
- Identificar todas las entidades que usan `InsumoId`.
- Identificar catálogos actuales de `CategoriaInventario` y `TipoInventario`.
- Confirmar qué campos de `MateriasPrimas` e `Insumos` deben converger en un modelo único.
- Confirmar qué vistas o flujos siguen mostrando separación rígida por origen.

### Resultado esperado
Mapa completo de impacto para migrar sin pérdida funcional.

### Estado
- [ ] Pendiente

---

## Fase 1. Modelo unificado inicial
### Objetivo
Crear la nueva tabla unificada sin romper lo actual.

### Propuesta de tabla
- `logistica_inventario_item`

### Campos base esperados
- `Id`
- `EmpresaId`
- `Codigo`
- `Nombre`
- `CategoriaInventarioId`
- `TipoInventarioId`
- `UnidadMedida`
- `Cantidad`
- `PrecioUnitario`
- `StockMinimo`
- `Activo`
- auditoría base

### Tareas
- Crear entidad nueva de inventario unificado.
- Registrar `DbSet` nuevo en `CrmDbContext`.
- Configurar tabla nueva con patrón `modulo_funcion`.
- Preparar migración inicial sin eliminar tablas viejas.

### Resultado esperado
La solución puede compilar con el nuevo modelo coexistiendo con `MateriasPrimas` e `Insumos`.

### Estado
- [ ] Pendiente

---

## Fase 2. Compatibilidad de movimientos
### Objetivo
Permitir que movimientos apunten al nuevo catálogo sin romper históricos.

### Tareas
- Agregar `InventarioItemId` a `MovimientosInventario`.
- Mantener temporalmente `MateriaPrimaId` e `InsumoId`.
- Ajustar lógica para escribir preferentemente al nuevo `InventarioItemId`.
- Conservar lectura compatible con históricos mientras existan datos viejos.

### Resultado esperado
Los movimientos nuevos ya pueden depender de una sola referencia de inventario.

### Estado
- [ ] Pendiente

---

## Fase 3. Migración de datos
### Objetivo
Copiar materias primas e insumos al catálogo unificado.

### Tareas
- Migrar filas de `MateriasPrimas` a `logistica_inventario_item`.
- Migrar filas de `Insumos` a `logistica_inventario_item`.
- Mapear categoría y tipo inventario por origen actual.
- Guardar trazabilidad entre ids viejos e ids nuevos.
- Validar conteos y montos relevantes.

### Resultado esperado
El catálogo unificado contiene todos los registros operativos necesarios.

### Estado
- [ ] Pendiente

---

## Fase 4. Refactor de referencias funcionales
### Objetivo
Mover cotizaciones, procesos y pedidos al nuevo item unificado.

### Tareas
- Reemplazar en `CotizacionDetalle` el uso dual de `MateriaPrimaId` / `InsumoId`.
- Reemplazar en `TipoProcesoConsumo` el uso dual de origen.
- Actualizar comprobación de inventario en pedidos.
- Actualizar descuento automático de inventario.
- Actualizar formularios de cotización para usar un solo catálogo filtrable.

### Resultado esperado
El flujo operativo deja de depender de dos catálogos distintos para materiales.

### Estado
- [ ] Pendiente

---

## Fase 5. Ajuste de UI y catálogos
### Objetivo
Hacer visible el inventario como un solo universo escalable por tipo.

### Tareas
- Unificar selectores de materiales en cotización.
- Mostrar materiales con nombres genéricos y escalables.
- Filtrar por categoría/tipo inventario en lugar de origen rígido.
- Ajustar inventario para operar sobre un solo catálogo.

### Resultado esperado
La UI deja de hablar de tablas separadas y refleja un modelo unificado.

### Estado
- [ ] Pendiente

---

## Fase 6. Retiro del modelo viejo
### Objetivo
Eliminar deuda técnica cuando toda la operación use el modelo unificado.

### Tareas
- Dejar de escribir en `MateriasPrimas`.
- Dejar de escribir en `Insumos`.
- Eliminar columnas viejas de referencias duales.
- Eliminar tablas viejas si ya no se requieren.
- Actualizar migraciones y documentación final.

### Resultado esperado
Inventario operando completamente sobre un modelo único.

### Estado
- [ ] Pendiente

---

## Riesgos clave
- Duplicidad o pérdida de referencias durante la migración.
- Movimientos históricos sin correspondencia clara.
- Flujos de cotización/pedido que aún dependan de origen dual.
- Catálogos mal clasificados al convertir insumos y materias primas.

## Criterios de validación
- La solución compila en cada fase.
- Las cotizaciones siguen calculando materiales y costos.
- Los pedidos siguen validando y descontando inventario.
- Los movimientos nuevos quedan ligados al item unificado.
- No se duplican saldos ni existencias.

## Bitácora de avance
### Avance actual
- Se confirmó que hoy `MateriasPrimas` e `Insumos` viven en tablas separadas.
- Se validó que ambos pertenecen a la misma base de datos activa.
- Se documentó la ruta por fases antes de tocar la migración estructural.

### Próximo paso recomendado
Implementar la **Fase 1** con el modelo unificado inicial y la tabla `logistica_inventario_item` coexistiendo con el esquema actual.
