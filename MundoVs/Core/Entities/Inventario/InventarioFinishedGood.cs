using MundoVs.Core.Entities;

namespace MundoVs.Core.Entities.Inventario;

public class InventarioFinishedGood : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public Guid ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public Guid ProductoVarianteId { get; set; }
    public ProductoVariante ProductoVariante { get; set; } = null!;

    public string Sku { get; set; } = string.Empty;
    public string Talla { get; set; } = string.Empty;
    public string? Color { get; set; }
    public decimal CantidadDisponible { get; set; }
    public decimal CantidadReservada { get; set; }
    public string? Ubicacion { get; set; }
    public string? Observaciones { get; set; }

    public ICollection<MovimientoFinishedGood> Movimientos { get; set; } = [];
}
