using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities.Serigrafia;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Infrastructure.Repositories;

public class PedidoSerigrafiaRepository : Repository<PedidoSerigrafia>, IPedidoSerigrafiaRepository
{
    public PedidoSerigrafiaRepository(CrmDbContext context) : base(context)
    {
    }

    private IQueryable<PedidoSerigrafia> BaseQuery()
    {
        return _dbSet
            .AsNoTracking()
            .Include(p => p.PedidoDetalle)
                .ThenInclude(pd => pd.Pedido)
                    .ThenInclude(ped => ped.Cliente)
            .Include(p => p.PedidoDetalle)
                .ThenInclude(pd => pd.DetallesTalla)
                    .ThenInclude(dt => dt.ProductoVariante)
            .Include(p => p.PedidoDetalle)
                .ThenInclude(pd => pd.CotizacionSerigrafia)
            .Include(p => p.TiposProceso)
                .ThenInclude(tp => tp.TipoProceso)
            .Include(p => p.Tallas);
    }

    public async Task<IEnumerable<PedidoSerigrafia>> GetByClienteAsync(Guid clienteId)
    {
        return await BaseQuery()
            .Where(p => p.PedidoDetalle.Pedido.ClienteId == clienteId)
            .OrderByDescending(p => p.FechaRecibido)
            .ToListAsync();
    }

    public async Task<IEnumerable<PedidoSerigrafia>> GetPendientesAsync()
    {
        return await BaseQuery()
            .Where(p => !p.Hecho && p.IsActive)
            .OrderBy(p => p.FechaEstimada)
            .ToListAsync();
    }

    public async Task<IEnumerable<PedidoSerigrafia>> GetCompletadosAsync()
    {
        return await BaseQuery()
            .Where(p => p.Hecho && p.IsActive)
            .OrderByDescending(p => p.FechaEntregaReal)
            .ToListAsync();
    }

    public async Task<IEnumerable<PedidoSerigrafia>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await BaseQuery()
            .Where(p => p.FechaRecibido >= fechaInicio && p.FechaRecibido <= fechaFin)
            .OrderBy(p => p.FechaRecibido)
            .ToListAsync();
    }

    public async Task<PedidoSerigrafia?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.PedidoDetalle)
                .ThenInclude(pd => pd.Pedido)
                    .ThenInclude(ped => ped.Cliente)
            .Include(p => p.PedidoDetalle)
                .ThenInclude(pd => pd.DetallesTalla)
                    .ThenInclude(dt => dt.ProductoVariante)
            .Include(p => p.PedidoDetalle)
                .ThenInclude(pd => pd.Producto)
            .Include(p => p.PedidoDetalle)
                .ThenInclude(pd => pd.CotizacionSerigrafia)
            .Include(p => p.TiposProceso)
                .ThenInclude(tp => tp.TipoProceso)
            .Include(p => p.Tallas)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public override async Task<IEnumerable<PedidoSerigrafia>> GetAllAsync()
    {
        return await BaseQuery()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.FechaRecibido)
            .ToListAsync();
    }
}
