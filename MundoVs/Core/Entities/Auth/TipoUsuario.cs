namespace MundoVs.Core.Entities.Auth;

public class TipoUsuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Entities.Empresa Empresa { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<TipoUsuarioCapacidad> Capacidades { get; set; } = [];
    public ICollection<Usuario> Usuarios { get; set; } = [];
}
