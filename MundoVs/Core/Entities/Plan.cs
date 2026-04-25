namespace MundoVs.Core.Entities;

public class Plan : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioMensual { get; set; }
    public decimal PrecioAnual { get; set; }
    public int? LimiteUsuarios { get; set; }
    public string? ModulosIncluidos { get; set; }
    public int TrialDays { get; set; }

    public ICollection<SuscripcionEmpresa> Suscripciones { get; set; } = new List<SuscripcionEmpresa>();
}
