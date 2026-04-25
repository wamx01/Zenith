namespace MundoVs.Core.Entities.Serigrafia;

public class PedidoSerigrafia : BaseEntity
{
    public Guid PedidoDetalleId { get; set; }

    // Información del producto
    public string Estilo { get; set; } = string.Empty;
    public string CombinacionColor { get; set; } = string.Empty;

    // Lote y corrida
    public string? OrdenCompra { get; set; }
    public string? LoteCliente { get; set; }
    public string? Corrida { get; set; }

    // Cantidad total calculada desde tallas
    public int CantidadTotal => Tallas?.Sum(t => t.Cantidad) ?? 0;

    // Fechas
    public DateTime? FechaRecibido { get; set; }
    public DateTime? FechaEstimada { get; set; }
    public DateTime? FechaEntregaReal { get; set; }

    // Estado
    public bool Hecho { get; set; }

    // Factura
    public string? Factura { get; set; }

    // Navegación
    public PedidoDetalle PedidoDetalle { get; set; } = null!;
    public ICollection<PedidoSerigrafiaProcesoDetalle> TiposProceso { get; set; } = new List<PedidoSerigrafiaProcesoDetalle>();
    public ICollection<PedidoSerigrafiaTalla> Tallas { get; set; } = new List<PedidoSerigrafiaTalla>();
}
