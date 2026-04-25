namespace MundoVs.Core.Entities;

public class RrhhEstadoAgente : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string NombreAgente { get; set; } = string.Empty;
    public string? Hostname { get; set; }
    public string? Version { get; set; }
    public DateTime? UltimoHeartbeatUtc { get; set; }
    public DateTime? UltimaEjecucionUtc { get; set; }
    public int MarcacionesLeidas { get; set; }
    public int MarcacionesEnviadas { get; set; }
    public string? UltimoError { get; set; }
    public string? UltimoLogNivel { get; set; }
    public string? UltimoLogMensaje { get; set; }
    public string? UltimoLogDetalle { get; set; }
    public DateTime? UltimoLogUtc { get; set; }
}
