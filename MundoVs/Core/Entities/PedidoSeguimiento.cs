namespace MundoVs.Core.Entities;

public class PedidoSeguimiento : BaseEntity
{
    public Guid PedidoId { get; set; }

    public PedidoSeguimientoTipo Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public bool Completado { get; set; }
    public DateTime? FechaCompletado { get; set; }

    public Pedido Pedido { get; set; } = null!;
}

public enum PedidoSeguimientoTipo
{
    Nota = 1,
    Estado = 2,
    Proceso = 3,
    Entrega = 4
}
