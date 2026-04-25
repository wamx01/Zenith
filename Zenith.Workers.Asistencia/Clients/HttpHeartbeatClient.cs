using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Zenith.Contracts.Asistencia;
using Zenith.Workers.Asistencia.Abstractions;
using Zenith.Workers.Asistencia.Options;

namespace Zenith.Workers.Asistencia.Clients;

public sealed class HttpHeartbeatClient(
    HttpClient httpClient,
    IOptions<AsistenciaWorkerOptions> options,
    ILogger<HttpHeartbeatClient> logger) : IHeartbeatClient
{
    public async Task EnviarHeartbeatAsync(AgenteHeartbeatDto heartbeat, CancellationToken cancellationToken)
    {
        var workerOptions = options.Value;
        if (heartbeat.EmpresaId == Guid.Empty || string.IsNullOrWhiteSpace(workerOptions.ApiBaseUrl) || string.IsNullOrWhiteSpace(workerOptions.ApiHeartbeatPath))
        {
            return;
        }

        httpClient.BaseAddress ??= new Uri(workerOptions.ApiBaseUrl, UriKind.Absolute);
        httpClient.DefaultRequestHeaders.Remove("X-Zenith-Worker-Key");
        if (!string.IsNullOrWhiteSpace(workerOptions.ApiKey))
        {
            httpClient.DefaultRequestHeaders.Add("X-Zenith-Worker-Key", workerOptions.ApiKey);
        }

        using var response = await httpClient.PostAsJsonAsync(workerOptions.ApiHeartbeatPath, heartbeat, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("El ERP respondió {StatusCode} al registrar heartbeat del agente {NombreAgente}. Body: {Body}", (int)response.StatusCode, heartbeat.NombreAgente, body);
        }
    }
}
