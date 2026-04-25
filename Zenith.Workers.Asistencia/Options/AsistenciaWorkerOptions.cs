using Zenith.Contracts.Asistencia;

namespace Zenith.Workers.Asistencia.Options;

public sealed class AsistenciaWorkerOptions
{
    public const string SectionName = "AsistenciaWorker";

    public Guid EmpresaId { get; set; }
    public string NombreInstancia { get; set; } = "Zenith Asistencia Worker";
    public int IntervaloSegundos { get; set; } = 60;
    public bool HabilitarSync { get; set; } = true;
    public bool HabilitarPersistenciaLocal { get; set; } = true;
    public string? ApiBaseUrl { get; set; }
    public string ApiConfiguracionPath { get; set; } = "/api/rrhh/agentes/configuracion";
    public string ApiMarcacionesPath { get; set; } = "/api/rrhh/marcaciones/sync";
    public string ApiHeartbeatPath { get; set; } = "/api/rrhh/agentes/heartbeat";
    public string? ApiKey { get; set; }
    public string ZkTecoProgId { get; set; } = "zkemkeeper.CZKEM";
    public List<ChecadorConfigDto> Checadores { get; set; } = [];
}