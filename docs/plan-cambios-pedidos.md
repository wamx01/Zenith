# Plan de cambios: pedidos, conceptos no productivos y trazabilidad por empleado

## Objetivo
Implementar una primera mejora al flujo de pedidos para cubrir dos necesidades:

1. Permitir capturar conceptos que no son productivos, por ejemplo `diseño`, `entrega`, `muestra`, `empaque especial` o cargos similares.
2. Registrar qué empleado realizó cada avance de proceso dentro del seguimiento.

---

## Hallazgos del sistema actual

### Flujo de pedido
- La pantalla principal relacionada es `Components/Pages/Produccion/Serigrafia/PedidosSerigrafia.razor`.
- El pedido productivo hoy se apoya en:
  - `Pedido`
  - `PedidoDetalle`
  - `PedidoSerigrafia`
  - `PedidoSerigrafiaTalla`
  - `PedidoSerigrafiaProcesoDetalle`
  - `PedidoSerigrafiaTallaProceso`
- El total del pedido se calcula a partir de un producto principal y sus tallas.

### Seguimiento de proceso
- La pantalla de avance es `Components/Pages/Produccion/Serigrafia/PedidoSeguimiento.razor`.
- Actualmente cada celda de talla/proceso guarda:
  - si está completado
  - fecha del paso
- No existe un campo para saber qué empleado hizo el trabajo.

### Catálogo de empleados
- Ya existe la entidad `Empleado` y una pantalla de administración en `Components/Pages/RRHH/Empleados.razor`.
- Esto permite reutilizar el catálogo actual sin crear uno nuevo.

---

## Propuesta funcional

## Cambio 1: conceptos no productivos dentro del pedido

### Qué se busca
Que al crear un pedido primero se pueda definir si se capturarán `productos`, `servicios` o ambos, y que los servicios no entren a la matriz de producción pero sí formen parte del cobro.

### Ejemplos
- Diseño
- Entrega
- Muestra
- Ajuste
- Empaque especial
- Servicio adicional

### Propuesta de comportamiento
- Al iniciar la captura del pedido, definir qué se va a agregar:
  - productos
  - servicios
  - o ambos
- Mantener el producto principal de producción como está hoy para los casos productivos.
- Agregar una sección de `servicios` o `conceptos adicionales` en el formulario del pedido.
- Cada servicio o concepto tendrá al menos:
  - tipo
  - descripción
  - cantidad
  - precio unitario
  - total
- Estos conceptos:
  - sí suman al subtotal, impuestos y total
  - no participan en tallas ni procesos
  - no aparecen como parte del seguimiento productivo

### Decisión de modelado propuesta
Crear una nueva entidad para conceptos del pedido, en lugar de forzar este caso dentro de `PedidoDetalle`.

#### Motivo
`PedidoDetalle` hoy está muy amarrado al producto. Separar los conceptos evita mezclar lógica productiva con servicios o cargos administrativos.

### Entidad propuesta
`PedidoConcepto`

Campos propuestos:
- `Id`
- `PedidoId`
- `Tipo`
- `Descripcion`
- `Cantidad`
- `PrecioUnitario`
- `Total`
- campos base de auditoría ya usados en el proyecto

### Catálogo o enum sugerido
Enum sugerido: `PedidoConceptoTipo`
- `Servicio`
- `Entrega`
- `Diseno`
- `Empaque`
- `Otro`

> El texto visible en UI puede mantenerse genérico.

---

## Cambio 2: saber qué empleado hizo cada proceso

### Qué se busca
Que cada avance de talla/proceso permita identificar al empleado responsable.

### Propuesta de comportamiento
En cada registro de `PedidoSerigrafiaTallaProceso` agregar:
- `EmpleadoId` opcional
- navegación a `Empleado`

Cuando se marque un proceso como completado:
- se podrá seleccionar el empleado responsable
- se conservará la fecha del paso
- en la vista de seguimiento se mostrará quién lo realizó

### Alcance recomendado
Guardar dos tipos de trazabilidad:
1. `EmpleadoId`: quién realizó físicamente el proceso
2. `UpdatedBy`: quién registró el cambio en sistema

### Resultado esperado
En seguimiento se podrá ver algo similar a:
- `Hecho por: Juan Pérez`
- `Fecha: 15/03/2026`

---

## Propuesta de UX

## Formulario de pedido
En `PedidosSerigrafia.razor`:
- agregar una selección inicial del tipo de captura del pedido:
  - productos
  - servicios
  - ambos
