namespace Zenith.Contracts.Asistencia;

public sealed class SyncResultDto
{
    public Guid ChecadorId { get; set; }
    public int Leidas { get; set; }
    public int Enviadas { get; set; }
    public int Duplicadas { get; set; }
    public int Fallidas { get; set; }
    public DateTime EjecutadoEnUtc { get; set; } = DateTime.UtcNow;
    public string? Mensaje { get; set; }
}