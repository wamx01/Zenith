namespace MundoVs.Core.Interfaces;

public interface INotaEntregaPdfService
{
    Task<string> GenerateAndStoreAsync(Guid notaEntregaId, CancellationToken cancellationToken = default);
}
