namespace MundoVs.Core.Interfaces;

public interface IModuloAccesoService
{
    Task<HashSet<string>> ObtenerModulosHabilitadosAsync(Guid empresaId, CancellationToken cancellationToken = default);
    Task<bool> TieneModuloAsync(Guid empresaId, string moduloClave, CancellationToken cancellationToken = default);
    Task<List<string>> FiltrarCapacidadesAsync(Guid empresaId, IEnumerable<string> capacidades, bool esSuperAdmin, CancellationToken cancellationToken = default);
    void InvalidarCacheEmpresa(Guid empresaId);
}
