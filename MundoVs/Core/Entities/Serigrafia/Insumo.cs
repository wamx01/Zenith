using MundoVs.Core.Entities;
using MundoVs.Core.Entities.Inventario;

namespace MundoVs.Core.Entities.Serigrafia;

public class Insumo : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public Guid? TipoInventarioId { get; set; }
    public TipoInventario? TipoInventario { get; set; }
    public TipoInsumoEnum TipoInsumo { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = "pza";
    public decimal StockMinimo { get; set; }
}

public enum TipoInsumoEnum
{
    Basico = 1,
    Diverso = 2
}
