using System.Net.Http.Json;
using Zenith.Contracts.Asistencia;
using Zenith.Workers.Asistencia.Abstractions;
using Zenith.Workers.Asistencia.Options;
using Microsoft.Extensions.Options;

namespace Zenith.Workers.Asistencia.Clients;

public sealed class HttpMarcacionSyncClient(
    HttpClient httpClient,
    IOptions<AsistenciaWorkerOptions> options,
    ILogger<HttpMarcacionSyncClient> logger) : IMarcacionSyncClient
{
    public async Task<SyncResultDto> EnviarMarcacionesAsync(MarcacionSyncBatchDto batch, CancellationToken cancellationToken)
    {
        var workerOptions = options.Value;
        if (string.IsNullOrWhiteSpace(workerOptions.ApiBaseUrl))
        {
            throw new InvalidOperationException("Configura `AsistenciaWorker:ApiBaseUrl` antes de sincronizar marcaciones.");
        }

        httpClient.BaseAddress ??= new Uri(workerOptions.ApiBaseUrl, UriKind.Absolute);
        httpClient.DefaultRequestHeaders.Remove("X-Zenith-Worker-Key");
        if (!string.IsNullOrWhiteSpace(workerOptions.ApiKey))
        {
            httpClient.DefaultRequestHeaders.Add("X-Zenith-Worker-Key", workerOptions.ApiKey);
        }

        using var response = await httpClient.PostAsJsonAsync(workerOptions.ApiMarcacionesPath, batch, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("El ERP respondió {StatusCode} al sincronizar el checador {ChecadorId}. Body: {Body}", (int)response.StatusCode, batch.ChecadorId, body);
            throw new InvalidOperationException($"Error al sincronizar marcaciones. Código HTTP: {(int)response.StatusCode}.");
        }

        var result = await response.Content.ReadFromJsonAsync<SyncResultDto>(cancellationToken: cancellationToken)
            ?? new SyncResultDto
            {
                ChecadorId = batch.ChecadorId,
                Leidas = batch.Marcaciones.Count,
                Enviadas = batch.Marcaciones.Count,
                Mensaje = "Sincronización realizada sin payload de respuesta."
            };

        return result;
    }
}
