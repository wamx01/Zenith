namespace MundoVs.Core.Interfaces;

public interface INominaPdfService
{
    Task<byte[]> GenerateReciboPdfAsync(Guid nominaDetalleId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateRecibosPdfAsync(Guid nominaId, CancellationToken cancellationToken = default);
}
