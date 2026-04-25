namespace MundoVs.Core.Entities;

public class PedidoDetalleTalla : BaseEntity
{
    public Guid PedidoDetalleId { get; set; }
    public Guid? ProductoVarianteId { get; set; }
    public Guid? ClienteFraccionCalzadoDetalleId { get; set; }
    public string Talla { get; set; } = string.Empty;
    public int Orden { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public decimal PorcentajeVariacion { get; set; }
    public bool GeneradaDesdeFraccion { get; set; }

    public PedidoDetalle PedidoDetalle { get; set; } = null!;
    public ProductoVariante? ProductoVariante { get; set; }
}
