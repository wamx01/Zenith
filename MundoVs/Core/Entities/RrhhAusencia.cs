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

    /// <summary>
    /// Indica si las horas de esta ausencia se descuentan del banco de horas acumulado del empleado.
    /// Es independiente de ConGocePago: capacitación, incapacidad y maternidad son con goce pero NO descuentan banco.
    /// Solo aplica a permisos parciales donde el empleado "gasta" su banco.
    /// </summary>
    public bool DescuentaBancoHoras { get; set; }

    public string? Motivo { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string? AprobadoPor { get; set; }

    /// <summary>
    /// Identificador del periodo al que pertenece esta ausencia cuando es sintética
    /// (generada automáticamente por el sistema desde una resolución de periodo).
    /// Formato: "Periodicidad-Anio-Numero" (ej. "Semanal-2026-31").
    /// Para ausencias manuales queda null.
    /// </summary>
    public string? PeriodoKey { get; set; }

    /// <summary>
    /// Origen de la ausencia. Manual = capturada por el operador; SinteticoPorPeriodo = generada
    /// automáticamente por un proceso (ej. permiso por diferencia neta en liquidación de periodo).
    /// </summary>
    public OrigenAusenciaRrhh OrigenAusencia { get; set; } = OrigenAusenciaRrhh.Manual;

    /// <summary>
    /// Determina si esta ausencia fue generada automáticamente por el sistema
    /// como parte de una resolución de periodo (permiso por diferencia, etc.).
    /// Se usa para que el helpers de apertura de periodo pueda revertir solo sintéticas.
    /// </summary>
    public bool EsSinteticaDePeriodo() =>
        OrigenAusencia == OrigenAusenciaRrhh.SinteticoPorPeriodo
        && !string.IsNullOrEmpty(PeriodoKey);

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
        TipoAusenciaRrhh.PermisoPorDiferenciaPeriodo => true, // default con goce; el operador decide la categoría final
        _ => false
    };

    /// <summary>
    /// Determina si este tipo puede configurarse el goce de pago (solo Permiso genérico)
    /// </summary>
    public bool PuedeConfigurarGocePago() => Tipo switch
    {
        TipoAusenciaRrhh.Permiso => true,
        _ => false
    };

    /// <summary>
    /// Determina si este tipo descuenta banco de horas por defecto.
    /// Solo los permisos donde el empleado usa su banco acumulado deben descontarlo.
    /// </summary>
    public static bool DebeDescuentarBancoPorDefecto(TipoAusenciaRrhh tipo) => tipo switch
    {
        TipoAusenciaRrhh.PermisoConGoce => true, // permiso explícitamente con goce = usa banco
        _ => false // vacaciones, capacitación, incapacidad, maternidad, etc. NO descuentan banco
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
    PermisoMaternidad = 11,
    /// <summary>
    /// Permiso sintético generado por el sistema al cierre del periodo cuando
    /// |retardoDetectado − extraDetectado| > 0. La categoría (banco / con goce / sin goce)
    /// la define el operador al autorizar y se persiste en DescuentaBancoHoras + ConGocePago.
    /// </summary>
    PermisoPorDiferenciaPeriodo = 12
}

public enum EstatusAusenciaRrhh
{
    Solicitada = 1,
    Aprobada = 2,
    Rechazada = 3,
    Aplicada = 4,
    Cancelada = 5
}

/// <summary>
/// Indica el origen de una ausencia. Manual = capturada por el operador.
/// SinteticoPorPeriodo = generada automáticamente por un proceso (ej. permiso por
/// diferencia neta en liquidación de periodo). Se usa para que las reaperturas de
/// periodo y los flujos de replicación distingan y no borren ausencias capturadas a mano.
/// </summary>
public enum OrigenAusenciaRrhh
{
    Manual = 1,
    SinteticoPorPeriodo = 2
}
