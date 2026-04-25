namespace MundoVs.Core.Entities.Serigrafia;

public class ColorSerigrafia : BaseEntity
{
    public Guid ProductoSerigrafiaId { get; set; }
    public string NombreColor { get; set; } = string.Empty;
    public string? CodigoPantone { get; set; }
    public string? CodigoHex { get; set; }
    public int Orden { get; set; }
    
    public ProductoSerigrafia ProductoSerigrafia { get; set; } = null!;
}
