namespace MundoVs.Core.Models;

public sealed class RrhhMarcacionZonaHorariaCorrectionRequest
{
    public Guid EmpresaId { get; set; }
    public Guid ChecadorId { get; set; }
    public DateOnly FechaDesde { get; set; }
    public DateOnly FechaHasta { get; set; }
    public string? ZonaHoraria { get; set; }
    public bool ReprocesarAsistencias { get; set; } = true;
    public string? Motivo { get; set; }
}
