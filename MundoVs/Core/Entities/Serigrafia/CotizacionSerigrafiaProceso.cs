namespace MundoVs.Core.Entities.Serigrafia;

public class CotizacionSerigrafiaProceso : BaseEntity
{
    public Guid CotizacionSerigrafiaId { get; set; }
    public Guid TipoProcesoId { get; set; }
    public decimal Multiplicador { get; set; } = 1m;
    public decimal MinutosEstandarAplicados { get; set; }
    public decimal TiempoTotal { get; set; }
    public int Orden { get; set; }

    public CotizacionSerigrafia CotizacionSerigrafia { get; set; } = null!;
    public TipoProceso TipoProceso { get; set; } = null!;
}
