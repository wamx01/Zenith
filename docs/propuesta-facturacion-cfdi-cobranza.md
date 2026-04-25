# Zenith ERP — Propuesta de Facturación CFDI 4.0 y Cobranza Integrada

> **Versión:** 1.0  
> **Fecha:** Abril 2026  
> **Proyecto:** Zenith ERP  
> **Enfoque:** Integración con API externa de facturación, por ejemplo `Facturapi`, sin acoplar el dominio interno a un proveedor único

---

## 1. Objetivo

Incorporar en `Zenith` un flujo de facturación y cobranza que permita:

- emitir CFDI 4.0 desde pedidos y cuentas por cobrar,
- operar con una API externa de timbrado como `Facturapi`,
- conservar en `Zenith` el control del negocio y la trazabilidad documental,
- registrar pagos parciales o totales de forma compatible con complemento de pago,
- mantener catálogos y validaciones alineados con el marco normativo mexicano aplicable.

La recomendación es que `Zenith` sea el sistema maestro del proceso comercial y contable operativo, mientras que el proveedor externo sea la capa de timbrado, cancelación, consulta fiscal y entrega de XML/PDF.

---

## 2. Marco normativo que debe respetar la solución

La propuesta se alinea con el marco indicado para el proyecto:

- `CFDI 4.0`
- `CFF art. 28-29-A`
- `NIF/CINIF`
- `LISR`
- `LIVA`
- `Código de Comercio`

En términos prácticos, esto implica que el sistema debe controlar al menos:

- datos fiscales correctos del emisor y receptor,
- uso de catálogos SAT vigentes,
- método de pago y forma de pago conforme al tipo de operación,
- manejo correcto de `PUE` vs `PPD`,
- emisión de complemento de pago cuando la cobranza ocurra después de la factura,
- conservación de UUID, XML, PDF, acuses y trazabilidad de cancelación.

---

## 3. Revisión funcional de una API tipo Facturapi

La referencia pública de `Facturapi` muestra un modelo de integración orientado a:

- `customer`
- `items`
- `payment_form`
- emisión de factura
- envío por correo
- operación multi-RFC

Esto confirma que conviene que `Zenith` no dependa de pantallas manuales del proveedor, sino que construya un **modelo canónico interno** y lo transforme al formato del PAC/API elegido.

### Conclusión técnica

`Zenith` debe regresar al conector de facturación un objeto de negocio completo y neutral, para después mapearlo a `Facturapi` u otro proveedor.

Es decir:

- el dominio interno decide qué se factura,
- el adaptador traduce al contrato del proveedor,
- la respuesta fiscal del proveedor vuelve a `Zenith` para quedar persistida.

---

## 4. Qué debe regresar Zenith para empatar con un modelo tipo Facturapi

La recomendación es que `Zenith` produzca un objeto interno de salida como `FacturaFiscalRequest` con esta estructura funcional.

### 4.1 Encabezado del documento

| Campo sugerido en Zenith | Propósito | Equivalente esperado en proveedor |
|---|---|---|
| `EmpresaId` | determinar emisor | organización / cuenta emisora |
| `TipoComprobante` | ingreso, egreso, pago | `type` o endpoint específico |
| `Serie` | serie interna/fiscal | `series` o dato equivalente |
| `FolioInterno` | trazabilidad en ERP | referencia interna / metadata |
| `FechaEmision` | fecha del CFDI | `date` |
| `Moneda` | MXN, USD, etc. | `currency` |
| `TipoCambio` | si aplica | `exchange` |
| `LugarExpedicionCp` | código postal emisor | `expedition` / address zip |
| `MetodoPagoSat` | `PUE` o `PPD` | `payment_method` |
| `FormaPagoSat` | catálogo SAT | `payment_form` |
| `Exportacion` | clave SAT | `export` |
| `CondicionesPago` | texto comercial | campo libre o metadata |
| `UsoCfdi` | clave SAT del receptor | `use` / dato fiscal del receptor |
| `Observaciones` | referencia operativa | metadata |

### 4.2 Datos fiscales del receptor

| Campo sugerido en Zenith | Propósito |
|---|---|
| `ClienteId` | relación comercial interna |
| `RazonSocial` | nombre fiscal del receptor |
| `Rfc` | RFC receptor |
| `RegimenFiscalReceptor` | catálogo SAT |
| `DomicilioFiscalCp` | código postal fiscal |
| `EmailFactura` | envío de CFDI |
| `UsoCfdi` | clave SAT vigente |
| `ResidenciaFiscal` | si aplica operaciones especiales |
| `NumRegIdTrib` | si aplica |

