namespace MundoVs.Core.Entities.Serigrafia;

public class ProductoSerigrafia : BaseEntity
{
    public Guid ProductoId { get; set; }
    public TipoImpresionEnum TipoImpresion { get; set; }
    public string? MaterialBase { get; set; }
    public int NumeroColores { get; set; }
    public string? TamanoImpresion { get; set; }
    public string? TipoTinta { get; set; }
    
    public Producto Producto { get; set; } = null!;
    public ICollection<ColorSerigrafia> Colores { get; set; } = new List<ColorSerigrafia>();
}

public enum TipoImpresionEnum
{
    Textil = 1,
    Papel = 2,
    Plastico = 3,
    Madera = 4,
    Metal = 5,
    Vidrio = 6
}
