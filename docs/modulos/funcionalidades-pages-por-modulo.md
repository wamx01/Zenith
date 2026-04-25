# Documentación funcional por módulo

## Objetivo
Este documento resume las funcionalidades implementadas en `Components/Pages`, agrupadas por módulo según la organización actual de carpetas y páginas del proyecto.

## Estructura general detectada
- `Auth`
- `Core`
- `Admin`
- `SuperAdmin`
- `Ventas`
- `RRHH`
- `Compras`
- `Contabilidad`
- `Produccion`
  - `Serigrafia`

---

## 1. Módulo `Auth`
Función general: autenticación, recuperación de acceso y primer cambio de contraseña.

### Páginas
- `Auth/Login.razor` — ruta `/login`
  - Permite iniciar sesión.
  - Usa proveedor de autenticación personalizado.
  - Presenta branding y acceso al flujo de recuperación de contraseña.
  - Redirige según el estado del usuario, incluyendo cambio de contraseña inicial cuando aplica.

- `Auth/RecuperarPassword.razor` — ruta `/auth/recuperar-password`
  - Solicita recuperación de contraseña por correo.
  - Genera token temporal de recuperación.
  - En ambiente de desarrollo muestra el token generado para pruebas locales.

- `Auth/RestablecerPassword.razor` — ruta `/auth/restablecer-password`
  - Restablece la contraseña usando correo, token temporal y nueva contraseña.
  - Valida coincidencia de contraseñas y reglas mínimas de seguridad.
  - Redirige al login al finalizar correctamente.

- `Auth/CambiarPasswordInicial.razor` — ruta `/auth/cambiar-password-inicial`
  - Obliga al usuario autenticado a definir una contraseña nueva en el primer acceso.
  - Valida la contraseña y actualiza el estado inicial del usuario.
  - Redirige al inicio una vez concluido el proceso.

### Observaciones
- `Components/Pages/Login.razor` está vacío actualmente.

---

## 2. Módulo `Core`
Función general: páginas base del sistema, inicio y manejo de errores.

### Páginas
- `Core/Home.razor` — ruta `/`
  - Funciona como dashboard principal.
  - Muestra vista distinta para `SuperAdmin` y para usuarios operativos.
  - Resume métricas de negocio y accesos rápidos según capacidades del usuario.
  - Expone accesos directos a clientes, productos, pedidos, cotizaciones, dashboard contable, configuración y usuarios.

- `Core/Error.razor` — ruta `/Error`
  - Presenta pantalla de error de aplicación.
  - Muestra `RequestId` para diagnóstico.

- `Core/NotFound.razor` — ruta `/not-found`
  - Pantalla simple para contenido inexistente.

- `Core/Counter.razor` — ruta `/counter`
  - Página de ejemplo base de Blazor.
  - No representa una funcionalidad de negocio.

- `Core/Weather.razor` — ruta `/weather`
  - Página de ejemplo de carga de datos.
  - No representa una funcionalidad de negocio del sistema.

---

## 3. Módulo `Admin`
Función general: configuración operativa, catálogos administrativos y seguridad interna por empresa.

### Páginas
- `Admin/Configuracion.razor` — ruta `/configuracion`
  - Centraliza la configuración del sistema.
  - Permite activar o desactivar módulos disponibles.
  - Da acceso a catálogos generales y configuraciones especializadas.
  - Expone accesos a tipos de inventario, nómina y catálogos del módulo productivo.

- `Admin/ConfiguracionNomina.razor` — ruta `/configuracion/nomina`
  - Configura días base, horas base y factor de horas extra.
  - Separa configuración semanal, quincenal y mensual.
  - Guarda parámetros que después usa el cálculo de nómina.

- `Admin/TiposInventario.razor` — ruta `/configuracion/tipos-inventario`
  - Administra tipos de inventario ligados a categorías.
  - Permite alta, edición, activación e inactivación.
  - Incluye filtros por categoría, texto y estado.