### 4.3 Conceptos o partidas

Cada renglón debe regresar al menos:

| Campo sugerido en Zenith | Propósito |
|---|---|
| `ReferenciaOrigen` | liga al pedido, servicio o cuenta de cobro |
| `Cantidad` | cantidad facturable |
| `ClaveUnidadSat` | catálogo SAT |
| `Unidad` | etiqueta visible |
| `ClaveProductoServicioSat` | catálogo SAT |
| `Descripcion` | descripción fiscal/comercial |
| `ValorUnitario` | precio unitario antes de impuestos |
| `Descuento` | descuento por concepto |
| `ObjetoImpuesto` | clave SAT |
| `ImpuestosTrasladados` | IVA/IEPS por línea |
| `ImpuestosRetenidos` | si aplica |
| `Importe` | subtotal de la línea |

### 4.4 Impuestos y totales

`Zenith` debe regresar valores calculados y auditables:

- `Subtotal`
- `DescuentoTotal`
- `TotalImpuestosTrasladados`
- `TotalImpuestosRetenidos`
- `Total`
- desglose por tasa y tipo de impuesto

### 4.5 Relación documental

Para no perder trazabilidad, el request debe poder incluir:

- `PedidoId`
- `PedidoFolio`
- `CotizacionId`
- `CuentaPorCobrarId`
- `FacturaRelacionadaUuid[]`
- `TipoRelacionCfdi`
- `PagoIds[]` en caso de complemento de pago

### 4.6 Metadata recomendada

Aunque no sea fiscal, ayuda mucho para soporte e integración:

- `TenantId` o `EmpresaId`
- `UsuarioGenero`
- `CanalOrigen` (`Pedidos`, `CuentasPorCobrar`, `Mostrador`, etc.)
- `NotasInternas`
- `Tags`
- `JsonSnapshotComercial`

---

## 5. Qué debe devolver el proveedor a Zenith

Después del timbrado o actualización fiscal, `Zenith` debe persistir como mínimo:

| Campo de respuesta | Uso en Zenith |
|---|---|
| `ExternalDocumentId` | identificador del proveedor |
| `UuidFiscal` | UUID oficial del CFDI |
| `EstatusFiscal` | borrador, timbrado, cancelado, error |
| `FechaTimbrado` | trazabilidad legal |
| `SerieFiscal` | visualización y búsqueda |
| `FolioFiscal` | visualización y búsqueda |
| `XmlUrl` o archivo | resguardo fiscal |
| `PdfUrl` o archivo | entrega al cliente |
| `CodigoQr` o representación | visualización |
| `SelloSat` / `SelloCfd` / cadena | auditoría si se decide guardar |
| `ErrorCode` | soporte operativo |
| `ErrorMessage` | soporte operativo |
| `Cancelable` | control de cancelación |
| `CancellationStatus` | seguimiento |
| `AcuseCancelacionUrl` | evidencia documental |

### Estados internos sugeridos

`Zenith` puede manejar estos estados internos sin depender del proveedor:

- `Borrador`
- `PendienteTimbrado`
- `Timbrado`
- `ErrorTimbrado`
- `PendienteCancelacion`
- `Cancelado`
- `PagadoParcial`
- `PagadoTotal`

---

## 6. Métodos de pago y cobro conformes a la normativa

Aquí conviene separar dos conceptos que normalmente se confunden.

### 6.1 Método de pago SAT

Debe manejarse con catálogo y validación estricta:

- `PUE` — Pago en una sola exhibición
- `PPD` — Pago en parcialidades o diferido

### 6.2 Forma de pago SAT

No debe capturarse como texto libre. Debe salir de catálogo SAT. Para la primera etapa se recomienda habilitar al menos:

- `01` Efectivo
- `02` Cheque nominativo
- `03` Transferencia electrónica de fondos
- `04` Tarjeta de crédito
- `28` Tarjeta de débito
- `99` Por definir

Si el negocio necesita más formas, el sistema debe soportar el catálogo completo, pero el arranque puede trabajar con un subconjunto controlado.

### 6.3 Regla crítica para cobranza

- Si la factura se liquida al emitirla, usar `MetodoPagoSat = PUE` y la `FormaPagoSat` real.
- Si la venta queda pendiente o parcial, usar `MetodoPagoSat = PPD` y `FormaPagoSat = 99` al emitir el CFDI de ingreso.
- Cada cobro posterior debe generar o preparar el **Complemento de Pago 2.0** con:
  - fecha de pago,
  - forma de pago real,
  - moneda,
  - monto,
  - documento relacionado,
  - saldo anterior,
  - importe pagado,
  - saldo insoluto.

