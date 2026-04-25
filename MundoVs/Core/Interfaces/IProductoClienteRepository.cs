using MundoVs.Core.Entities;

namespace MundoVs.Core.Interfaces;

public interface IProductoClienteRepository : IRepository<ProductoCliente>
{
    Task<IEnumerable<ProductoCliente>> GetByClienteAsync(Guid clienteId);
    Task<bool> ExistsAsync(Guid clienteId, Guid productoId);
}
