namespace MundoVs.Core.Entities.Serigrafia;

public class EscalaSerigrafiaTalla : BaseEntity
{
    public Guid EscalaSerigrafiaId { get; set; }
    public string Talla { get; set; } = string.Empty;
    public int Cantidad { get; set; }

    public EscalaSerigrafia EscalaSerigrafia { get; set; } = null!;
}