### 6.4 Medio de cobro interno vs forma SAT

Se recomienda que `Zenith` conserve además un campo operativo interno, por ejemplo `MedioCobroInterno`, para conciliación y operación diaria:

- depósito SPEI,
- transferencia bancaria,
- terminal física,
- link de cobro,
- caja,
- cheque,
- compensación.

Ese dato operativo **no sustituye** la `FormaPagoSat`; solo ayuda a tesorería, cobranza y conciliación.

---

## 7. Modelo funcional propuesto dentro de Zenith

### 7.1 Catálogos mínimos

- `RegimenFiscal`
- `UsoCfdi`
- `FormaPagoSat`
- `MetodoPagoSat`
- `ClaveProductoServicioSat`
- `ClaveUnidadSat`
- `ObjetoImpuesto`
- `TipoRelacionCfdi`
- `TipoComprobante`
- `Exportacion`

### 7.2 Revisión del modelo actual de terceros

Con base en las entidades actuales del sistema, hoy existe esta separación:

- `Cliente` para ventas, pedidos y productos por cliente.
- `Proveedor` para compras y cuentas por pagar.

La separación funciona para la operación básica, pero para facturación, cobranza y futura conciliación todavía faltan datos importantes.

#### Lo que hoy le falta a `Cliente`

- `RegimenFiscalReceptor`
- `UsoCfdi` por default
- `DomicilioFiscalCp` explícito como dato fiscal
- distinción entre domicilio fiscal y domicilio comercial
- múltiples correos con propósito fiscal y de cobranza
- estatus de validación SAT o verificación manual del RFC
- condiciones comerciales de crédito y cobranza
- contactos por rol, por ejemplo compras, pagos, fiscal y recepción de CFDI

#### Lo que hoy le falta a `Proveedor`

- homogeneidad con `Cliente` en auditoría y estructura base
- ciudad, estado, país y código postal por separado
- régimen fiscal y datos fiscales más completos
- correo de recepción de órdenes y correo fiscal por separado
- contactos múltiples, no solo un campo `Contacto`
- cuentas bancarias y datos para pagos
- condiciones de pago y plazo por default
- capacidad para identificar si el proveedor también actúa como cliente

#### Lo que hoy también falta en pagos y cobranza

- `PagoPedido` ya puede extenderse para separar `MetodoPagoSat` de la forma simplificada y guardar también el medio interno de cobro.
- `PagoCxP` usa un enum propio de método de pago que no está alineado al catálogo SAT ni a una tabla compartida.
- no existe todavía una entidad fiscal central para UUID, XML, PDF, cancelación o complemento de pago.

### 7.3 Recomendación sobre `Cliente` y `Proveedor` vs `Partner`

La recomendación es **no reemplazar de inmediato** las tablas actuales por una sola tabla `Partner`, porque `Cliente` ya está ampliamente referenciado en ventas, pedidos, contactos, productos por cliente y producción, mientras que `Proveedor` ya está amarrado a cuentas por pagar.

#### Recomendación práctica

**Corto plazo:**

- conservar `Cliente` y `Proveedor`,
- ampliar directamente `Cliente` y `Proveedor` con los datos fiscales y comerciales mínimos para empezar a operar,
- introducir el dominio fiscal base (`Factura`, `PagoRecibido`, `ComplementoPago`) sin romper ventas ni compras,
- dejar las tablas compartidas de terceros como siguiente normalización,
- preparar el dominio fiscal para facturación y cobranza sin romper lo actual.

**Mediano plazo:**

sí conviene evolucionar a un modelo maestro de terceros tipo `Partner`, pero como capa superior y no como cambio brusco.

#### Cuándo sí conviene migrar a `Partner`

Si el negocio necesita cualquiera de estos escenarios, entonces `Partner` sí vale la pena:

- una misma razón social puede ser cliente y proveedor,
- se quiere una sola ficha maestra fiscal y comercial,
- se quiere compartir contactos, domicilios y cuentas bancarias,
- se quiere unificar validaciones fiscales y expedientes documentales,
- se quiere escalar a compras, ventas, cobranza, pagos y contabilidad con un catálogo único de terceros.

#### Modelo recomendado si se adopta `Partner`

- `Partner`
- `PartnerRol` (`Cliente`, `Proveedor`, o ambos)
- `PartnerFiscalProfile`
- `PartnerAddress`
- `PartnerContact`
- `PartnerBankAccount`

Y sobre ese modelo:

