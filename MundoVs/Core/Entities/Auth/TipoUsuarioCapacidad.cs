namespace MundoVs.Core.Entities.Auth;

public class TipoUsuarioCapacidad
{
    public Guid TipoUsuarioId { get; set; }
    public Guid CapacidadId { get; set; }

    public TipoUsuario TipoUsuario { get; set; } = null!;
    public Capacidad Capacidad { get; set; } = null!;
}
