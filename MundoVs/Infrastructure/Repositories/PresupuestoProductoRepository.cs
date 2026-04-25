using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities.Serigrafia;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Infrastructure.Repositories;

public class PresupuestoProductoRepository : Repository<PresupuestoProducto>, IPresupuestoProductoRepository
{
    public PresupuestoProductoRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PresupuestoProducto>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.Producto)
            .Include(p => p.Detalles.Where(d => d.IsActive).OrderBy(d => d.Categoria).ThenBy(d => d.Orden))
            .Where(p => p.IsActive)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<PresupuestoProducto?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Producto)
            .Include(p => p.Detalles.Where(d => d.IsActive).OrderBy(d => d.Categoria).ThenBy(d => d.Orden))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PresupuestoProducto>> GetByProductoAsync(Guid productoId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.Producto)
            .Include(p => p.Detalles.Where(d => d.IsActive).OrderBy(d => d.Categoria).ThenBy(d => d.Orden))
            .Where(p => p.ProductoId == productoId && p.IsActive)
            .ToListAsync();
    }
}
