namespace MundoVs.Core.Entities.Serigrafia;

public class PedidoSerigrafiaTalla : BaseEntity
{
    public Guid PedidoSerigrafiaId { get; set; }
    public string Talla { get; set; } = string.Empty;
    public int Cantidad { get; set; }

    public PedidoSerigrafia PedidoSerigrafia { get; set; } = null!;
}
