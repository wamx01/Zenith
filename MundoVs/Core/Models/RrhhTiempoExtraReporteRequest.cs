namespace MundoVs.Core.Models;

/// <summary>
/// Filtros para el reporte de tiempo extra por persona/fecha.
/// </summary>
public sealed class RrhhTiempoExtraReporteRequest
{
    public Guid EmpresaId { get; set; }
    public DateOnly FechaDesde { get; set; }
    public DateOnly FechaHasta { get; set; }
    public Guid? EmpleadoId { get; set; }

    /// <summary>
    /// Departamento (string) opcional para acotar el reporte. Vacío = todos los departamentos.
    /// </summary>
    public string? Departamento { get; set; }

    /// <summary>
    /// Si se establece, sólo se devuelven empleados que tengan al menos un día con
    /// MinutosExtra sugeridos estrictamente mayor al valor indicado.
    /// </summary>
    public int? MinutosExtraMinimos { get; set; }

    /// <summary>
    /// "dia" (default) => el reporte trae un renglón por empleado-fecha.
    /// "empleado" => agrupa todos los días bajo un solo renglón por empleado con totales.
    /// </summary>
    public string AgrupadoPor { get; set; } = "dia";
}
