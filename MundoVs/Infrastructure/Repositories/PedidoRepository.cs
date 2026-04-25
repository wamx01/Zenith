using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Infrastructure.Repositories;

public class PedidoRepository : Repository<Pedido>, IPedidoRepository
{
    public PedidoRepository(CrmDbContext context) : base(context)
    {
    }

    public async Task<Pedido?> GetByNumeroAsync(string numeroPedido)
    {
        return await _dbSet
            .Include(p => p.Cliente)
            .Include(p => p.Conceptos)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.DetallesTalla)
            .FirstOrDefaultAsync(p => p.NumeroPedido == numeroPedido);
    }

    public async Task<IEnumerable<Pedido>> GetByClienteAsync(Guid clienteId)
    {
        return await _dbSet
            .Include(p => p.Cliente)
            .Include(p => p.Conceptos)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.DetallesTalla)
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pedido>> GetByEstadoAsync(EstadoPedidoEnum estado)
    {
        return await _dbSet
            .Include(p => p.Cliente)
            .Include(p => p.Conceptos)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.DetallesTalla)
            .Where(p => p.Estado == estado)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pedido>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await _dbSet
            .Include(p => p.Cliente)
            .Include(p => p.Conceptos)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.DetallesTalla)
            .Where(p => p.FechaPedido >= fechaInicio && p.FechaPedido <= fechaFin)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync();
    }

    public override async Task<Pedido?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Cliente)
            .Include(p => p.Conceptos)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.DetallesTalla)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
