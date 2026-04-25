using Zenith.Contracts.Asistencia;

namespace Zenith.Workers.Asistencia.Abstractions;

public interface IMarcacionSyncClient
{
    Task<SyncResultDto> EnviarMarcacionesAsync(MarcacionSyncBatchDto batch, CancellationToken cancellationToken);
}