using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Zenith.Contracts.Asistencia;
using Zenith.Workers.Asistencia.Abstractions;
using Zenith.Workers.Asistencia.Options;

namespace Zenith.Workers.Asistencia.Providers;

public sealed class RemoteChecadorConfigProvider(
    HttpClient httpClient,
    IOptions<AsistenciaWorkerOptions> options,
    ILogger<RemoteChecadorConfigProvider> logger) : IChecadorConfigProvider
{
    public async Task<IReadOnlyCollection<ChecadorConfigDto>> ObtenerChecadoresActivosAsync(CancellationToken cancellationToken)
    {
        var workerOptions = options.Value;
        var fallback = workerOptions.Checadores
            .Where(c => c.Activo)
            .ToList();

        if (string.IsNullOrWhiteSpace(workerOptions.ApiBaseUrl) || workerOptions.EmpresaId == Guid.Empty)
        {
            return fallback;
        }

        try
        {
            httpClient.BaseAddress ??= new Uri(workerOptions.ApiBaseUrl, UriKind.Absolute);
            httpClient.DefaultRequestHeaders.Remove("X-Zenith-Worker-Key");
            if (!string.IsNullOrWhiteSpace(workerOptions.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("X-Zenith-Worker-Key", workerOptions.ApiKey);
            }

            var nombreAgente = Uri.EscapeDataString(workerOptions.NombreInstancia);
            var requestUri = $"{workerOptions.ApiConfiguracionPath}?empresaId={workerOptions.EmpresaId:D}&nombreAgente={nombreAgente}";
            using var response = await httpClient.GetAsync(requestUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("No fue posible obtener configuración remota. Status: {StatusCode}. Body: {Body}", (int)response.StatusCode, body);
                return fallback;
            }

            var configuracion = await response.Content.ReadFromJsonAsync<AgenteConfiguracionDto>(cancellationToken: cancellationToken);
            if (configuracion == null)
            {
                return fallback;
            }

            return configuracion.Checadores
                .Where(c => c.Activo)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falló la obtención remota de checadores. Se usará configuración local de respaldo.");
            return fallback;
        }
    }
}
