namespace MundoVs.Core.Entities;

public class ClienteReglaVariacionPrecio : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public string Dimension { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool PermiteVariacionPrecio { get; set; } = true;
    public decimal PorcentajeVariacionSugerido { get; set; }
    public bool Activa { get; set; } = true;
}
