namespace MundoVs.Core.Entities;

public class PedidoConcepto : BaseEntity
{
    public Guid PedidoId { get; set; }
    public PedidoConceptoTipo Tipo { get; set; } = PedidoConceptoTipo.Servicio;
    public string Descripcion { get; set; } = string.Empty;
    public int Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public bool Completado { get; set; }
    public DateTime? FechaCompletado { get; set; }

    public Pedido Pedido { get; set; } = null!;
}

public enum PedidoConceptoTipo
{
    Servicio = 1,
    Entrega = 2,
    Diseno = 3,
    Empaque = 4,
    Otro = 5
}
