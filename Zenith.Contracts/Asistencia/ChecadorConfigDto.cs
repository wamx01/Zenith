namespace Zenith.Contracts.Asistencia;

public sealed class ChecadorConfigDto
{
    public Guid Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? NumeroSerie { get; set; }
    public string? Ip { get; set; }
    public int Puerto { get; set; }
    public int NumeroMaquina { get; set; } = 1;
    public string? Ubicacion { get; set; }
    public string? ZonaHoraria { get; set; }
    public DateTime? UltimaSincronizacionUtc { get; set; }
    public string? UltimoEventoLeido { get; set; }
    public bool Activo { get; set; } = true;
}