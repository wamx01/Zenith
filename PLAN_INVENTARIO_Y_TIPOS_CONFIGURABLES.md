# Plan de implementación: inventario unificado y tipos configurables

## Objetivo
Implementar una solución genérica para varias industrias donde:

1. Los `tipos de materia prima` sean configurables desde catálogo.
2. `Materias Primas` e `Insumos` sigan separados en base de datos por ahora.
3. Exista una vista unificada de `Inventario` para consulta y operación.
4. La terminología del sistema siga siendo genérica para zapato, sombrero y serigrafía.

---

## Decisión técnica
### Se hará
- Convertir los tipos de materia prima de `enum` a catálogo en BD.
- Crear catálogo reutilizable para clasificar inventario.
- Crear una pantalla unificada de inventario.
- Mantener `MateriaPrima` e `Insumo` como entidades separadas en esta fase.

### No se hará en esta fase
- Fusionar `MateriasPrimas` e `Insumos` en una sola tabla física.
- Reescribir toda la lógica de producción alrededor de una entidad única.

### Motivo
Unificar en una sola tabla ahora implicaría cambiar demasiada lógica existente y mezclar campos que hoy son específicos. La mejor ruta es unificar la experiencia de usuario primero y dejar la fusión física como posible fase futura.

---

## Estado actual
### Materias primas
- `MateriaPrima` usa `TipoMateriaPrimaEnum`.
- Los tipos están fijos en código.
- No existe catálogo para administrarlos.

### Insumos
- `Insumo` existe por separado.
- También maneja clasificación propia.

### Menú
- Ya existe acceso a:
  - `Materias Primas`
  - `Insumos`
- No existe todavía una pantalla central de `Inventario`.

---

## Estado objetivo
### Catálogos nuevos
Se propone crear dos tablas:

#### 1. `CategoriaInventario`
Sirve para separar grandes grupos.

Campos sugeridos:
- `Id`
- `EmpresaId`
- `Nombre`
- `Codigo`
- `Orden`
- `IsActive`
- auditoría base

Valores iniciales sugeridos:
- `MATERIA_PRIMA`
- `INSUMO`

#### 2. `TipoInventario`
Sirve para clasificar elementos dentro de una categoría.

Campos sugeridos:
- `Id`
- `EmpresaId`
- `CategoriaInventarioId`
- `Nombre`
- `Codigo`
- `Orden`
- `Color`
- `IsActive`
- auditoría base

Ejemplos:
- Categoría `Materia Prima`:
  - Tinta
  - Base
  - Solvente
  - Aditivo
  - Otro
- Categoría `Insumo`:
  - Herramienta
  - Empaque
  - Consumible
  - Refacción
  - Otro

---

## Estrategia de migración
## Fase 1. Catálogos base
### Backend
1. Crear entidad `CategoriaInventario`.
2. Crear entidad `TipoInventario`.
3. Registrar ambas entidades en `DbContext`.
4. Configurar índices y relaciones.
5. Crear migración EF Core.
6. Sembrar datos iniciales por defecto.

### Reglas importantes
- Respetar `EmpresaId` si el sistema ya es multiempresa.
- Insertar catálogos base sin romper datos existentes.
- Mantener compatibilidad con datos ya guardados.

### Resultado esperado
Ya existe una estructura configurable en BD para clasificar inventario.

---

## Fase 2. Reemplazar enum en materias primas
### Backend
1. Agregar a `MateriaPrima`:
   - `TipoInventarioId`
   - navegación a `TipoInventario`
2. Mantener temporalmente `TipoMateriaPrimaEnum` solo durante transición.
3. Crear migración para agregar la FK.
4. Mapear los registros existentes del enum a los tipos creados en catálogo.
5. Cuando la migración esté completa, eliminar el enum del flujo funcional.

### Datos
Mapeo sugerido:
- `Tinta` -> tipo catálogo `Tinta`
- `Base` -> tipo catálogo `Base`
- `Solvente` -> tipo catálogo `Solvente`
- `Aditivo` -> tipo catálogo `Aditivo`
- `Otro` -> tipo catálogo `Otro`

### Resultado esperado
Las materias primas ya no dependen de tipos fijos en código.

---

## Fase 3. Evaluar alineación de insumos
### Backend
Revisar si `Insumo` ya tiene algo similar a tipo configurable.

#### Opción preferida
Agregar también:
- `TipoInventarioId`
- navegación a `TipoInventario`

Con esto ambos modelos quedan alineados conceptualmente sin fusionarlos.

### Resultado esperado
`MateriaPrima` e `Insumo` usan el mismo catálogo de clasificación.

---

## Fase 4. Pantallas de catálogo
### Pantallas nuevas sugeridas
#### 1. `Configuración > Categorías de inventario`
CRUD básico para categorías.

#### 2. `Configuración > Tipos de inventario`
CRUD con:
- filtro por categoría
- alta
- edición
- activación/desactivación
- orden

