namespace MundoVs.Core.Entities;

public class ProductoVariante : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public string Sku { get; set; } = string.Empty;
    public string? Talla { get; set; }
    public string? Color { get; set; }
    public decimal? PrecioOverride { get; set; }
    public bool Activa { get; set; } = true;

    public ICollection<PedidoDetalle> PedidoDetalles { get; set; } = new List<PedidoDetalle>();
    public ICollection<PedidoDetalleTalla> PedidoDetallesTalla { get; set; } = new List<PedidoDetalleTalla>();
}