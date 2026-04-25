namespace MundoVs.Core.Entities.Calzado;

public class TallaCalzado : BaseEntity
{
    public Guid ProductoCalzadoId { get; set; }
    public string Talla { get; set; } = string.Empty;
    public string? TallaUS { get; set; }
    public string? TallaEU { get; set; }
    public string? TallaUK { get; set; }
    public int StockDisponible { get; set; }
    
    public ProductoCalzado ProductoCalzado { get; set; } = null!;
}
