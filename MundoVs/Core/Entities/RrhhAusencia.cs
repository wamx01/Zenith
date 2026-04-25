namespace MundoVs.Core.Entities;

public class RrhhAusencia : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public TipoAusenciaRrhh Tipo { get; set; }
    public EstatusAusenciaRrhh Estatus { get; set; } = EstatusAusenciaRrhh.Solicitada;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public int Dias { get; set; }
    public decimal Horas { get; set; }
    public bool ConGocePago { get; set; }
    public string? Motivo { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string? AprobadoPor { get; set; }
}

public enum TipoAusenciaRrhh
{
    Vacaciones = 1,
    Permiso = 2
}

public enum EstatusAusenciaRrhh
{
    Solicitada = 1,
    Aprobada = 2,
    Rechazada = 3,
    Aplicada = 4,
    Cancelada = 5
}
