using Zenith.Contracts.Asistencia;
using Zenith.Workers.Asistencia.Abstractions;

namespace Zenith.Workers.Asistencia.Readers;

public sealed class StubMarcacionReader : IMarcacionReader
{
    public Task<IReadOnlyCollection<MarcacionRawDto>> LeerMarcacionesAsync(ChecadorConfigDto checador, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyCollection<MarcacionRawDto>>([]);
}