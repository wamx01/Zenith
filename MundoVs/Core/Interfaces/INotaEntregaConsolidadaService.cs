using MundoVs.Core.Entities;

namespace MundoVs.Core.Interfaces;

public interface INotaEntregaConsolidadaService
{
    Task<IReadOnlyList<NotaEntregaPedidoElegibleVm>> GetPedidosElegiblesAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotaEntregaLineaConsolidadaVm>> BuildLineasConsolidadasAsync(IReadOnlyCollection<Guid> pedidoIds, CancellationToken cancellationToken = default);
}

public sealed class NotaEntregaPedidoElegibleVm
{
    public Guid PedidoId { get; init; }
    public string NumeroPedido { get; init; } = string.Empty;
    public DateTime FechaPedido { get; init; }
    public decimal TotalPedido { get; init; }
    public decimal CantidadProductoPedida { get; init; }
    public decimal CantidadProductoEntregada { get; init; }
    public decimal CantidadProductoPendiente => Math.Max(0, CantidadProductoPedida - CantidadProductoEntregada);
    public decimal CantidadServiciosPendientes { get; init; }
    public bool TieneNotasActivas { get; init; }
    public bool Elegible { get; init; }
    public string MotivoBloqueo { get; init; } = string.Empty;
}

public sealed class NotaEntregaLineaConsolidadaVm
{
    public Guid PedidoId { get; init; }
    public string NumeroPedido { get; init; } = string.Empty;
    public Guid? PedidoDetalleId { get; init; }
    public Guid? PedidoDetalleTallaId { get; init; }
    public Guid? PedidoConceptoId { get; init; }
    public NotaEntregaAsignacionOrigenTipo TipoOrigen { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public string? Talla { get; init; }
    public int OrdenTalla { get; init; }
    public decimal CantidadMaximaTomable { get; init; }
    public decimal CantidadSeleccionada { get; set; }
    public decimal CantidadDisponibleFg { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal Importe => Math.Round(CantidadSeleccionada * PrecioUnitario, 2);
}