- `Admin/TiposUsuario.razor` — ruta `/admin/tipos-usuario`
  - Administra roles o tipos de usuario por empresa.
  - Permite crear y editar tipos de usuario.
  - Asigna capacidades o permisos por módulo.
  - Oculta capacidades globales de empresas cuando el usuario no es `SuperAdmin`.

- `Admin/Usuarios.razor` — ruta `/admin/usuarios`
  - Administra usuarios del tenant actual.
  - Permite alta y edición de usuarios.
  - Asigna tipo de usuario, estado y contraseña.
  - Valida límites de usuarios del plan o empresa activa.

---

## 4. Módulo `SuperAdmin`
Función general: administración global de tenants, permisos globales y suscripciones.

### Páginas
- `SuperAdmin/Empresas.razor` — ruta `/superadmin/empresas`
  - Administra empresas registradas en la plataforma.
  - Permite crear y editar tenants.
  - Configura razón social, nombre comercial, RFC, estado, trial, plan y límite de usuarios.
  - Gestiona suscripción actual y sus pagos.
  - Permite cargar logo por empresa.
  - Puede crear usuario administrador inicial al alta.

- `SuperAdmin/Capacidades.razor` — ruta `/superadmin/capacidades`
  - Administra el catálogo global de capacidades/permisos.
  - Permite alta, edición y eliminación de capacidades.
  - Organiza permisos por módulo, clave y descripción.

### Observaciones
- `Components/Pages/Empresas.razor` está vacío actualmente.

---

## 5. Módulo `Ventas`
Función general: gestión comercial de clientes, productos, relaciones cliente-producto, pedidos y cotizaciones.

### Páginas
- `Ventas/Clientes.razor` — ruta `/clientes`
  - Catálogo y gestión de clientes.
  - Permite alta, edición e inactivación lógica.
  - Captura datos fiscales, comerciales, de contacto y clasificación por industria.

- `Ventas/Productos.razor` — ruta `/productos`
  - Catálogo general de productos.
  - Permite alta, edición e inactivación lógica.
  - Captura referencia, categoría, unidad de medida, industria y precio base.
  - Permite definir si el producto usa variantes.
  - Genera SKU por talla, color o ambas dimensiones según la configuración del producto.
  - Incluye filtro por industria.

- `Ventas/ProductosCliente.razor` — ruta `/clientes/productos`
  - Asigna productos específicos a cada cliente.
  - Consulta productos permitidos por cliente.
  - Permite agregar o retirar asignaciones.
  - Mantiene la lista del cliente vacía cuando no tiene productos asignados.
  - La asignación sigue siendo por producto base; las tallas y fracciones del cliente se administran en la pantalla de clientes.

- `Ventas/Pedidos.razor` — ruta `/pedidos`
  - Administra pedidos generales.
  - Permite alta, edición, filtrado por estado y fechas, e inactivación lógica.
  - Controla datos de cliente, fechas, importes, observaciones y estado del pedido.
  - Si el producto usa variación, permite elegir la variación fija del pedido antes de resolver las tallas.
  - Convierte fracciones por talla a SKU reales usando `ProductoVariante` y las tallas permitidas del cliente.
  - Da acceso a seguimiento por pedido.

- `Ventas/Presupuestos.razor` — ruta `/serigrafia/cotizaciones`
  - Gestiona cotizaciones orientadas al flujo productivo.
  - Permite crear cotización por producto.
  - Selecciona procesos a usar y genera mano de obra automáticamente.
  - Calcula costos por tintas, insumos básicos, insumos diversos, mano de obra y gastos fijos.
  - Resume costo base, costo unitario, utilidad, precio contado, precio crédito y costo por tarea.

---

## 6. Módulo `RRHH`
Función general: administración de empleados, esquemas de pago, vales de destajo y nómina.

