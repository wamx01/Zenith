using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Entities.Calzado;
using MundoVs.Core.Interfaces;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

public sealed class MultiEmpresaIsolationTests
{
    [Fact]
    public async Task CatalogoTallaCalzado_QueryFilter_AislaRegistrosPorEmpresa()
    {
        var empresaA = CreateEmpresa("EMP-A", "Empresa A");
        var empresaB = CreateEmpresa("EMP-B", "Empresa B");

        await SeedCatalogoTallasAsync(Guid.Empty, empresaA, empresaB);

        await using var dbEmpresaA = CreateDbContext(empresaA.Id);
        var tallasEmpresaA = await dbEmpresaA.CatalogoTallasCalzado
            .AsNoTracking()
            .OrderBy(t => t.Orden)
            .ToListAsync();

        Assert.Equal(2, tallasEmpresaA.Count);
        Assert.All(tallasEmpresaA, talla => Assert.Equal(empresaA.Id, talla.EmpresaId));
        Assert.DoesNotContain(tallasEmpresaA, talla => talla.Talla == "28");

        await using var dbEmpresaB = CreateDbContext(empresaB.Id);
        var tallasEmpresaB = await dbEmpresaB.CatalogoTallasCalzado
            .AsNoTracking()
            .OrderBy(t => t.Orden)
            .ToListAsync();

        Assert.Single(tallasEmpresaB);
        Assert.All(tallasEmpresaB, talla => Assert.Equal(empresaB.Id, talla.EmpresaId));
        Assert.Equal("28", tallasEmpresaB[0].Talla);
    }

    [Fact]
    public async Task FacturacionRelacionada_DebeConservarEmpresaIdEnFacturasRelacionesYCobros()
    {
        var empresa = CreateEmpresa("EMP-FAC", "Empresa Facturación");
        var cliente = CreateCliente(empresa.Id, "CLI-001", "Cliente Demo");
        var pedido = CreatePedido(empresa.Id, cliente.Id, "PED-001");
        var nota = CreateNotaEntrega(empresa.Id, cliente.Id, pedido.Id, "NE-001");

        await using (var seedDb = CreateDbContext(empresa.Id))
        {
            seedDb.Empresas.Add(empresa);
            seedDb.Clientes.Add(cliente);
            seedDb.Pedidos.Add(pedido);
            seedDb.NotasEntrega.Add(nota);
            await seedDb.SaveChangesAsync();
        }

        var facturaId = Guid.NewGuid();
        var pagoId = Guid.NewGuid();

        await using (var db = CreateDbContext(empresa.Id))
        {
            db.Facturas.Add(new Factura
            {
                Id = facturaId,
                EmpresaId = empresa.Id,
                ClienteId = cliente.Id,
                PedidoId = pedido.Id,
                NotaEntregaId = nota.Id,
                FolioInterno = "FAC-001",
                FechaEmision = DateTime.Today,
                Estatus = FacturaEstatus.Borrador,
                TipoComprobante = FacturaTipoComprobante.Ingreso,
                MetodoPagoSat = "PUE",
                FormaPagoSat = "03",
                Moneda = "MXN",
                Total = 116m,
                Subtotal = 100m,
                TotalImpuestosTrasladados = 16m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            db.FacturasNotasEntrega.Add(new FacturaNotaEntrega
            {
                Id = Guid.NewGuid(),
                EmpresaId = nota.EmpresaId,
                FacturaId = facturaId,
                NotaEntregaId = nota.Id,
                Subtotal = nota.Subtotal,
                Impuestos = nota.Impuestos,
                Total = nota.Total,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            db.PagosRecibidos.Add(new PagoRecibido
            {
                Id = pagoId,
                EmpresaId = empresa.Id,
                ClienteId = cliente.Id,
                PedidoId = pedido.Id,
                NotaEntregaId = nota.Id,
                FechaPago = DateTime.Today,
                Monto = 116m,
                FormaPagoSat = "03",
                MedioCobroInterno = "Transferencia",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            db.PagosAplicacionDocumento.Add(new PagoAplicacionDocumento
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresa.Id,
                PagoRecibidoId = pagoId,
                FacturaId = facturaId,
                ImporteAplicado = 116m,
                SaldoAnterior = 116m,
                SaldoInsoluto = 0m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }

        await using var assertDb = CreateDbContext(empresa.Id);
        var factura = await assertDb.Facturas.AsNoTracking().SingleAsync(f => f.Id == facturaId);
        var relacion = await assertDb.FacturasNotasEntrega.AsNoTracking().SingleAsync(r => r.FacturaId == facturaId);
        var pago = await assertDb.PagosRecibidos.AsNoTracking().SingleAsync(p => p.Id == pagoId);
        var aplicacion = await assertDb.PagosAplicacionDocumento.AsNoTracking().SingleAsync(a => a.PagoRecibidoId == pagoId);

        Assert.Equal(empresa.Id, factura.EmpresaId);
        Assert.Equal(empresa.Id, relacion.EmpresaId);
        Assert.Equal(empresa.Id, pago.EmpresaId);
        Assert.Equal(empresa.Id, aplicacion.EmpresaId);
    }

    private static async Task SeedCatalogoTallasAsync(Guid empresaContexto, Empresa empresaA, Empresa empresaB)
    {
        await using var db = CreateDbContext(empresaContexto);
        db.Empresas.AddRange(empresaA, empresaB);
        db.CatalogoTallasCalzado.AddRange(
            new CatalogoTallaCalzado
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaA.Id,
                Talla = "26",
                SistemaNumeracion = SistemaNumeracionCalzadoEnum.MX,
                Orden = 1,
                Activa = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CatalogoTallaCalzado
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaA.Id,
                Talla = "27",
                SistemaNumeracion = SistemaNumeracionCalzadoEnum.MX,
                Orden = 2,
                Activa = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new CatalogoTallaCalzado
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaB.Id,
                Talla = "28",
                SistemaNumeracion = SistemaNumeracionCalzadoEnum.MX,
                Orden = 1,
                Activa = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync();
    }

    private static CrmDbContext CreateDbContext(Guid empresaId)
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase("multiempresa-regression-tests")
            .Options;

        return new CrmDbContext(options, new TestEmpresaContext(empresaId));
    }

    private static Empresa CreateEmpresa(string codigo, string razonSocial) => new()
    {
        Id = Guid.NewGuid(),
        Codigo = codigo,
        RazonSocial = razonSocial,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static Cliente CreateCliente(Guid empresaId, string codigo, string razonSocial) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Codigo = codigo,
        RazonSocial = razonSocial,
        RfcCif = "XAXX010101000",
        Email = "cliente@test.local",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static Pedido CreatePedido(Guid empresaId, Guid clienteId, string numeroPedido) => new()
    {
        Id = Guid.NewGuid(),
        ClienteId = clienteId,
        NumeroPedido = numeroPedido,
        FechaPedido = DateTime.Today,
        Estado = EstadoPedidoEnum.Nuevo,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static NotaEntrega CreateNotaEntrega(Guid empresaId, Guid clienteId, Guid pedidoId, string numeroNota) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        ClienteId = clienteId,
        PedidoId = pedidoId,
        NumeroNota = numeroNota,
        FechaNota = DateTime.Today,
        Estatus = NotaEntregaEstatus.Emitida,
        Subtotal = 100m,
        Impuestos = 16m,
        Total = 116m,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private sealed class TestEmpresaContext(Guid empresaId) : IEmpresaContext
    {
        public Guid EmpresaId { get; private set; } = empresaId;

        public void SetEmpresaId(Guid empresaId)
            => EmpresaId = empresaId;
    }
}
