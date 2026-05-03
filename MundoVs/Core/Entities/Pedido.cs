namespace MundoVs.Core.Entities;

public class Pedido : BaseEntity
{
    public string NumeroPedido { get; set; } = string.Empty;
    public Guid ClienteId { get; set; }
    public DateTime FechaPedido { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEntregaEstimada { get; set; }
    public EstadoPedidoEnum Estado { get; set; } = EstadoPedidoEnum.Nuevo;
    public TipoPrecioEnum TipoPrecio { get; set; } = TipoPrecioEnum.Contado;
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
    public ICollection<PedidoConcepto> Conceptos { get; set; } = new List<PedidoConcepto>();
    public ICollection<PedidoSeguimiento> Seguimientos { get; set; } = new List<PedidoSeguimiento>();
    public ICollection<PagoPedido> Pagos { get; set; } = new List<PagoPedido>();
    public ICollection<NotaEntrega> NotasEntrega { get; set; } = new List<NotaEntrega>();
    public ICollection<NotaEntregaPedido> NotasEntregaRelacionadas { get; set; } = new List<NotaEntregaPedido>();
    public ICollection<NotaEntregaAsignacion> NotasEntregaAsignaciones { get; set; } = new List<NotaEntregaAsignacion>();
    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    public ICollection<PagoRecibido> PagosRecibidos { get; set; } = new List<PagoRecibido>();
}

public enum TipoPrecioEnum
{
    Contado = 1,
    Credito = 2
}

public enum EstadoPedidoEnum
{
    Nuevo = 1,
    EnProceso = 2,
    Producido = 3,
    Entregado = 4,
    Facturado = 5,
    Cancelado = 6,
    Pagado = 7
}
