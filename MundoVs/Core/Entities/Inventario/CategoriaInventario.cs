using MundoVs.Core.Entities;

namespace MundoVs.Core.Entities.Inventario;

public class CategoriaInventario : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int Orden { get; set; }

    public ICollection<TipoInventario> Tipos { get; set; } = [];
}
