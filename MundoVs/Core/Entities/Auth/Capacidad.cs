namespace MundoVs.Core.Entities.Auth;

public class Capacidad
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public Guid ModuloAccesoId { get; set; }
    public ModuloAcceso ModuloAcceso { get; set; } = null!;
    public string? Descripcion { get; set; }

    public ICollection<TipoUsuarioCapacidad> TiposUsuario { get; set; } = [];
}
