using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

public class RrhhEsquemaJornadaResolverTests
{
    private static CrmDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new CrmDbContext(options);
    }

    private static Empresa CreateEmpresa() => new()
    {
        Id = Guid.NewGuid(),
        Codigo = $"EMP-{Guid.NewGuid():N}"[..12],
        RazonSocial = "Empresa Test"
    };

    private static Empleado CreateEmpleado(Guid empresaId) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Codigo = "EMP-001",
        NumeroEmpleado = "001",
        Nombre = "Empleado Test",
        CodigoChecador = "3001",
        TipoNomina = TipoNomina.Semanal,
        PeriodicidadPago = PeriodicidadPago.Semanal,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static EmpleadoEsquemaJornada Esquema(Guid empleadoId, TipoJornada tipo, DateTime desde, DateTime? hasta = null)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpleadoId = empleadoId,
            TipoJornada = tipo,
            VigenteDesde = desde,
            VigenteHasta = hasta,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    [Fact]
    public async Task SinEsquemas_DevuelveFijaPorDefecto()
    {
        await using var db = CreateDbContext();
        var (_, empleado) = await SembrarSemillaAsync(db);
        var resolver = new RrhhEsquemaJornadaResolver();

        var resuelto = await resolver.ObtenerEsquemaVigenteAsync(db, empleado.Id, new DateTime(2026, 7, 18));

        Assert.Equal(TipoJornada.Fija, resuelto.TipoJornada);
        Assert.True(resuelto.EsDefault);
        Assert.False(resuelto.EsHueco);
        Assert.Null(resuelto.EsquemaId);
    }

    private static async Task<(Empresa empresa, Empleado empleado)> SembrarSemillaAsync(CrmDbContext db)
    {
        var empresa = CreateEmpresa();
        var empleado = CreateEmpleado(empresa.Id);
        db.Empresas.Add(empresa);
        db.Empleados.Add(empleado);
        await db.SaveChangesAsync();
        return (empresa, empleado);
    }

    [Fact]
    public async Task EsquemaFijaVigente_DevuelveFija()
    {
        await using var db = CreateDbContext();
        var (_, empleado) = await SembrarSemillaAsync(db);
        db.EmpleadosEsquemaJornada.Add(Esquema(empleado.Id, TipoJornada.Fija, new DateTime(2024, 1, 1)));
        await db.SaveChangesAsync();
        var resolver = new RrhhEsquemaJornadaResolver();

        var resuelto = await resolver.ObtenerEsquemaVigenteAsync(db, empleado.Id, new DateTime(2026, 7, 18));

        Assert.Equal(TipoJornada.Fija, resuelto.TipoJornada);
        Assert.False(resuelto.EsDefault);
        Assert.False(resuelto.EsHueco);
        Assert.NotNull(resuelto.EsquemaId);
    }

    [Fact]
    public async Task EsquemaPorHorasVigente_DevuelvePorHoras()
    {
        await using var db = CreateDbContext();
        var (_, empleado) = await SembrarSemillaAsync(db);
        db.EmpleadosEsquemaJornada.Add(Esquema(empleado.Id, TipoJornada.PorHoras, new DateTime(2026, 6, 1)));
        await db.SaveChangesAsync();
        var resolver = new RrhhEsquemaJornadaResolver();

        var resuelto = await resolver.ObtenerEsquemaVigenteAsync(db, empleado.Id, new DateTime(2026, 7, 18));

        Assert.Equal(TipoJornada.PorHoras, resuelto.TipoJornada);
        Assert.False(resuelto.EsDefault);
        Assert.False(resuelto.EsHueco);
    }

    [Fact]
    public async Task EsquemaConVigenciaCerrada_CaeADefaultFija()
    {
        await using var db = CreateDbContext();
        var (_, empleado) = await SembrarSemillaAsync(db);
        // Vigencia cerrada en mayo 2026; a julio ya no aplica y no hay esquema futuro.
        db.EmpleadosEsquemaJornada.Add(Esquema(empleado.Id, TipoJornada.PorHoras, new DateTime(2026, 1, 1), new DateTime(2026, 5, 31)));
        await db.SaveChangesAsync();
        var resolver = new RrhhEsquemaJornadaResolver();

        var resuelto = await resolver.ObtenerEsquemaVigenteAsync(db, empleado.Id, new DateTime(2026, 7, 18));

        Assert.Equal(TipoJornada.Fija, resuelto.TipoJornada);
        Assert.True(resuelto.EsDefault);
        Assert.False(resuelto.EsHueco);
        Assert.Null(resuelto.EsquemaId);
    }

    [Fact]
    public async Task Hueco_EsquemaFuturoSinEsquemaActivo_DevuelveFijaConHueco()
    {
        await using var db = CreateDbContext();
        var (_, empleado) = await SembrarSemillaAsync(db);
        // Sólo existe un esquema PorHoras que entra en agosto; a julio no hay nada activo → hueco.
        db.EmpleadosEsquemaJornada.Add(Esquema(empleado.Id, TipoJornada.PorHoras, new DateTime(2026, 8, 1)));
        await db.SaveChangesAsync();
        var resolver = new RrhhEsquemaJornadaResolver();

        var resuelto = await resolver.ObtenerEsquemaVigenteAsync(db, empleado.Id, new DateTime(2026, 7, 18));

        Assert.Equal(TipoJornada.Fija, resuelto.TipoJornada);
        Assert.False(resuelto.EsDefault);
        Assert.True(resuelto.EsHueco);
        Assert.Null(resuelto.EsquemaId);
    }

    [Fact]
    public async Task CambioFijaAPorHoras_AntesDeLaFronteraEsFija_EnLaFronteraEsPorHoras()
    {
        await using var db = CreateDbContext();
        var (_, empleado) = await SembrarSemillaAsync(db);
        var frontera = new DateTime(2026, 7, 1);
        db.EmpleadosEsquemaJornada.Add(Esquema(empleado.Id, TipoJornada.Fija, new DateTime(2024, 1, 1), new DateTime(2026, 6, 30)));
        db.EmpleadosEsquemaJornada.Add(Esquema(empleado.Id, TipoJornada.PorHoras, frontera));
        await db.SaveChangesAsync();
        var resolver = new RrhhEsquemaJornadaResolver();

        var antes = await resolver.ObtenerTipoJornadaAsync(db, empleado.Id, new DateTime(2026, 6, 30));
        var enFrontera = await resolver.ObtenerTipoJornadaAsync(db, empleado.Id, frontera);
        var despues = await resolver.ObtenerTipoJornadaAsync(db, empleado.Id, new DateTime(2026, 7, 18));

        Assert.Equal(TipoJornada.Fija, antes);
        Assert.Equal(TipoJornada.PorHoras, enFrontera);
        Assert.Equal(TipoJornada.PorHoras, despues);
    }

    [Fact]
    public async Task SolapamientoDeVigencia_PriorizaElEsquemaMasReciente()
    {
        await using var db = CreateDbContext();
        var (_, empleado) = await SembrarSemillaAsync(db);
        // No debería pasar (la validación UI debe evitarlo), pero si ocurre, gana el VigenteDesde mayor.
        db.EmpleadosEsquemaJornada.Add(Esquema(empleado.Id, TipoJornada.Fija, new DateTime(2024, 1, 1)));
        db.EmpleadosEsquemaJornada.Add(Esquema(empleado.Id, TipoJornada.PorHoras, new DateTime(2026, 7, 10)));
        await db.SaveChangesAsync();
        var resolver = new RrhhEsquemaJornadaResolver();

        var resuelto = await resolver.ObtenerEsquemaVigenteAsync(db, empleado.Id, new DateTime(2026, 7, 18));

        Assert.Equal(TipoJornada.PorHoras, resuelto.TipoJornada);
        Assert.False(resuelto.EsHueco);
    }

    [Fact]
    public async Task EsquemaInactivo_SeIgnora()
    {
        await using var db = CreateDbContext();
        var (_, empleado) = await SembrarSemillaAsync(db);
        var inactivo = Esquema(empleado.Id, TipoJornada.PorHoras, new DateTime(2026, 6, 1));
        inactivo.IsActive = false;
        db.EmpleadosEsquemaJornada.Add(inactivo);
        await db.SaveChangesAsync();
        var resolver = new RrhhEsquemaJornadaResolver();

        var resuelto = await resolver.ObtenerEsquemaVigenteAsync(db, empleado.Id, new DateTime(2026, 7, 18));

        Assert.Equal(TipoJornada.Fija, resuelto.TipoJornada);
        Assert.True(resuelto.EsDefault);
        Assert.False(resuelto.EsHueco);
    }
}