### Páginas
- `RRHH/Empleados.razor` — ruta `/rrhh/empleados`
  - Administra empleados.
  - Captura datos personales, laborales y salariales.
  - Asigna posición, periodicidad de pago y esquema de pago vigente.
  - Muestra historial de esquemas asignados por empleado.

- `RRHH/EsquemasPago.razor` — ruta `/rrhh/esquemas-pago`
  - Define esquemas de pago.
  - Soporta sueldo fijo, destajo por pieza, destajo por operación, bono por meta y esquemas mixtos.
  - Permite configurar sueldo base sugerido, metas y reglas de reparto.
  - Permite registrar tarifas por proceso y/o posición.

- `RRHH/ValesDestajo.razor` — ruta `/rrhh/vales-destajo`
  - Registra vales de destajo por empleado.
  - Filtra por empleado, fechas y estatus.
  - Captura detalle de producción por proceso y pedido.
  - Calcula tarifa aplicada e importe por línea.
  - Maneja flujo de borrador, aprobación, envío a nómina, pago o cancelación.

- `RRHH/Nominas.razor` — ruta `/rrhh/nominas`
  - Administra periodos de nómina.
  - Permite crear y editar nóminas.
  - Calcula detalle por empleado con sueldo base, destajo, bonos, horas extra y deducciones.
  - Integra vales de destajo aprobados dentro del cálculo.
  - Permite recalcular nómina y guardar ajustes por empleado.

---

## 7. Módulo `Compras`
Función general: administración de proveedores y obligaciones por pagar.

### Páginas
- `Compras/Proveedores.razor` — ruta `/proveedores`
  - Catálogo de proveedores.
  - Permite alta, edición y activación/inactivación.
  - Captura datos fiscales, comerciales y de contacto.

- `Compras/CuentasPorPagar.razor` — ruta `/contabilidad/cuentas-por-pagar`
  - Administra cuentas por pagar a proveedores.
  - Permite registrar documentos por pagar con fechas, importes y estatus.
  - Registra pagos parciales o totales.
  - Calcula saldo pendiente por documento.

---

## 8. Módulo `Contabilidad`
Función general: indicadores financieros, cobranza y estado de resultados.

### Páginas
- `Contabilidad/Dashboard.razor` — ruta `/dashboard`
  - Dashboard con indicadores del negocio.
  - Muestra pedidos, ingresos, piezas, pendientes y tendencias.
  - Presenta distribución por estado, ingresos por mes y ranking de clientes.

- `Contabilidad/CuentasPorCobrar.razor` — ruta `/contabilidad/cuentas-por-cobrar`
  - Controla facturas y pagos recibidos.
  - Agrupa cuentas por cliente y factura.
  - Muestra montos facturados, cobrados y saldo pendiente.
  - Permite registrar y eliminar pagos desde cada factura.

- `Contabilidad/EstadoResultados.razor` — ruta `/contabilidad/estado-resultados`
  - Presenta estado de resultados financiero.
  - Resume ingresos, descuentos, costo de ventas, gastos operativos, impuestos y utilidad.
  - Usa estructura tipo NIF B-3 con indicadores de utilidad bruta, operación y utilidad neta.

---

## 9. Módulo `Produccion`
Función general: inventario consolidado y operaciones productivas.

### Páginas
- `Produccion/Inventario.razor` — ruta `/inventario`
  - Consolida materias primas e insumos en una sola vista.
  - Permite altas rápidas de materia prima e insumo.
  - Permite registrar movimientos manuales.
  - Incluye filtros por origen, tipo, texto, stock bajo y sin existencia.
  - Muestra métricas de registros, costo, stock bajo y movimientos.

---

## 10. Submódulo `Produccion/Serigrafia`
Función general: configuración productiva, catálogos de operación, pedidos productivos y seguimiento por proceso.

### Páginas
- `Produccion/Serigrafia/Actividades.razor` — rutas `/serigrafia/actividades`, `/serigrafia/posiciones`
  - Administra posiciones de trabajo.
  - Configura nombre, orden y tarifa por minuto.
  - Sirve como base para procesos, cotizaciones y destajo.

