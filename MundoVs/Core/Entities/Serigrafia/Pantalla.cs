using MundoVs.Core.Entities;

namespace MundoVs.Core.Entities.Serigrafia;

public class Pantalla : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int MallaNumero { get; set; }
    public string? Dimensiones { get; set; }
    public EstadoPantallaEnum Estado { get; set; } = EstadoPantallaEnum.Nueva;
    public int UsosTotales { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public string? DisenoPara { get; set; }
}

public enum EstadoPantallaEnum
{
    Nueva = 1,
    Activa = 2,
    Desgastada = 3,
    Recuperable = 4,
    Inservible = 5
}
