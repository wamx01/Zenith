using MundoVs.Core.Entities;

namespace MundoVs.Core.Entities.Serigrafia;

public class EscalaSerigrafia : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public int Total => Tallas?.Sum(t => t.Cantidad) ?? 0;

    public ICollection<EscalaSerigrafiaTalla> Tallas { get; set; } = new List<EscalaSerigrafiaTalla>();
}
