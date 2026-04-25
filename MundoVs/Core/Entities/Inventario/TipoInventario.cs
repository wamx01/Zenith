using MundoVs.Core.Entities;

namespace MundoVs.Core.Entities.Inventario;

public class TipoInventario : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid CategoriaInventarioId { get; set; }
    public CategoriaInventario CategoriaInventario { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }
}
