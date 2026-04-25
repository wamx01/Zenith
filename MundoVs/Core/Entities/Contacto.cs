namespace MundoVs.Core.Entities;

public class Contacto : BaseEntity
{
    public Guid ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Movil { get; set; }
    public bool EsPrincipal { get; set; }
    
    public Cliente Cliente { get; set; } = null!;
}
