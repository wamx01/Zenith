using MundoVs.Core.Entities;

namespace MundoVs.Core.Entities.Serigrafia;

public class RegistroDestajoProceso : BaseEntity
{
    public Guid PedidoSerigrafiaId { get; set; }
    public Guid PedidoSerigrafiaTallaProcesoId { get; set; }
    public Guid PedidoSerigrafiaTallaId { get; set; }
    public Guid? ValeDestajoDetalleId { get; set; }
    public Guid TipoProcesoId { get; set; }
    public Guid EmpleadoId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public int CantidadProcesada { get; set; }
    public decimal TarifaUnitario { get; set; }
    public decimal Importe { get; set; }
    public int? TiempoMinutos { get; set; }
    public string? Observaciones { get; set; }

    public PedidoSerigrafia PedidoSerigrafia { get; set; } = null!;
    public PedidoSerigrafiaTallaProceso PedidoSerigrafiaTallaProceso { get; set; } = null!;
    public PedidoSerigrafiaTalla PedidoSerigrafiaTalla { get; set; } = null!;
    public ValeDestajoDetalle? ValeDestajoDetalle { get; set; }
    public TipoProceso TipoProceso { get; set; } = null!;
    public Empleado Empleado { get; set; } = null!;
}
