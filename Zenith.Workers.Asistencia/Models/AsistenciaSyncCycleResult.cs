namespace Zenith.Workers.Asistencia.Models;

public sealed class AsistenciaSyncCycleResult
{
    public DateTime EjecutadoEnUtc { get; set; } = DateTime.UtcNow;
    public int MarcacionesLeidas { get; set; }
    public int MarcacionesEnviadas { get; set; }
    public string? ErroresRecientes { get; set; }
}
