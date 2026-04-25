namespace MundoVs.Core.Models;

public sealed class RrhhAsistenciaReprocesoRequest
{
    public Guid EmpresaId { get; set; }
    public DateOnly FechaDesde { get; set; }
    public DateOnly FechaHasta { get; set; }
    public Guid? EmpleadoId { get; set; }
    public string? Motivo { get; set; }
}
