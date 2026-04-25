using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Infrastructure.Repositories;

public class PedidoSeguimientoRepository : Repository<PedidoSeguimiento>, IPedidoSeguimientoRepository
{
    public PedidoSeguimientoRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PedidoSeguimiento>> GetByPedidoAsync(Guid pedidoId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.PedidoId == pedidoId && s.IsActive)
            .OrderByDescending(s => s.Fecha)
            .ToListAsync();
    }
}
