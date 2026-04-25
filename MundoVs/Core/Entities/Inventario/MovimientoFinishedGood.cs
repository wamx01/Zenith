using MundoVs.Core.Entities;
using MundoVs.Core.Entities.Serigrafia;

namespace MundoVs.Core.Entities.Inventario;

public class MovimientoFinishedGood : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid InventarioFinishedGoodId { get; set; }
    public InventarioFinishedGood InventarioFinishedGood { get; set; } = null!;

    public MovimientoFinishedGoodTipoEnum TipoMovimiento { get; set; }
    public decimal Cantidad { get; set; }
    public decimal ExistenciaAnterior { get; set; }
    public decimal ExistenciaNueva { get; set; }
    public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;
    public string? Referencia { get; set; }
    public string? Observaciones { get; set; }

    public Guid? PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public Guid? PedidoSerigrafiaId { get; set; }
    public PedidoSerigrafia? PedidoSerigrafia { get; set; }

    public Guid? PedidoDetalleTallaId { get; set; }
    public PedidoDetalleTalla? PedidoDetalleTalla { get; set; }

    public Guid? NotaEntregaId { get; set; }
    public NotaEntrega? NotaEntrega { get; set; }
}

public enum MovimientoFinishedGoodTipoEnum
{
    Ajuste = 1,
    IngresoProduccion = 2,
    SalidaNotaEntrega = 3,
    CancelacionNota = 4
}
