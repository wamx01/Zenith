using MundoVs.Core.Entities;

namespace MundoVs.Core.Interfaces;

public interface IPedidoRepository : IRepository<Pedido>
{
    Task<Pedido?> GetByNumeroAsync(string numeroPedido);
    Task<IEnumerable<Pedido>> GetByClienteAsync(Guid clienteId);
    Task<IEnumerable<Pedido>> GetByEstadoAsync(EstadoPedidoEnum estado);
    Task<IEnumerable<Pedido>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin);
}
