using MundoVs.Core.Entities.Serigrafia;

namespace MundoVs.Core.Interfaces;

public interface IPedidoSerigrafiaRepository : IRepository<PedidoSerigrafia>
{
    Task<IEnumerable<PedidoSerigrafia>> GetByClienteAsync(Guid clienteId);
    Task<IEnumerable<PedidoSerigrafia>> GetPendientesAsync();
    Task<IEnumerable<PedidoSerigrafia>> GetCompletadosAsync();
    Task<IEnumerable<PedidoSerigrafia>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<PedidoSerigrafia?> GetWithDetailsAsync(Guid id);
}
