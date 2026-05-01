# Copilot Instructions

## Project Guidelines
- Al implementar o modificar algo específico del sistema, consultar `docs/conocimiento/` como fuente funcional y operativa del repositorio.
- Cuando un cliente no tiene productos asignados, no se deben cargar todos los productos; la lista debe quedar vacía.
- El menú y la terminología de la app deben ser genéricos; evitar mostrar 'serigrafía', 'calzado' o 'pedidos de serigrafía' cuando el flujo es el mismo y debe mostrarse simplemente como 'Pedidos'.
- Usar `Zenith` como nombre de la app y al crear o publicar el repositorio del proyecto en GitHub; `Arzmec` es el desarrollador y su mención debe ser discreta y profesional en el branding. No cargar branding específico de la empresa desde la configuración; mantener el branding fijo como `Zenith` para evitar confusiones entre empresas.
- La cadena de conexión no debe llamarse `DefaultConnection`; debe llamarse `ZenithConnection`.
- Toda tabla nueva debe nombrarse con el patrón `modulo_funcion`.
- Usar como marco normativo del ERP México: NIF/CINIF, CFDI 4.0, CFF art. 28-29-A, complemento nómina 1.2, IMSS/LSS/SUA, LISR, LIVA, Código de Comercio y LFT; las propuestas de contabilidad, compras, ventas, nómina e impuestos deben evaluarse contra ese marco.
- Toda característica que no se indique explícitamente como configuración maestra debe modelarse y aplicarse por cliente; esta es una regla de negocio base del repositorio.
- Los archivos adjuntos deben tener una configuración maestra de almacenamiento y además una configuración por empresa para decidir dónde guardar; la estructura debe poder organizarse por empresa, cliente, nota y entidades similares.
- La interfaz de usuario (UI) debe ser compacta en desktop: utilizar tipografías, menú lateral y espaciados menos grandes para que se sienta como PC y no como tablet.
- La pantalla de procesar pedido debe ser más intuitiva, menos estorbosa con múltiples tallas y con mejor UI/UX en general. El usuario prefiere mantener la vista compacta anterior con colores de pendiente/terminado, reducir el espacio del precio y fecha, usar un botón con modal para agregar empleados y mostrar solo un conteo/resumen en la grilla principal.
- Al crear un pedido, el usuario debe elegir talla y variación/color para que el sistema despliegue los SKU relacionados; si aplica fracción, la fracción debe operar sobre la variación elegida y resolver los SKU de esa variación.
- **Preferencia funcional de UI en pedidos**: el flujo debe verse más lógico; `Detalles del pedido` debe incluir primero selección de variación, tallas disponibles y corrida, y `Tipos de proceso` debe quedar al final.
- No debe agregarse una sola talla manualmente; el usuario debe poder agregar varios SKU o las tallas de una corrida ya creada, y si existe variación de color primero se debe elegir la variación para generar la corrida del pedido.
- En manufactura, el usuario quiere separar estado productivo de entrega: mostrar observación de entrega pendiente aun cuando el pedido ya esté producido, considerar ingreso a inventario de finished goods cuando todos los procesos de un SKU estén completos, y bloquear edición de checks de procesos en pedidos producidos/entregados salvo capability especial.
- En seguimiento de producción, cuando un SKU quede completo y esté listo para finished goods, el usuario prefiere lanzar el ingreso a FG con confirmación en lugar de hacerlo automático sin preguntar.
- La configuración de tallas y fracciones por cliente no debe limitarse a empresas o clientes de industria Calzado/Ambas; debe estar disponible para cualquier tipo de empresa.
- En este flujo, la validación previa de consumos e inventario debe implementarse en `MundoVs/Components/Pages/Ventas/Pedidos.razor`; la cotización solo sirve como fuente de consumos, no como lugar del cambio UI.
- Para esta arquitectura, el agente interno debe vivir en `ZkTecoApi` porque se desplegará en el servidor interno del cliente; no debe asumirse `Zenith.Workers.Asistencia` como agente final.
- La documentación operativa se debe guardar en `docs/manual/`, organizada por secuencia o módulos, con un índice navegable.

## Order Management Guidelines
- Al agregar un pedido, primero se debe definir si se capturarán productos o servicios; los servicios también deben manejar descripción y precio.
- En el dominio del proyecto, 'fracción' en calzado es la distribución de pares por talla dentro de una docena o surtido. Variantes comunes: docena corrida, media docena, docena y media y surtido especial. Puede modelarse como una plantilla reutilizable para convertir docenas por modelo/color en cantidades por talla dentro de pedidos, inventario y producción.
- En cotización de calzado se debe poder definir una talla base; las fracciones pueden ser plantillas reutilizables con pares/unidades por fracción y porcentaje de variación por talla. Al crear el pedido se debe indicar si aplica fracción y cuál, y la disponibilidad de tallas debe poder administrarse por cliente salvo que se marque explícitamente como configuración maestra.
- Un pedido puede entregarse en parcialidades; cada parcialidad debe manejarse como una nota y puede generar su propia factura, por lo que la lógica no debe asumir facturación por pedido completo.
- **Regla funcional**: no todas las notas de entrega se facturan, por lo que cuentas por cobrar y cobranza deben soportar notas de entrega como documento cobrable independiente.

## Inventory Module Guidelines
- En el módulo de inventario, los usuarios prefieren acciones directas por cada fila para registrar entrada o salida del item, en lugar de un flujo separado más genérico.
- La columna de acciones debe aparecer al inicio, del lado izquierdo de las tablas, no al final.
- `Tipos de Inventario` debe estar en `Logística` y la página `/configuracion` debe ser compacta, mostrando solo catálogos generales, usuarios, roles y permisos, ocultando opciones de módulos no habilitados para la empresa.

## Production Cost Calculation Guidelines
- Para calcular destajo en producción, relacionar el costo/actividad de mano de obra con el proceso para determinar pago por minuto o tarifa de la operación. La tarifa no debe salir del esquema fijo por empleado; debe derivarse de la cotización del pedido: tiempo estándar/proceso y pago por minuto u operación. Si el empleado está en otra operación, calcular igualmente la tarifa aplicable y solo avisar que no es su operación natural.

## Human Resources Guidelines
- Los permisos parciales con goce deben descontarse del banco de horas del empleado.
- Las capturas fijas de IMSS/ISR deben manejarse por empleado, no como configuración general.

## Deployment Guidelines
- Para tareas de publicación/despliegue, responder solo con los comandos exactos sin explicación adicional.
- Cuando el usuario pida comandos para Docker o despliegue, dar una instrucción por línea y no agrupar múltiples comandos en una sola línea.

## User Preferences
- El usuario prefiere optimizaciones robustas y bien hechas sobre mejoras rápidas o parciales, especialmente en el flujo de corrección de checador.