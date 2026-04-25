namespace MundoVs.Core.Entities.Serigrafia;

public class PedidoSerigrafiaProcesoDetalle : BaseEntity
{
    public Guid PedidoSerigrafiaId { get; set; }
    public Guid TipoProcesoId { get; set; }
    
    // Navegación
    public PedidoSerigrafia PedidoSerigrafia { get; set; } = null!;
    public TipoProceso TipoProceso { get; set; } = null!;
}