- `Pedido` apuntaría a `Partner` con rol cliente,
- `CuentaPorPagar` apuntaría a `Partner` con rol proveedor,
- `Factura` y `PagoRecibido` usarían el mismo tercero maestro.

#### Decisión recomendada para Zenith hoy

Para `Zenith`, la mejor ruta es:

1. **no renombrar todavía** `Cliente` y `Proveedor` a `Partner`,
2. **sí diseñar desde ahora** el modelo fiscal y de integración como si existiera un tercero maestro,
3. **dejar preparada** una migración futura a `Partner` cuando ventas y compras ya compartan más información.

### 7.4 Entidades funcionales sugeridas

- `Factura`  
- `FacturaDetalle`  
- `FacturaImpuesto`  
- `FacturaRelacionada`  
- `FacturaEvento`  
- `PagoRecibido`  
- `PagoAplicacionDocumento`  
- `ComplementoPago`  
- `ComplementoPagoDocumentoRelacionado`  
- `ClienteDatoFiscalSnapshot`

### 7.5 Entidades compartidas recomendadas para terceros

Aunque se conserven `Cliente` y `Proveedor`, conviene introducir entidades transversales:

- `TerceroFiscalProfile`
- `TerceroAddress`
- `TerceroContact`
- `TerceroBankAccount`
- `TerceroCreditProfile`

Estas entidades pueden relacionarse primero con `Cliente` y `Proveedor`, y después migrarse a `Partner` sin rehacer el módulo fiscal.

> **Nota de implementación inicial:** en esta primera aplicación se puede comenzar ampliando directamente `Cliente` y `Proveedor`, y crear después estas entidades compartidas como fase de normalización.

### 7.6 Principio importante

El pedido no debe convertirse automáticamente en CFDI sin una capa intermedia. Lo correcto es:

`Pedido/Servicio` → `Cuenta por cobrar / documento comercial` → `Factura CFDI` → `Cobranza` → `Complemento de pago`

Así se conserva control comercial, fiscal y contable.

---

## 8. Flujo propuesto de operación

### Escenario A — Factura de contado

1. Se confirma el pedido.
2. `Zenith` arma el documento fiscal.
3. Se selecciona `PUE` + forma de pago real.
4. Se envía al proveedor de timbrado.
5. Se guarda UUID, XML, PDF y estatus.
6. La cuenta queda saldada.

### Escenario B — Factura a crédito

1. Se confirma el pedido.
2. `Zenith` genera la cuenta por cobrar.
3. Se emite CFDI con `PPD` + `99 Por definir`.
4. Se registra cada cobro parcial o total.
5. Cada cobro se aplica contra la factura.
6. Se genera complemento de pago cuando corresponda.
7. La cuenta cambia de pendiente a parcial o liquidada.

### Escenario C — Nota de crédito / egreso

1. Se selecciona la factura origen.
2. Se define motivo y relación CFDI.
3. `Zenith` arma CFDI de egreso relacionado.
4. Se timbra con el proveedor.
5. Se ajusta saldo del cliente y trazabilidad.

---

## 9. Recomendación de integración técnica

### 9.1 No acoplar la app al SDK del proveedor en todo el dominio

La mejor práctica es implementar:

- un `modelo canónico interno`,
- un `servicio de integración fiscal`,
- un `adaptador por proveedor`.

Ejemplo conceptual:

- `IFacturacionFiscalService`
- `FacturaFiscalRequest`
- `FacturaFiscalResponse`
- `FacturapiAdapter`

Con esto, si en el futuro cambia el PAC o se integra otro proveedor, el dominio comercial de `Zenith` no se rompe.

### 9.2 Qué sí debe decidir Zenith

- qué pedido o servicio se factura,
- qué conceptos salen,
- qué impuestos aplican,
- si la operación es `PUE` o `PPD`,
- cómo se aplica la cobranza,
- cuándo emitir complemento,
- cómo se reflejan saldos y estados internos.

### 9.3 Qué debe resolver el proveedor

- timbrado,
- cancelación fiscal,
- generación oficial de XML/PDF,
- validaciones propias del PAC,
- estatus fiscal externo.

### 9.4 DTOs y contratos técnicos sugeridos

#### DTOs de salida desde Zenith

- `FacturaFiscalRequest`
- `FacturaFiscalCustomerDto`
- `FacturaFiscalItemDto`
- `FacturaFiscalTaxDto`
- `FacturaFiscalRelatedDocumentDto`
- `ComplementoPagoRequest`
- `ComplementoPagoDocumentoDto`

