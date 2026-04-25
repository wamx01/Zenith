namespace MundoVs.Core.Entities;

public class AuditLog : BaseEntity
{
    public Guid? EmpresaId { get; set; }
    public Guid? UsuarioId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public Guid? EntidadId { get; set; }
    public string? Detalle { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
}
