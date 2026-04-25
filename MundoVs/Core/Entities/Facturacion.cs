namespace MundoVs.Core.Entities;

public enum FacturaTipoComprobante
{
    Ingreso = 1,
    Egreso = 2,
    Pago = 3
}

public enum FacturaEstatus
{
    Borrador = 1,
    PendienteTimbrado = 2,
    Timbrado = 3,
    ErrorTimbrado = 4,
    PendienteCancelacion = 5,
    Cancelado = 6,
    PagadoParcial = 7,
    PagadoTotal = 8
}

public enum NotaEntregaEstatus
{
    Borrador = 1,
    Emitida = 2,
    FacturadaParcial = 3,
    FacturadaTotal = 4,
    Cancelada = 5
}

public class NotaEntrega : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public Guid PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;

    public string NumeroNota { get; set; } = string.Empty;
    public DateTime FechaNota { get; set; } = DateTime.UtcNow;
    public NotaEntregaEstatus Estatus { get; set; } = NotaEntregaEstatus.Borrador;
    public bool NoRequiereFactura { get; set; }
    public string? Observaciones { get; set; }
    public string? PdfUrl { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }

    public ICollection<NotaEntregaDetalle> Detalles { get; set; } = [];
    public ICollection<FacturaNotaEntrega> FacturasRelacionadas { get; set; } = [];
}

public class NotaEntregaDetalle : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid NotaEntregaId { get; set; }
    public NotaEntrega NotaEntrega { get; set; } = null!;

    public Guid? PedidoDetalleId { get; set; }
    public PedidoDetalle? PedidoDetalle { get; set; }

    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Importe { get; set; }

    public ICollection<NotaEntregaDetalleTalla> Tallas { get; set; } = [];
}

public class NotaEntregaDetalleTalla : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid NotaEntregaDetalleId { get; set; }
    public NotaEntregaDetalle NotaEntregaDetalle { get; set; } = null!;

    public Guid? PedidoDetalleTallaId { get; set; }

    public string Talla { get; set; } = string.Empty;
    public int Orden { get; set; }
    public decimal Cantidad { get; set; }
}

public class Factura : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public Guid? PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public Guid? NotaEntregaId { get; set; }
    public NotaEntrega? NotaEntrega { get; set; }

    public FacturaTipoComprobante TipoComprobante { get; set; } = FacturaTipoComprobante.Ingreso;
    public FacturaEstatus Estatus { get; set; } = FacturaEstatus.Borrador;

    public string? Serie { get; set; }
    public string FolioInterno { get; set; } = string.Empty;
    public string? SerieFiscal { get; set; }
    public string? FolioFiscal { get; set; }
    public string? ExternalDocumentId { get; set; }
    public string? UuidFiscal { get; set; }
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public DateTime? FechaTimbrado { get; set; }
    public string Moneda { get; set; } = "MXN";
    public decimal TipoCambio { get; set; } = 1m;
    public string? LugarExpedicionCp { get; set; }
    public string MetodoPagoSat { get; set; } = "PUE";
    public string FormaPagoSat { get; set; } = "99";
    public string? UsoCfdi { get; set; }
    public string? Exportacion { get; set; } = "01";
    public string? CondicionesPago { get; set; }
    public string? Observaciones { get; set; }

    public decimal Subtotal { get; set; }
    public decimal DescuentoTotal { get; set; }
    public decimal TotalImpuestosTrasladados { get; set; }
    public decimal TotalImpuestosRetenidos { get; set; }
    public decimal Total { get; set; }

    public bool Cancelable { get; set; }
    public string? CancellationStatus { get; set; }
    public string? XmlUrl { get; set; }
    public string? PdfUrl { get; set; }
    public string? AcuseCancelacionUrl { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? JsonSnapshotComercial { get; set; }

    public ICollection<FacturaNotaEntrega> NotasEntregaRelacionadas { get; set; } = [];
    public ICollection<FacturaDetalle> Detalles { get; set; } = [];
    public ICollection<FacturaImpuesto> Impuestos { get; set; } = [];
    public ICollection<FacturaRelacionada> FacturasRelacionadas { get; set; } = [];
    public ICollection<FacturaEvento> Eventos { get; set; } = [];
    public ICollection<PagoAplicacionDocumento> PagosAplicados { get; set; } = [];
    public ICollection<ComplementoPagoDocumentoRelacionado> ComplementosRelacionados { get; set; } = [];
}

public class FacturaNotaEntrega : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid FacturaId { get; set; }
    public Factura Factura { get; set; } = null!;

    public Guid NotaEntregaId { get; set; }
    public NotaEntrega NotaEntrega { get; set; } = null!;

    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
}

