namespace MundoVs.Core.Entities;

public class RrhhEmpleadoTurno : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public Guid TurnoBaseId { get; set; }
    public TurnoBase TurnoBase { get; set; } = null!;

    public DateOnly VigenteDesde { get; set; }
    public DateOnly? VigenteHasta { get; set; }
    public string? Observaciones { get; set; }
}
