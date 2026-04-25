using MundoVs.Core.Entities;

namespace MundoVs.Core.Entities.Serigrafia;

public class Posicion : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public decimal TarifaPorMinuto { get; set; }
    public int Orden { get; set; }
    public Guid? BonoEstructuraRrhhId { get; set; }
    public decimal MontoBonoDistribuido { get; set; }

    public BonoEstructuraRrhh? BonoEstructuraRrhh { get; set; }
}
