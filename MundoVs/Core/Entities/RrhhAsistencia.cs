namespace MundoVs.Core.Entities;

public class RrhhAsistencia : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public Guid? TurnoBaseId { get; set; }
    public TurnoBase? TurnoBase { get; set; }

    public DateOnly Fecha { get; set; }
    public TimeSpan? HoraEntradaProgramada { get; set; }
    public TimeSpan? HoraSalidaProgramada { get; set; }
    public TimeSpan? HoraEntradaReal { get; set; }
    public TimeSpan? HoraSalidaReal { get; set; }
    public int TotalMarcaciones { get; set; }
    public int MinutosJornadaProgramada { get; set; }
    public int MinutosJornadaNetaProgramada { get; set; }
    public int MinutosTrabajadosBrutos { get; set; }
    public int MinutosTrabajadosNetos { get; set; }
    public int MinutosPerdonadosManual { get; set; }
    public int MinutosDescansoProgramado { get; set; }
    public int MinutosDescansoTomado { get; set; }
    public int MinutosDescansoPagado { get; set; }
    public int MinutosDescansoNoPagado { get; set; }
    public int MinutosRetardo { get; set; }
    public int MinutosSalidaAnticipada { get; set; }
    public int MinutosExtra { get; set; }
    public int MinutosExtraAutorizadosPago { get; set; }
    public int MinutosExtraAutorizadosBanco { get; set; }
    public int MinutosCubiertosBancoHoras { get; set; }
    public string? ResolucionTiempoExtra { get; set; }
    /// <summary>
    /// Factor aplicado al autorizar tiempo extra. Si es null o 0, se usa el factor de configuración.
    /// Se persiste para que la nómina y el reporte respeten el override manual.
    /// </summary>
    public decimal? FactorTiempoExtraAplicado { get; set; }
    /// <summary>
    /// Modo de cálculo de tiempo extra para este día:
    /// "EntradaSalida" = entrada/salida real vs programada (default),
    /// "NetoVsNeto" = neto trabajado vs neto esperado del turno.
    /// </summary>
    public string? ModoSugerenciaExtra { get; set; }
    public RrhhAsistenciaEstatus Estatus { get; set; } = RrhhAsistenciaEstatus.Pendiente;
    public bool RequiereRevision { get; set; }
    public string? Observaciones { get; set; }
    public string? ResumenDescansos { get; set; }
    public string? ObservacionPerdonManual { get; set; }
    /// <summary>
    /// Números de descanso (separados por coma) que el usuario marcó como no descontados.
    /// El empleado no tomó esos descansos y el tiempo se cuenta como trabajo efectivo.
    /// Ejemplo: "1" o "1,2"
    /// </summary>
    public string? DescansosNoDescontar { get; set; }
}

public enum RrhhAsistenciaEstatus
{
    Pendiente = 1,
    AsistenciaNormal = 2,
    Retardo = 3,
    Falta = 4,
    Descanso = 5,
    DescansoTrabajado = 6,
    Incompleta = 7,
    TurnoNoAsignado = 8,
    MarcaNoReconocida = 9
}