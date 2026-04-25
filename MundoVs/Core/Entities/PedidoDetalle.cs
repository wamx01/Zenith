using MundoVs.Core.Entities.Calzado;
using MundoVs.Core.Entities.Serigrafia;

namespace MundoVs.Core.Entities;

public class PedidoDetalle : BaseEntity
{
    public Guid PedidoId { get; set; }
    public Guid ProductoId { get; set; }
    public Guid? ProductoVarianteId { get; set; }
    public Guid? CotizacionSerigrafiaId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public string? TallaBaseCalzado { get; set; }
    public string? VariacionValor { get; set; }
    public bool AplicaFraccionCalzado { get; set; }
    public Guid? ClienteFraccionCalzadoId { get; set; }
    public string? Especificaciones { get; set; }
    
    public Pedido Pedido { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
    public ProductoVariante? ProductoVariante { get; set; }
    public ClienteFraccionCalzado? ClienteFraccionCalzado { get; set; }
    public CotizacionSerigrafia? CotizacionSerigrafia { get; set; }
    public ICollection<PedidoDetalleTalla> DetallesTalla { get; set; } = new List<PedidoDetalleTalla>();
    public ICollection<NotaEntregaDetalle> NotasEntregaDetalle { get; set; } = new List<NotaEntregaDetalle>();
}
