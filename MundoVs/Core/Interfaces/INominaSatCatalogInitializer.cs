namespace MundoVs.Core.Interfaces;

public interface INominaSatCatalogInitializer
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
