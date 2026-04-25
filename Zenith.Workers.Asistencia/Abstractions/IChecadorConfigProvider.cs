using Zenith.Contracts.Asistencia;

namespace Zenith.Workers.Asistencia.Abstractions;

public interface IChecadorConfigProvider
{
    Task<IReadOnlyCollection<ChecadorConfigDto>> ObtenerChecadoresActivosAsync(CancellationToken cancellationToken);
}