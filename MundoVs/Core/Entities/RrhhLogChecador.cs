namespace MundoVs.Core.Entities;

public class RrhhLogChecador : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid? ChecadorId { get; set; }
    public RrhhChecador? Checador { get; set; }

    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
    public string Nivel { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Detalle { get; set; }
}