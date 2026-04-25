namespace MundoVs.Core.Entities;

public class RrhhChecador : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public string? NumeroSerie { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public string? Ip { get; set; }
    public int Puerto { get; set; } = 4370;
    public int NumeroMaquina { get; set; } = 1;
    public string? Ubicacion { get; set; }
    public string? ZonaHoraria { get; set; }
    public DateTime? UltimaSincronizacionUtc { get; set; }
    public string? UltimoEventoLeido { get; set; }

    public ICollection<RrhhMarcacion> Marcaciones { get; set; } = [];
    public ICollection<RrhhLogChecador> Logs { get; set; } = [];
}