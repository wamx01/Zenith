using MundoVs.Core.Entities;

namespace MundoVs.Core.Entities.Inventario;

public class InventarioItem : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public Guid? CategoriaInventarioId { get; set; }
    public CategoriaInventario? CategoriaInventario { get; set; }

    public Guid? TipoInventarioId { get; set; }
    public TipoInventario? TipoInventario { get; set; }

    public InventarioItemOrigenLegacy OrigenLegacy { get; set; }
    public Guid? MateriaPrimaOrigenId { get; set; }
    public Guid? InsumoOrigenId { get; set; }

    public string? CodigoPantone { get; set; }
    public string? CodigoHex { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = "pza";
    public decimal StockMinimo { get; set; }
}

public enum InventarioItemOrigenLegacy
{
    MateriaPrima = 1,
    Insumo = 2
}
