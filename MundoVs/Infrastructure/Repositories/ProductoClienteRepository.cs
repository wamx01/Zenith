using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Infrastructure.Repositories;

public class ProductoClienteRepository : Repository<ProductoCliente>, IProductoClienteRepository
{
    public ProductoClienteRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductoCliente>> GetByClienteAsync(Guid clienteId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(pc => pc.Producto)
            .Where(pc => pc.ClienteId == clienteId && pc.IsActive)
            .OrderBy(pc => pc.Producto.Nombre)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid clienteId, Guid productoId)
    {
        return await _dbSet.AnyAsync(pc => pc.ClienteId == clienteId && pc.ProductoId == productoId);
    }
}