public class FacturaDetalle : BaseEntity
{
    public Guid FacturaId { get; set; }
    public Factura Factura { get; set; } = null!;

    public string? ReferenciaOrigen { get; set; }
    public decimal Cantidad { get; set; }
    public string? ClaveUnidadSat { get; set; }
    public string? Unidad { get; set; }
    public string? ClaveProductoServicioSat { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal ValorUnitario { get; set; }
    public decimal Descuento { get; set; }
    public string? ObjetoImpuesto { get; set; }
    public decimal Importe { get; set; }

    public ICollection<FacturaImpuesto> Impuestos { get; set; } = [];
}

public class FacturaImpuesto : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid FacturaId { get; set; }
    public Factura Factura { get; set; } = null!;

    public Guid? FacturaDetalleId { get; set; }
    public FacturaDetalle? FacturaDetalle { get; set; }

    public string Impuesto { get; set; } = string.Empty;
    public string TipoFactor { get; set; } = "Tasa";
    public decimal TasaOCuota { get; set; }
    public decimal Base { get; set; }
    public decimal Importe { get; set; }
    public bool EsRetencion { get; set; }
}

public class FacturaRelacionada : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid FacturaId { get; set; }
    public Factura Factura { get; set; } = null!;

    public string TipoRelacionCfdi { get; set; } = string.Empty;
    public string UuidRelacionado { get; set; } = string.Empty;
}

public class FacturaEvento : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid FacturaId { get; set; }
    public Factura Factura { get; set; } = null!;

    public string Tipo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? Usuario { get; set; }
    public string? Descripcion { get; set; }
    public string? DatosJson { get; set; }
}

public class PagoRecibido : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public Guid? PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public Guid? NotaEntregaId { get; set; }
    public NotaEntrega? NotaEntrega { get; set; }

    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
    public decimal Monto { get; set; }
    public string FormaPagoSat { get; set; } = "03";
    public string? MedioCobroInterno { get; set; }
    public string? Referencia { get; set; }
    public string? Notas { get; set; }

    public ICollection<PagoAplicacionDocumento> Aplicaciones { get; set; } = [];
    public ICollection<ComplementoPago> ComplementosPago { get; set; } = [];
}

public class PagoAplicacionDocumento : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid PagoRecibidoId { get; set; }
    public PagoRecibido PagoRecibido { get; set; } = null!;

    public Guid FacturaId { get; set; }
    public Factura Factura { get; set; } = null!;

    public decimal ImporteAplicado { get; set; }
    public decimal SaldoAnterior { get; set; }
    public decimal SaldoInsoluto { get; set; }
}

public class ComplementoPago : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public Guid? PagoRecibidoId { get; set; }
    public PagoRecibido? PagoRecibido { get; set; }

    public FacturaEstatus Estatus { get; set; } = FacturaEstatus.Borrador;
    public string? Serie { get; set; }
    public string FolioInterno { get; set; } = string.Empty;
    public string? SerieFiscal { get; set; }
    public string? FolioFiscal { get; set; }
    public string? ExternalDocumentId { get; set; }
    public string? UuidFiscal { get; set; }
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public DateTime? FechaTimbrado { get; set; }
    public string Moneda { get; set; } = "MXN";
    public decimal TipoCambio { get; set; } = 1m;
    public decimal MontoTotalPagos { get; set; }
    public string? XmlUrl { get; set; }
    public string? PdfUrl { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public ICollection<ComplementoPagoDocumentoRelacionado> DocumentosRelacionados { get; set; } = [];
}

public class ComplementoPagoDocumentoRelacionado : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid ComplementoPagoId { get; set; }
    public ComplementoPago ComplementoPago { get; set; } = null!;

    public Guid FacturaId { get; set; }
    public Factura Factura { get; set; } = null!;

    public string? UuidDocumentoRelacionado { get; set; }
    public string MonedaDocumento { get; set; } = "MXN";
    public decimal TipoCambioDocumento { get; set; } = 1m;
    public decimal SaldoAnterior { get; set; }
    public decimal ImportePagado { get; set; }
    public decimal SaldoInsoluto { get; set; }
}

public class ClienteDatoFiscalSnapshot : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public Guid? FacturaId { get; set; }
    public Factura? Factura { get; set; }

    public string RazonSocial { get; set; } = string.Empty;
    public string Rfc { get; set; } = string.Empty;
    public string? RegimenFiscalReceptor { get; set; }
    public string? DomicilioFiscalCp { get; set; }
    public string? UsoCfdi { get; set; }
    public string? EmailFactura { get; set; }
    public string? JsonDatos { get; set; }
}
