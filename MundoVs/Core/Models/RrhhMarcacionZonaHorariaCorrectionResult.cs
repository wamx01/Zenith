namespace MundoVs.Core.Models;

public sealed class RrhhMarcacionZonaHorariaCorrectionResult
{
    public Guid EmpresaId { get; set; }
    public Guid ChecadorId { get; set; }
    public DateOnly FechaDesde { get; set; }
    public DateOnly FechaHasta { get; set; }
    public string ZonaHorariaAplicada { get; set; } = string.Empty;
    public int MarcacionesEncontradas { get; set; }
    public int MarcacionesCorregidas { get; set; }
}
