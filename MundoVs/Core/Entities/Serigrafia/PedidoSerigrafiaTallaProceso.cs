namespace MundoVs.Core.Entities.Serigrafia;

public class PedidoSerigrafiaTallaProceso : BaseEntity
{
    public Guid PedidoSerigrafiaId { get; set; }
    public Guid PedidoSerigrafiaTallaId { get; set; }
    public Guid TipoProcesoId { get; set; }
    public Guid? EmpleadoId { get; set; }
    public bool Completado { get; set; }
    public DateTime? FechaPaso { get; set; }

    public PedidoSerigrafia PedidoSerigrafia { get; set; } = null!;
    public PedidoSerigrafiaTalla PedidoSerigrafiaTalla { get; set; } = null!;
    public TipoProceso TipoProceso { get; set; } = null!;
    public Empleado? Empleado { get; set; }
}
