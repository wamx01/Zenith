namespace MundoVs.Core.Entities.Serigrafia;

public class CotizacionSerigrafiaProceso : BaseEntity
{
    public Guid CotizacionSerigrafiaId { get; set; }
    public Guid TipoProcesoId { get; set; }

    public CotizacionSerigrafia CotizacionSerigrafia { get; set; } = null!;
    public TipoProceso TipoProceso { get; set; } = null!;
}