- agregar una nueva sección `Conceptos adicionales`
- permitir agregar y quitar renglones
- recalcular subtotal/impuestos/total considerando:
  - producto principal
  - conceptos adicionales

### Estructura visual sugerida
Columnas:
- tipo
- descripción
- cantidad
- precio unitario
- total
- acción para eliminar

### Comportamiento esperado de captura
- Si el usuario va a registrar productos, se muestra el flujo actual de producto y producción.
- Si el usuario va a registrar servicios, se habilita la captura directa de renglones con descripción y precio.
- Si el pedido combina ambos, deben convivir ambas secciones dentro del mismo pedido.

---

## Pantalla de seguimiento
En `PedidoSeguimiento.razor`:
- mantener la matriz actual de talla/proceso
- agregar selección de empleado por celda o mostrarla cuando la celda ya tenga avance

### Opción recomendada para primera fase
- Dropdown de empleado en la celda cuando exista seguimiento
- Si se marca como completado y no tiene empleado, permitir seleccionarlo
- Mostrar nombre corto debajo de la fecha

### Nota
Esto es más exacto que asignar un empleado por proceso completo, porque permite saber quién hizo cada talla/proceso.

---

## Diseño técnico propuesto

## Nuevas entidades / cambios de modelo

### 1. Nueva entidad
- `Core/Entities/PedidoConcepto.cs`

### 2. Cambios en entidades existentes
- `Core/Entities/Pedido.cs`
  - agregar colección `Conceptos`
- `Core/Entities/Serigrafia/PedidoSerigrafiaTallaProceso.cs`
  - agregar `EmpleadoId`
  - agregar navegación `Empleado`

### 3. Cambios en DbContext
- `Infrastructure/Data/CrmDbContext.cs`
  - nuevo `DbSet<PedidoConcepto>`
  - configuración de la nueva entidad
  - configuración de la FK hacia `Empleado` en seguimiento de procesos

### 4. Migración EF Core
Crear una migración para:
- tabla `PedidoConceptos`
- columna `EmpleadoId` en `PedidoSerigrafiaTallaProcesos`
- índices y relaciones correspondientes

---

## Cambios de UI previstos

### Archivos a tocar
- `Components/Pages/Produccion/Serigrafia/PedidosSerigrafia.razor`
- `Components/Pages/Produccion/Serigrafia/PedidoSeguimiento.razor`

### Posibles archivos de apoyo
- `Core/Entities/Pedido.cs`
- `Core/Entities/Serigrafia/PedidoSerigrafiaTallaProceso.cs`
- `Infrastructure/Data/CrmDbContext.cs`
- migración nueva en `Migrations/`

---

## Reglas de cálculo

## Total del pedido
El total se recalculará como:
- subtotal producto principal
- más subtotal de conceptos adicionales
- más impuestos

## Seguimiento
El estado del pedido seguirá dependiendo del avance actual, pero ahora cada avance podrá incluir responsable.

---

## Riesgos y consideraciones
- El filtro de productos por cliente no debe alterarse: si el cliente no tiene productos asignados, la lista debe permanecer vacía.
- La terminología en UI debe mantenerse lo más genérica posible.
- El cambio debe ser mínimo y compatible con el flujo actual.
- Para la migración se deben seguir las reglas del repo para EF Core.

---

## Fases de implementación sugeridas

## Fase 1
- Crear `PedidoConcepto`
- Agregar `EmpleadoId` a `PedidoSerigrafiaTallaProceso`
- Actualizar `DbContext`
- Crear migración

## Fase 2
- Agregar UI de conceptos en pedido
- Ajustar cálculo de importes
- Agregar UI de empleado en seguimiento

## Fase 3
- Validación manual
- Revisión de compilación
- Prueba de flujo completo:
  - crear pedido
  - agregar concepto no productivo
  - marcar proceso con empleado
  - validar total y trazabilidad

---

## Criterio de aceptación
- Al crear un pedido se puede definir si llevará productos, servicios o ambos.
- Se puede crear un pedido con uno o más conceptos no productivos.
- Los conceptos adicionales impactan los importes del pedido.
- Se puede registrar un empleado por avance de proceso.
- En seguimiento se puede consultar quién realizó el avance.
- El flujo actual de producto, tallas y procesos sigue funcionando.

---

## Estado
Pendiente de confirmación para aplicar cambios en código.
