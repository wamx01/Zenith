namespace MundoVs.Core.Models;

/// <summary>
/// Un renglón del reporte de tiempo extra para un día específico de un empleado.
/// </summary>
public sealed class RrhhTiempoExtraReporteDiaDto
{
    public DateOnly Fecha { get; set; }
    public string Estatus { get; set; } = string.Empty;
    public int MinutosExtra { get; set; }
    public int MinutosExtraAutorizadosPago { get; set; }
    public int MinutosExtraAutorizadosBanco { get; set; }
    public int MinutosCubiertosBancoHoras { get; set; }
    public decimal FactorTiempoExtra { get; set; }
    public int MinutosPagoFactorado { get; set; }
    public int MinutosBancoFactorado { get; set; }
    public int MinutosRetardo { get; set; }
    public int MinutosSalidaAnticipada { get; set; }
    public int MinutosTrabajadosBrutos { get; set; }
    public int MinutosTrabajadosNetos { get; set; }
    public bool RequiereRevision { get; set; }
    public string? ResolucionTiempoExtra { get; set; }
    public string? Turno { get; set; }
    public TimeSpan? HoraEntradaProgramada { get; set; }
    public TimeSpan? HoraSalidaProgramada { get; set; }
    public TimeSpan? HoraEntradaReal { get; set; }
    public TimeSpan? HoraSalidaReal { get; set; }
    public decimal HorasExtraDobles { get; set; }
    public decimal HorasExtraTriples { get; set; }
    public decimal SueldoHora { get; set; }
    public decimal MontoHorasExtraEstimado { get; set; }
    public string? Observaciones { get; set; }
}

/// <summary>
/// Bloque de totales agregados por empleado (para agrupadoPor="empleado" o al pie de la respuesta).
/// </summary>
public sealed class RrhhTiempoExtraReporteTotalesDto
{
    public int TotalDias { get; set; }
    public int DiasConExtra { get; set; }
    public int TotalMinutosExtra { get; set; }
    public int TotalMinutosExtraAutorizadosPago { get; set; }
    public int TotalMinutosExtraAutorizadosBanco { get; set; }
    public int TotalMinutosCubiertosBancoHoras { get; set; }
    public int TotalMinutosPagoFactorado { get; set; }
    public int TotalMinutosBancoFactorado { get; set; }
    public int TotalMinutosRetardo { get; set; }
    public int TotalMinutosSalidaAnticipada { get; set; }
    public int TotalMinutosTrabajadosNetos { get; set; }
    public decimal TotalHorasExtraDobles { get; set; }
    public decimal TotalHorasExtraTriples { get; set; }
    public decimal TotalMontoHorasExtraEstimado { get; set; }

    public string TotalHorasExtraFormateado => FormatearHorasMinutos(TotalMinutosExtra);
    public string TotalHorasExtraPagoFormateado => FormatearHorasMinutos(TotalMinutosExtraAutorizadosPago);
    public string TotalHorasExtraBancoFormateado => FormatearHorasMinutos(TotalMinutosExtraAutorizadosBanco);
    public string TotalBancoAplicadoFormateado => FormatearHorasMinutos(TotalMinutosCubiertosBancoHoras);
    public string TotalPagoFactoradoFormateado => FormatearHorasMinutos(TotalMinutosPagoFactorado);
    public string TotalBancoFactoradoFormateado => FormatearHorasMinutos(TotalMinutosBancoFactorado);

    public static string FormatearHorasMinutos(int minutos)
    {
        if (minutos <= 0)
        {
            return "0h 0m";
        }

        var horas = minutos / 60;
        var mins = minutos % 60;
        return $"{horas}h {mins}m";
    }
}

/// <summary>
/// Renglón de empleado dentro del reporte de tiempo extra.
/// </summary>
public sealed class RrhhTiempoExtraReporteEmpleadoDto
{
    public Guid EmpleadoId { get; set; }
    public string NumeroEmpleado { get; set; } = string.Empty;
    public string? CodigoChecador { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Departamento { get; set; }
    public string? Puesto { get; set; }
    public decimal SueldoSemanal { get; set; }
    public string PeriodicidadPago { get; set; } = string.Empty;
    public IReadOnlyList<RrhhTiempoExtraReporteDiaDto> Dias { get; set; } = [];
    public RrhhTiempoExtraReporteTotalesDto Totales { get; set; } = new();
}

/// <summary>
/// Respuesta del endpoint de reporte de tiempo extra.
/// </summary>
public sealed class RrhhTiempoExtraReporteResponse
{
    public Guid EmpresaId { get; set; }
    public DateOnly FechaDesde { get; set; }
    public DateOnly FechaHasta { get; set; }
    public string AgrupadoPor { get; set; } = "dia";
    public int TotalEmpleados { get; set; }
    public int TotalDias { get; set; }
    public RrhhTiempoExtraReporteTotalesDto Totales { get; set; } = new();
    public IReadOnlyList<RrhhTiempoExtraReporteEmpleadoDto> Empleados { get; set; } = [];
}
