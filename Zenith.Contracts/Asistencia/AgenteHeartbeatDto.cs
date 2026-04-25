namespace Zenith.Contracts.Asistencia;

public sealed class AgenteHeartbeatDto
{
    public Guid EmpresaId { get; set; }
    public string NombreAgente { get; set; } = string.Empty;
    public string? Hostname { get; set; }
    public string? Version { get; set; }
    public DateTime UltimaEjecucionUtc { get; set; } = DateTime.UtcNow;
    public int MarcacionesLeidas { get; set; }
    public int MarcacionesEnviadas { get; set; }
    public string? ErroresRecientes { get; set; }
}

public sealed class AgenteLogDto
{
    public Guid EmpresaId { get; set; }
    public string NombreAgente { get; set; } = string.Empty;
    public string? Hostname { get; set; }
    public Guid? ChecadorId { get; set; }
    public string Nivel { get; set; } = "Information";
    public string Mensaje { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
}
