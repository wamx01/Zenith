using Microsoft.Extensions.Options;
using Zenith.Workers.Asistencia.Abstractions;
using Zenith.Workers.Asistencia.Models;
using Zenith.Workers.Asistencia.Options;

namespace Zenith.Workers.Asistencia;

public sealed class Worker(
    ILogger<Worker> logger,
    IOptions<AsistenciaWorkerOptions> options,
    IAsistenciaSyncService asistenciaSyncService,
    IHeartbeatClient heartbeatClient) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workerOptions = options.Value;
        var intervalo = TimeSpan.FromSeconds(Math.Max(5, workerOptions.IntervaloSegundos));
        var version = typeof(Worker).Assembly.GetName().Version?.ToString();

        logger.LogInformation("{NombreInstancia} iniciado. Intervalo: {IntervaloSegundos}s.", workerOptions.NombreInstancia, workerOptions.IntervaloSegundos);

        while (!stoppingToken.IsCancellationRequested)
        {
            AsistenciaSyncCycleResult ciclo;

            if (workerOptions.HabilitarSync)
            {
                ciclo = await asistenciaSyncService.EjecutarCicloAsync(stoppingToken);
            }
            else
            {
                ciclo = new AsistenciaSyncCycleResult
                {
                    EjecutadoEnUtc = DateTime.UtcNow
                };

                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("La sincronización está deshabilitada por configuración.");
                }
            }

            await heartbeatClient.EnviarHeartbeatAsync(new Zenith.Contracts.Asistencia.AgenteHeartbeatDto
            {
                EmpresaId = workerOptions.EmpresaId,
                NombreAgente = workerOptions.NombreInstancia,
                Hostname = Environment.MachineName,
                Version = version,
                UltimaEjecucionUtc = ciclo.EjecutadoEnUtc,
                MarcacionesLeidas = ciclo.MarcacionesLeidas,
                MarcacionesEnviadas = ciclo.MarcacionesEnviadas,
                ErroresRecientes = ciclo.ErroresRecientes
            }, stoppingToken);

            await Task.Delay(intervalo, stoppingToken);
        }
    }
}
