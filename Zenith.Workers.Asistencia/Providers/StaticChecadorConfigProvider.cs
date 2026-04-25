using Zenith.Contracts.Asistencia;
using Zenith.Workers.Asistencia.Abstractions;
using Zenith.Workers.Asistencia.Options;
using Microsoft.Extensions.Options;

namespace Zenith.Workers.Asistencia.Providers;

public sealed class StaticChecadorConfigProvider(IOptions<AsistenciaWorkerOptions> options) : IChecadorConfigProvider
{
    public Task<IReadOnlyCollection<ChecadorConfigDto>> ObtenerChecadoresActivosAsync(CancellationToken cancellationToken)
    {
        var checadores = options.Value.Checadores
            .Where(c => c.Activo)
            .ToList();

        return Task.FromResult<IReadOnlyCollection<ChecadorConfigDto>>(checadores);
    }
}