### UX esperada
- Pantalla simple de administración.
- Select dependiente por categoría.
- Nombres genéricos, no amarrados a serigrafía.

### Resultado esperado
El usuario puede configurar tipos sin tocar código.

---

## Fase 5. Actualizar pantalla de materias primas
### Cambios
En `MateriasPrimas.razor`:
1. Reemplazar `Enum.GetValues<TipoMateriaPrimaEnum>()`.
2. Cargar tipos desde repositorio.
3. Filtrar únicamente tipos cuya categoría sea `Materia Prima`.
4. Guardar `TipoInventarioId` en vez de enum.
5. Mostrar nombre del tipo desde relación.

### Resultado esperado
La pantalla usa catálogo real y no enum.

---

## Fase 6. Actualizar pantalla de insumos
### Cambios
En `Insumos.razor`:
1. Cargar tipos desde repositorio.
2. Filtrar solo tipos de categoría `Insumo`.
3. Guardar `TipoInventarioId`.
4. Mostrar nombre del tipo desde relación.

### Resultado esperado
Ambas pantallas quedan consistentes.

---

## Fase 7. Crear pantalla unificada de inventario
### Nueva pantalla sugerida
`/inventario`

### Objetivo
Mostrar en una sola vista:
- materias primas
- insumos

### Columnas sugeridas
- Tipo de registro (`Materia Prima` / `Insumo`)
- Código
- Nombre
- Categoría
- Tipo
- Unidad
- Existencia
- Stock mínimo
- Precio/Costo
- Estado

### Filtros sugeridos
- Categoría
- Tipo
- texto libre
- stock bajo
- activo/inactivo

### Implementación sugerida
Crear un DTO o view model común, por ejemplo:
- `InventarioItemViewModel`

Con propiedades como:
- `Origen`
- `Id`
- `Codigo`
- `Nombre`
- `CategoriaNombre`
- `TipoNombre`
- `Cantidad`
- `UnidadMedida`
- `StockMinimo`
- `PrecioUnitario`
- `EstaBajoMinimo`

### Resultado esperado
La operación consulta inventario desde un solo lugar sin perder separación interna.

---

## Fase 8. Menú y navegación
### Menú sugerido
#### Manufactura
- Pedidos
- Cotizaciones
- Inventario

#### Configuración
- General
- Tipos de inventario
- Ajustes del módulo

### Ajustes de menú
- Mantener `Materias Primas` e `Insumos` inicialmente dentro de configuración o como accesos secundarios según necesidad operativa.
- `Inventario` debe convertirse en la entrada principal para consulta.

---

## Fase 9. Limpieza técnica
Cuando todo lo anterior ya esté estable:
1. Eliminar `TipoMateriaPrimaEnum` si ya no se usa.
2. Retirar lógica vieja basada en enum.
3. Ajustar badges, filtros y helpers que dependan del enum.
4. Revisar reportes o cálculos que lean el tipo anterior.

---

## Orden recomendado de implementación
1. Crear catálogos nuevos.
2. Migración y seed inicial.
3. Migrar `MateriaPrima` a catálogo.
4. Migrar `Insumo` a catálogo.
5. Crear CRUD de tipos de inventario.
6. Actualizar `MateriasPrimas.razor`.
7. Actualizar `Insumos.razor`.
8. Crear pantalla unificada de `Inventario`.
9. Ajustar `NavMenu`.
10. Limpiar código legado.

---

## Riesgos
### Riesgo 1
Migración de datos incorrecta del enum a catálogo.

Mitigación:
- hacer seed controlado
- mapear por nombre/código fijo
- validar registros migrados

### Riesgo 2
Romper pantallas actuales por dependencia al enum.

Mitigación:
- migración en dos pasos
- mantener compatibilidad temporal

### Riesgo 3
Unificar inventario demasiado pronto.

Mitigación:
- unificar vista primero, no la tabla física

---

## Criterios de aceptación
### Catálogos
- Se pueden crear y editar tipos de inventario.
- Los tipos pueden asociarse a categoría.

### Materias primas
- Ya no dependen de enum fijo.
- Se pueden asignar tipos configurables.

### Insumos
- También se clasifican con catálogo común.

### Inventario
- Existe una vista unificada.
- Se puede filtrar por categoría y tipo.

### Menú
- Usa terminología genérica para varias industrias.

---

## Fase recomendada para empezar ahora
### Paso 1 inmediato
Implementar solo esto primero:
- entidades `CategoriaInventario` y `TipoInventario`
- migración EF
- seed inicial
- pantalla CRUD de `Tipos de inventario`

Ese paso desbloquea todo lo demás sin romper la operación actual.

---

## Siguiente paso después de este plan
Cuando se decida ejecutar:
1. empezar por las entidades y configuración EF
2. luego migración
3. luego pantalla de catálogo
4. luego reemplazo del enum
