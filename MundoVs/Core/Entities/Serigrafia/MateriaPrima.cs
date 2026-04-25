using MundoVs.Core.Entities;
using MundoVs.Core.Entities.Inventario;

namespace MundoVs.Core.Entities.Serigrafia;

public class MateriaPrima : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public Guid? TipoInventarioId { get; set; }
    public TipoInventario? TipoInventario { get; set; }
    public TipoMateriaPrimaEnum TipoMateriaPrima { get; set; }
    public string? CodigoPantone { get; set; }
    public string? CodigoHex { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = "kg";
    public decimal StockMinimo { get; set; }
}

public enum TipoMateriaPrimaEnum
{
    Tinta = 1,
    Base = 2,
    Solvente = 3,
    Aditivo = 4,
    Otro = 5
}
