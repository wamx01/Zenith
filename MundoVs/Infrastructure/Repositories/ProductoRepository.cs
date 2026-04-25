using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Infrastructure.Repositories;

public class ProductoRepository : Repository<Producto>, IProductoRepository
{
    public ProductoRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<Producto?> GetByCodigoAsync(string codigo)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Codigo == codigo);
    }

    public async Task<IEnumerable<Producto>> GetByIndustriaAsync(IndustriaEnum industria)
    {
        return await _dbSet
            .Where(p => p.Industria == industria && p.IsActive)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public override async Task<IEnumerable<Producto>> GetAllAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }
}
