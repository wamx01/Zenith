using MundoVs.Core.Entities.Serigrafia;

namespace MundoVs.Core.Entities;

public class EsquemaPagoTarifa : BaseEntity
{
    public Guid EsquemaPagoId { get; set; }
    public EsquemaPago EsquemaPago { get; set; } = null!;

    public Guid? TipoProcesoId { get; set; }
    public TipoProceso? TipoProceso { get; set; }

    public Guid? PosicionId { get; set; }
    public Posicion? Posicion { get; set; }

    public decimal Tarifa { get; set; }
    public string? Descripcion { get; set; }
}
