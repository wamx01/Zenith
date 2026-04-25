namespace MundoVs.Core.Entities.Calzado;

public class ProductoCalzado : BaseEntity
{
    public Guid ProductoId { get; set; }
    public string? Modelo { get; set; }
    public string? Material { get; set; }
    public string? TipoSuela { get; set; }
    public string? Color { get; set; }
    public string? Temporada { get; set; }
    public GeneroEnum Genero { get; set; }
    
    public Producto Producto { get; set; } = null!;
    public ICollection<TallaCalzado> Tallas { get; set; } = new List<TallaCalzado>();
}

public enum GeneroEnum
{
    Hombre = 1,
    Mujer = 2,
    Nino = 3,
    Nina = 4,
    Unisex = 5
}
