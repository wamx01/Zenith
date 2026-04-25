using Zenith.Contracts.Asistencia;
using Zenith.Workers.Asistencia.Abstractions;
using Zenith.Workers.Asistencia.Models;

namespace Zenith.Workers.Asistencia.Services;

public sealed class AsistenciaSyncService(
    IChecadorConfigProvider checadorConfigProvider,
    IMarcacionReader marcacionReader,
    IMarcacionSyncClient marcacionSyncClient,
    ILogger<AsistenciaSyncService> logger) : IAsistenciaSyncService
{
    public async Task<AsistenciaSyncCycleResult> EjecutarCicloAsync(CancellationToken cancellationToken)
    {
        var result = new AsistenciaSyncCycleResult
        {
            EjecutadoEnUtc = DateTime.UtcNow
        };

        var checadores = await checadorConfigProvider.ObtenerChecadoresActivosAsync(cancellationToken);
        if (checadores.Count == 0)
        {
            logger.LogInformation("No hay checadores activos configurados para sincronizar.");
            return result;
        }

        var errores = new List<string>();

        foreach (var checador in checadores)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var marcaciones = await marcacionReader.LeerMarcacionesAsync(checador, cancellationToken);
                result.MarcacionesLeidas += marcaciones.Count;
                if (marcaciones.Count == 0)
                {
                    logger.LogInformation("El checador {ChecadorNombre} no devolvió marcaciones nuevas.", checador.Nombre);
                    continue;
                }

                var resultado = await marcacionSyncClient.EnviarMarcacionesAsync(new MarcacionSyncBatchDto
                {
                    EmpresaId = checador.EmpresaId,
                    ChecadorId = checador.Id,
                    NumeroSerieChecador = checador.NumeroSerie,
                    Marcaciones = marcaciones
                }, cancellationToken);

                logger.LogInformation(
                    "Sincronización completada para {ChecadorNombre}. Leídas: {Leidas}. Enviadas: {Enviadas}. Fallidas: {Fallidas}.",
                    checador.Nombre,
                    resultado.Leidas,
                    resultado.Enviadas,
                    resultado.Fallidas);

                result.MarcacionesEnviadas += resultado.Enviadas;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falló la sincronización del checador {ChecadorNombre}.", checador.Nombre);
                errores.Add($"{checador.Nombre}: {ex.Message}");
            }
        }

        result.ErroresRecientes = errores.Count == 0
            ? null
            : string.Join(" | ", errores.Take(3));

        return result;
    }
}