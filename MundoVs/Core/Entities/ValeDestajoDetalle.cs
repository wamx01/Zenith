using MundoVs.Core.Entities.Serigrafia;

namespace MundoVs.Core.Entities;

public class ValeDestajoDetalle : BaseEntity
{
    public Guid ValeDestajoId { get; set; }
    public ValeDestajo ValeDestajo { get; set; } = null!;

    public Guid TipoProcesoId { get; set; }
    public TipoProceso TipoProceso { get; set; } = null!;

    public Guid? PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public int Cantidad { get; set; }
    public decimal TarifaAplicada { get; set; }
    public decimal Importe { get; set; }

    public Guid? EsquemaPagoTarifaId { get; set; }
    public EsquemaPagoTarifa? EsquemaPagoTarifa { get; set; }

    public int? TiempoMinutos { get; set; }
    public string? Observaciones { get; set; }

    public ICollection<RegistroDestajoProceso> RegistrosOrigen { get; set; } = [];
}
