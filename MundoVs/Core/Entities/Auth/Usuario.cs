namespace MundoVs.Core.Entities.Auth;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmpresaId { get; set; }
    public Entities.Empresa Empresa { get; set; } = null!;

    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid TipoUsuarioId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool RequiereCambioPassword { get; set; }
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHastaUtc { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? UltimoAcceso { get; set; }

    public TipoUsuario TipoUsuario { get; set; } = null!;
}
