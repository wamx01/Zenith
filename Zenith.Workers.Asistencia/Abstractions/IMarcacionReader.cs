using Zenith.Contracts.Asistencia;

namespace Zenith.Workers.Asistencia.Abstractions;

public interface IMarcacionReader
{
    Task<IReadOnlyCollection<MarcacionRawDto>> LeerMarcacionesAsync(ChecadorConfigDto checador, CancellationToken cancellationToken);
}