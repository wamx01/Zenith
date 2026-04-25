using MundoVs.Core.Entities.Serigrafia;

namespace MundoVs.Core.Entities;

public class BonoDistribucionPeriodoRrhh : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Periodo { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string? Departamento { get; set; }

    public Guid PosicionId { get; set; }
    public Posicion Posicion { get; set; } = null!;

    public Guid BonoEstructuraRrhhId { get; set; }
    public BonoEstructuraRrhh BonoEstructuraRrhh { get; set; } = null!;

    public decimal MontoTotalDistribuir { get; set; }
    public string? Observaciones { get; set; }

    public ICollection<BonoDistribucionEmpleadoRrhh> Detalles { get; set; } = [];
}

public class BonoDistribucionEmpleadoRrhh : BaseEntity
{
    public Guid BonoDistribucionPeriodoRrhhId { get; set; }
    public BonoDistribucionPeriodoRrhh BonoDistribucionPeriodoRrhh { get; set; } = null!;

    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public decimal Porcentaje { get; set; }
    public decimal MontoAsignado { get; set; }
    public string? Observaciones { get; set; }

    public ICollection<BonoDistribucionEmpleadoDetalleRrhh> Detalles { get; set; } = [];
}

public class BonoDistribucionEmpleadoDetalleRrhh : BaseEntity
{
    public Guid BonoDistribucionEmpleadoRrhhId { get; set; }
    public BonoDistribucionEmpleadoRrhh BonoDistribucionEmpleadoRrhh { get; set; } = null!;

    public Guid BonoRubroRrhhId { get; set; }
    public BonoRubroRrhh BonoRubroRrhh { get; set; } = null!;

    public decimal Porcentaje { get; set; }
    public decimal MontoAsignado { get; set; }
    public int Orden { get; set; }
}
