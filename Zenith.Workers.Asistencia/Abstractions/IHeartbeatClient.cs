using Zenith.Contracts.Asistencia;

namespace Zenith.Workers.Asistencia.Abstractions;

public interface IHeartbeatClient
{
    Task EnviarHeartbeatAsync(AgenteHeartbeatDto heartbeat, CancellationToken cancellationToken);
}
