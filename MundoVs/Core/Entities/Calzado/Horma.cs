using MundoVs.Core.Entities;

namespace MundoVs.Core.Entities.Calzado;

public class Horma : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Talla { get; set; } = string.Empty;
    public string? Medidas { get; set; }
    public int StockDisponible { get; set; }
    public EstadoHormaEnum Estado { get; set; } = EstadoHormaEnum.Disponible;
}

public enum EstadoHormaEnum
{
    Disponible = 1,
    EnUso = 2,
    Mantenimiento = 3,
    Desgastada = 4
}
