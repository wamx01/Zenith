using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Infrastructure.Repositories;

public class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<Cliente?> GetByCodigoAsync(string codigo)
    {
        return await _dbSet
            .Include(c => c.Contactos)
            .FirstOrDefaultAsync(c => c.Codigo == codigo);
    }

    public async Task<IEnumerable<Cliente>> GetByIndustriaAsync(IndustriaEnum industria)
    {
        return await _dbSet
            .Where(c => c.Industria == industria || c.Industria == IndustriaEnum.Ambas)
            .Include(c => c.Contactos)
            .ToListAsync();
    }

    public async Task<bool> ExistsByCodigoAsync(string codigo)
    {
        return await _dbSet.AnyAsync(c => c.Codigo == codigo);
    }

    public override async Task<Cliente?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Contactos)
            .Include(c => c.Pedidos)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public override async Task<IEnumerable<Cliente>> GetAllAsync()
    {
        return await _dbSet
            .Include(c => c.Contactos)
            .Where(c => c.IsActive)
            .OrderBy(c => c.RazonSocial)
            .ToListAsync();
    }
}
