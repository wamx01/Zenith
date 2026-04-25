using MundoVs.Core.Entities;

namespace MundoVs.Core.Interfaces;

public interface IPedidoSeguimientoRepository : IRepository<PedidoSeguimiento>
{
    Task<IEnumerable<PedidoSeguimiento>> GetByPedidoAsync(Guid pedidoId);
}
