namespace MundoVs.Core.Entities.Serigrafia;

public class Diseno : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public Guid? ClienteId { get; set; }
    public string? RutaArchivo { get; set; }
    public int NumeroColores { get; set; }
    public string? Dimensiones { get; set; }
    public bool Aprobado { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    
    public Cliente? Cliente { get; set; }
}
