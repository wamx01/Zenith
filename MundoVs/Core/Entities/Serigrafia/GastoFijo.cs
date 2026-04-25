using MundoVs.Core.Entities;

namespace MundoVs.Core.Entities.Serigrafia;

public class GastoFijo : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Concepto { get; set; } = string.Empty;
    public decimal CostoMensual { get; set; }
    public int Orden { get; set; }
}