- `Produccion/Serigrafia/TiposProceso.razor` — ruta `/serigrafia/tipos-proceso`
  - Administra tipos de proceso productivo.
  - Asocia cada proceso a una posición.
  - Hereda o muestra tarifa por minuto desde la posición.
  - Permite ordenar y activar/inactivar procesos.

- `Produccion/Serigrafia/MateriasPrimas.razor` — ruta `/serigrafia/materias-primas`
  - Catálogo de materias primas.
  - Controla tipo de inventario, precio, unidad, existencia y stock mínimo.
  - Incluye atributos de color como Pantone y código hex.
  - Señala visualmente stock bajo.

- `Produccion/Serigrafia/Insumos.razor` — ruta `/serigrafia/insumos`
  - Catálogo de insumos.
  - Controla tipo de inventario, precio, unidad, existencia y stock mínimo.
  - Permite filtrado por tipo y detección de stock bajo.

- `Produccion/Serigrafia/Pantallas.razor` — ruta `/serigrafia/pantallas`
  - Administra pantallas de producción.
  - Registra código, malla, dimensiones, estado, usos y diseño asignado.
  - Permite seguimiento básico del ciclo de vida de cada pantalla.

- `Produccion/Serigrafia/GastosFijos.razor` — ruta `/serigrafia/gastos-fijos`
  - Catálogo de gastos fijos productivos.
  - Registra concepto, costo mensual y orden.
  - Se usa como insumo para cotizaciones y análisis financiero.

- `Produccion/Serigrafia/PedidosSerigrafia.razor` — ruta `/serigrafia/pedidos`
  - Trabaja sobre pedidos productivos que nacen de `Pedido` y `PedidoDetalle`.
  - Conserva estilo y combinación de color del trabajo productivo.

- `Produccion/Serigrafia/PedidoSeguimiento.razor` — ruta `/pedidos/{PedidoId:guid}/seguimiento`
  - Da seguimiento por talla y operación al pedido productivo.
  - Permite registrar avance, fecha, reparto de destajo y entregas parciales por talla.
  - Gestiona pedidos del flujo productivo principal.
  - Permite capturar pedidos por productos, servicios o ambos.
  - Selecciona cliente, tipo de precio y modo de captura.
  - Usa productos asignados al cliente y cotizaciones base por producto.
  - Captura estilos, combinaciones de color, procesos, tallas, servicios, entrega, factura y pagos.
  - Incluye seguimiento por talla y proceso dentro del pedido.

- `Produccion/Serigrafia/PedidoSeguimiento.razor` — ruta `/pedidos/{PedidoId}/seguimiento`
  - Realiza seguimiento operativo detallado del pedido.
  - Muestra avance por talla y proceso.
  - Permite marcar procesos completados y registrar fecha de paso.
  - Distribuye destajo por empleado dentro de cada proceso.
  - Administra conceptos de servicio completados.
  - Permite guardar datos de entrega real y factura.

---

## Hallazgos relevantes
- La organización funcional principal sí está definida por carpetas dentro de `Components/Pages`.
- El sistema combina módulos de negocio administrativos, comerciales, productivos y financieros.
- El flujo productivo más completo hoy está concentrado en `Produccion/Serigrafia` y se conecta con:
  - cotizaciones,
  - pedidos,
  - seguimiento,
  - vales de destajo,
  - nómina,
  - cuentas por cobrar,
  - estado de resultados.
- Existen páginas base de plantilla (`Counter`, `Weather`) y páginas vacías (`Empresas.razor`, `Login.razor`) que no aportan lógica funcional actual.

## Recomendación de uso de este documento
Este archivo puede servir como base para:
- documentación funcional para usuarios,
- mapa de navegación del sistema,
- definición de backlog por módulo,
- análisis de cobertura funcional antes de nuevas implementaciones.