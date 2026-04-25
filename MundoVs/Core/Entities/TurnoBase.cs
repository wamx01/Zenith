namespace MundoVs.Core.Entities;

public class TurnoBase : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public ICollection<TurnoBaseDetalle> Detalles { get; set; } = [];
    public ICollection<Empleado> Empleados { get; set; } = [];
    public ICollection<RrhhEmpleadoTurno> EmpleadosVigencia { get; set; } = [];
}

public class TurnoBaseDetalle : BaseEntity
{
    public Guid TurnoBaseId { get; set; }
    public TurnoBase TurnoBase { get; set; } = null!;

    public DiaSemanaTurno DiaSemana { get; set; }
    public bool Labora { get; set; } = true;
    public TimeSpan? HoraEntrada { get; set; }
    public TimeSpan? HoraSalida { get; set; }
    public byte CantidadDescansos { get; set; }
    public TimeSpan? Descanso1Inicio { get; set; }
    public TimeSpan? Descanso1Fin { get; set; }
    public bool Descanso1EsPagado { get; set; }
    public TimeSpan? Descanso2Inicio { get; set; }
    public TimeSpan? Descanso2Fin { get; set; }
    public bool Descanso2EsPagado { get; set; }
}

public enum DiaSemanaTurno
{
    Lunes = 1,
    Martes = 2,
    Miercoles = 3,
    Jueves = 4,
    Viernes = 5,
    Sabado = 6,
    Domingo = 7
}