#### DTOs de respuesta hacia Zenith

- `FacturaFiscalResponse`
- `FacturaFiscalStatusDto`
- `FacturaFiscalCancellationDto`
- `ComplementoPagoResponse`

### 9.5 Servicios de aplicación sugeridos

- `IFacturacionFiscalService`
- `IFacturaBuilder`
- `IComplementoPagoBuilder`
- `IFiscalCatalogService`
- `IFacturaRepository`
- `IPagoRecibidoRepository`

#### Responsabilidades mínimas

- `IFacturaBuilder`: convertir pedido, servicio o cuenta por cobrar a `FacturaFiscalRequest`.
- `IFacturacionFiscalService`: timbrar, consultar, cancelar y sincronizar estatus.
- `IComplementoPagoBuilder`: convertir pagos aplicados a request fiscal de complemento.
- `IFiscalCatalogService`: resolver catálogos SAT y validaciones de captura.

### 9.6 Mapeo inicial recomendado a una API tipo Facturapi

| Zenith | Adaptador tipo Facturapi |
|---|---|
| `FacturaFiscalRequest.Customer.RazonSocial` | `customer.legal_name` |
| `FacturaFiscalRequest.Customer.Rfc` | `customer.tax_id` |
| `FacturaFiscalRequest.Customer.RegimenFiscalReceptor` | `customer.tax_system` |
| `FacturaFiscalRequest.Customer.DomicilioFiscalCp` | `customer.address.zip` |
| `FacturaFiscalRequest.Items[].Descripcion` | `items[].product.description` |
| `FacturaFiscalRequest.Items[].ClaveProductoServicioSat` | `items[].product.product_key` |
| `FacturaFiscalRequest.Items[].ValorUnitario` | `items[].product.price` |
| `FacturaFiscalRequest.Items[].Cantidad` | `items[].quantity` |
| `FacturaFiscalRequest.FormaPagoSat` | `payment_form` |

La recomendación es que este mapeo viva únicamente dentro del `FacturapiAdapter`, no en páginas, repositorios comerciales ni entidades del dominio.

---

## 10. Alcance recomendado por fases

### Fase 1 — Base fiscal

- catálogo de datos fiscales del cliente,
- ampliación de datos maestros de cliente y proveedor,
- catálogo SAT esencial,
- entidad `Factura` y detalle,
- snapshot fiscal del tercero al momento de facturar,
- emisión CFDI ingreso desde pedido/cuenta por cobrar,
- almacenamiento de UUID, XML y PDF.

### Fase 2 — Cobranza formal

- registro de pagos recibidos,
- aplicación por factura,
- saldos insolutos,
- método y forma de pago controlados,
- homologación de catálogos de pago entre CxC y CxP,
- preparación de complemento de pago.

### Fase 3 — Complementos y egresos

- complemento de pago 2.0,
- CFDI de egreso,
- cancelaciones y reemisiones,
- historial de eventos fiscales,
- entidades compartidas de terceros (`fiscal`, `contacto`, `domicilio`, `banco`).

### Fase 4 — Automatización y control

- envío por correo,
- reintentos automáticos,
- conciliación con cuentas por cobrar,
- dashboard de facturación y cobranza,
- alertas de error fiscal y documentos pendientes,
- evaluación de migración gradual a `Partner` si el mismo tercero opera en ambos lados.

---

## 11. Beneficios esperados

- `Zenith` conserva el control del proceso comercial y contable.
- La integración fiscal se vuelve sustituible y escalable.
- La cobranza queda alineada con CFDI 4.0 y complemento de pago.
- Se reduce captura libre y errores en formas/métodos de pago.
- Mejora la trazabilidad entre pedido, factura, pago y saldo.
- La información queda lista para reportes de cuentas por cobrar y seguimiento fiscal.

---

## 12. Recomendación final

Sí conviene integrar una API externa como `Facturapi`, pero `Zenith` no debe modelar su dominio directamente con el contrato del proveedor.

La mejor decisión es construir en `Zenith` un modelo fiscal propio y regresar al adaptador un objeto completo con:

- emisor,
- receptor,
- encabezado fiscal,
- conceptos,
- impuestos,
- relación documental,
- datos de cobranza.

Y de regreso persistir:

- identificador externo,
- UUID,
- estatus fiscal,
- XML,
- PDF,
- acuses,
- errores,
- cancelación,
- relación con pagos y saldo.

Con este enfoque, la propuesta queda compatible con una API tipo `Facturapi`, pero también lista para crecer a una arquitectura fiscal más robusta dentro de `Zenith`.
