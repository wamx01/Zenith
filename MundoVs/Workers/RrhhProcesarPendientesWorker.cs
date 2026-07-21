using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Workers;

/// <summary>
/// Worker en segundo plano que drena la cola de marcaciones pendientes
/// clasificándolas una a una en orden FIFO.
///
/// Se ejecuta como <see cref="BackgroundService"/> hosted del proyecto Blazor
/// (MundoVs) y mantiene un único consumidor para no saturar la base de datos
/// ni generar concurrencia sobre las mismas marcaciones.
/// </summary>
public sealed class RrhhProcesarPendientesWorker(
    RrhhMarcacionesPendientesQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<RrhhProcesarPendientesWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker de marcaciones pendientes iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            RrhhColaPendientesItem item;
            try
            {
                item = await queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<CrmDbContext>>();
                var rrhhAsistenciaProcessor = scope.ServiceProvider.GetRequiredService<IRrhhAsistenciaProcessor>();
                await using var db = await dbContextFactory.CreateDbContextAsync(stoppingToken);
                await rrhhAsistenciaProcessor.ProcesarMarcacionesPendientesAsync(
                    db,
                    item.EmpresaId,
                    item.ChecadorId,
                    stoppingToken);

                stopwatch.Stop();
                logger.LogInformation(
                    "Marcaciones pendientes procesadas para checador {ChecadorId} en {ElapsedMs} ms (cola restante: {QueueDepth}).",
                    item.ChecadorId,
                    stopwatch.ElapsedMilliseconds,
                    queue.Count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                // Loggeo y continúo: el siguiente item de la cola no debe
                // bloquearse por un fallo aislado. Si el ítem sigue pendiente
                // en BD, el agente lo re-enviará como duplicada y, gracias al
                // re-marcado en IngerirLoteAsync, será reprocesado.
                logger.LogError(
                    ex,
                    "Falló el procesamiento de marcaciones pendientes para checador {ChecadorId} tras {ElapsedMs} ms.",
                    item.ChecadorId,
                    stopwatch.ElapsedMilliseconds);
            }
        }

        logger.LogInformation("Worker de marcaciones pendientes detenido.");
    }
}
