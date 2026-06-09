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

    /// <summary>
    /// Determina si este tipo de ausencia siempre debe tener goce de pago
    /// </summary>
    public bool DebeSerConGocePago() => Tipo switch
    {
        TipoAusenciaRrhh.Vacaciones => true,
        TipoAusenciaRrhh.Capacitacion => true,
        TipoAusenciaRrhh.Incapacidad => true,
        TipoAusenciaRrhh.PermisoConGoce => true,
        TipoAusenciaRrhh.DiasEconomicos => true,
        TipoAusenciaRrhh.PermisoPaternidad => true,
        TipoAusenciaRrhh.PermisoMaternidad => true,
        _ => false
    };

    /// <summary>
    /// Determina si este tipo puede configurarse el goce de pago
    /// </summary>
    public bool PuedeConfigurarGocePago() => Tipo switch
    {
        TipoAusenciaRrhh.Permiso => true,
        _ => false
    };
}

public enum TipoAusenciaRrhh
{
    Vacaciones = 1,
    Permiso = 2,
    PermisoConGoce = 3,
    PermisoSinGoce = 4,
    Capacitacion = 5,
    Incapacidad = 6,
    FaltaInjustificada = 7,
    Suspension = 8,
    DiasEconomicos = 9,
    PermisoPaternidad = 10,
    PermisoMaternidad = 11
}

public enum EstatusAusenciaRrhh
{
    Solicitada = 1,
    Aprobada = 2,
    Rechazada = 3,
    Aplicada = 4,
    Cancelada = 5
}
