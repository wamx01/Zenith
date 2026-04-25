using Zenith.Contracts.Asistencia;
using Zenith.Workers.Asistencia.Abstractions;

namespace Zenith.Workers.Asistencia.Clients;

public sealed class LoggingMarcacionSyncClient(ILogger<LoggingMarcacionSyncClient> logger) : IMarcacionSyncClient
{
    public Task<SyncResultDto> EnviarMarcacionesAsync(MarcacionSyncBatchDto batch, CancellationToken cancellationToken)
    {
        logger.LogInformation("Preparando envío de {TotalMarcaciones} marcación(es) del checador {ChecadorId}.", batch.Marcaciones.Count, batch.ChecadorId);

        return Task.FromResult(new SyncResultDto
        {
            ChecadorId = batch.ChecadorId,
            Leidas = batch.Marcaciones.Count,
            Enviadas = batch.Marcaciones.Count,
            Mensaje = "Simulación de envío completada."
        });
    }
}