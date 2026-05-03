using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Entities.Inventario;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public sealed class NotaEntregaConsolidadaService : INotaEntregaConsolidadaService
{
    private readonly IDbContextFactory<CrmDbContext> _dbFactory;

    public NotaEntregaConsolidadaService(IDbContextFactory<CrmDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<NotaEntregaPedidoElegibleVm>> GetPedidosElegiblesAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var pedidos = await db.Pedidos
            .AsNoTracking()
            .Where(p => p.IsActive && p.ClienteId == clienteId && p.Estado != EstadoPedidoEnum.Cancelado)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.DetallesTalla)
            .Include(p => p.Conceptos)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync(cancellationToken);

        var pedidoIds = pedidos.Select(p => p.Id).ToList();

        var notasActivas = await db.NotasEntregaPedidos
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Include(r => r.NotaEntrega)
                .ThenInclude(n => n.Detalles)
                    .ThenInclude(d => d.Tallas)
            .Where(r => pedidoIds.Contains(r.PedidoId)
                && r.IsActive
                && r.NotaEntrega.IsActive
                && r.NotaEntrega.Estatus != NotaEntregaEstatus.Cancelada)
            .ToListAsync(cancellationToken);

        var entregadoPorPedido = notasActivas
            .GroupBy(r => r.PedidoId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.NotaEntrega.Detalles.SelectMany(d => d.Tallas).Sum(t => t.Cantidad)));

        return pedidos.Select(pedido =>
        {
            var cantidadPedida = pedido.Detalles.SelectMany(d => d.DetallesTalla).Sum(t => t.Cantidad);
            if (cantidadPedida <= 0)
                cantidadPedida = pedido.Detalles.Sum(d => d.Cantidad);

            var serviciosPendientes = pedido.Conceptos.Count(c => c.IsActive && c.Completado && !notasActivas.Any(n => n.PedidoId == pedido.Id && n.NotaEntrega.Detalles.Any(d => d.PedidoConceptoId == c.Id)));
            var entregado = entregadoPorPedido.GetValueOrDefault(pedido.Id, 0m);
            var pendienteProducto = Math.Max(0, cantidadPedida - entregado);
            var tieneNotas = notasActivas.Any(n => n.PedidoId == pedido.Id);
            var elegible = pendienteProducto > 0 || serviciosPendientes > 0;

            return new NotaEntregaPedidoElegibleVm
            {
                PedidoId = pedido.Id,
                NumeroPedido = pedido.NumeroPedido,
                FechaPedido = pedido.FechaPedido,
                TotalPedido = pedido.Total,
                CantidadProductoPedida = cantidadPedida,
                CantidadProductoEntregada = entregado,
                CantidadServiciosPendientes = serviciosPendientes,
                TieneNotasActivas = tieneNotas,
                Elegible = elegible,
                MotivoBloqueo = elegible ? string.Empty : "Sin saldo entregable real"
            };
        }).ToList();
    }

    public async Task<IReadOnlyList<NotaEntregaLineaConsolidadaVm>> BuildLineasConsolidadasAsync(IReadOnlyCollection<Guid> pedidoIds, CancellationToken cancellationToken = default)
    {
        if (pedidoIds.Count == 0)
            return [];

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var pedidos = await db.Pedidos
            .AsNoTracking()
            .Where(p => pedidoIds.Contains(p.Id) && p.IsActive)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.DetallesTalla)
                    .ThenInclude(t => t.ProductoVariante)
            .Include(p => p.Conceptos)
            .OrderByDescending(p => p.FechaPedido)
            .ToListAsync(cancellationToken);

        var clienteId = pedidos.Select(p => p.ClienteId).FirstOrDefault();
        var varianteIds = pedidos
            .SelectMany(p => p.Detalles)
            .SelectMany(d => d.DetallesTalla)
            .Where(t => t.ProductoVarianteId.HasValue)
            .Select(t => t.ProductoVarianteId!.Value)
            .Distinct()
            .ToList();

        var inventariosFg = varianteIds.Count == 0
            ? []
            : await db.InventariosFinishedGoods
                .AsNoTracking()
                .Where(i => i.ClienteId == clienteId && varianteIds.Contains(i.ProductoVarianteId) && i.IsActive)
                .ToListAsync(cancellationToken);

        var notasActivas = await db.NotasEntregaAsignaciones
            .AsNoTracking()
            .Where(a => pedidoIds.Contains(a.PedidoId)
                && a.IsActive
                && a.NotaEntrega.IsActive
                && a.NotaEntrega.Estatus != NotaEntregaEstatus.Cancelada)
            .ToListAsync(cancellationToken);

        var resultado = new List<NotaEntregaLineaConsolidadaVm>();

        foreach (var pedido in pedidos)
        {
            foreach (var detalle in pedido.Detalles)
            {
                foreach (var talla in detalle.DetallesTalla.Where(t => t.Cantidad > 0).OrderBy(t => t.Orden).ThenBy(t => t.Talla))
                {
                    var entregado = notasActivas
                        .Where(a => a.PedidoDetalleTallaId == talla.Id)
                        .Sum(a => a.Cantidad);
                    var pendiente = Math.Max(0, talla.Cantidad - entregado);
                    if (pendiente <= 0)
                        continue;

                    resultado.Add(new NotaEntregaLineaConsolidadaVm
                    {
                        PedidoId = pedido.Id,
                        NumeroPedido = pedido.NumeroPedido,
                        PedidoDetalleId = detalle.Id,
                        PedidoDetalleTallaId = talla.Id,
                        TipoOrigen = NotaEntregaAsignacionOrigenTipo.Producto,
                        Descripcion = $"{detalle.Producto.Nombre} · Pedido {pedido.NumeroPedido}",
                        Talla = talla.Talla,
                        OrdenTalla = talla.Orden,
                        CantidadMaximaTomable = pendiente,
                        CantidadSeleccionada = pendiente,
                        CantidadDisponibleFg = talla.ProductoVarianteId.HasValue
                            ? inventariosFg.FirstOrDefault(i => i.ProductoVarianteId == talla.ProductoVarianteId.Value)?.CantidadDisponible ?? 0m
                            : 0m,
                        PrecioUnitario = talla.PrecioUnitario > 0 ? talla.PrecioUnitario : detalle.PrecioUnitario
                    });
                }
            }

            foreach (var concepto in pedido.Conceptos.Where(c => c.IsActive && c.Completado).OrderBy(c => c.CreatedAt))
            {
                var entregado = notasActivas
                    .Where(a => a.PedidoConceptoId == concepto.Id)
                    .Sum(a => a.Cantidad);
                var pendiente = Math.Max(0, concepto.Cantidad - entregado);
                if (pendiente <= 0)
                    continue;

                resultado.Add(new NotaEntregaLineaConsolidadaVm
                {
                    PedidoId = pedido.Id,
                    NumeroPedido = pedido.NumeroPedido,
                    PedidoConceptoId = concepto.Id,
                    TipoOrigen = NotaEntregaAsignacionOrigenTipo.Servicio,
                    Descripcion = $"{concepto.Descripcion} · Pedido {pedido.NumeroPedido}",
                    CantidadMaximaTomable = pendiente,
                    CantidadSeleccionada = pendiente,
                    CantidadDisponibleFg = 0m,
                    PrecioUnitario = concepto.PrecioUnitario
                });
            }
        }

        return resultado;
    }
}
