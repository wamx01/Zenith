using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

/// <summary>
/// Fase 7 — gate duro: <see cref="IRrhhResolucionPeriodoService.ObtenerEmpleadosConExtraSinAutorizarAsync"/>
/// devuelve los empleados con tiempo extra detectado (Sum(MinutosExtra) &gt; 0) del periodo y SIN
/// resolución Autorizada. Vacío = OK. Solo bloquea cuando existe extra no autorizado.
/// </summary>
public sealed class RrhhResolucionGateTests
{
    private static readonly DateTime Inicio = new(2026, 1, 5);
    private static readonly DateTime Fin = new(2026, 1, 11);
    private static readonly DateOnly InicioDate = DateOnly.FromDateTime(Inicio);
    private static readonly DateOnly FinDate = DateOnly.FromDateTime(Fin);
    private static readonly DateOnly Dia = new(2026, 1, 6);

    [Fact]
    public async Task SinExtra_NoBloquea()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);

        var service = CreateService();
        var sinAutorizar = await service.ObtenerEmpleadosConExtraSinAutorizarAsync(db, empresa.Id, PeriodicidadPago.Semanal, Inicio, Fin);

        Assert.Empty(sinAutorizar);
    }

    [Fact]
    public async Task TodoAutorizado_NoBloquea()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, Dia, minutosExtra: 60));
        db.RrhhResolucionesTiempoExtraPeriodo.Add(CrearResolucion(empresa.Id, empleado.Id, RrhhResolucionPeriodoEstatus.Autorizada));
        await db.SaveChangesAsync();

        var service = CreateService();
        var sinAutorizar = await service.ObtenerEmpleadosConExtraSinAutorizarAsync(db, empresa.Id, PeriodicidadPago.Semanal, Inicio, Fin);

        Assert.Empty(sinAutorizar);
    }

    [Fact]
    public async Task ExtraSinResolucion_DevuelveEmpleado()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, Dia, minutosExtra: 60));
        await db.SaveChangesAsync();

        var service = CreateService();
        var sinAutorizar = await service.ObtenerEmpleadosConExtraSinAutorizarAsync(db, empresa.Id, PeriodicidadPago.Semanal, Inicio, Fin);

        Assert.Single(sinAutorizar);
        Assert.Equal(empleado.Id, sinAutorizar[0]);
    }

    [Fact]
    public async Task ExtraCeroSinResolucion_NoBloquea()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, Dia, minutosExtra: 0));
        await db.SaveChangesAsync();

        var service = CreateService();
        var sinAutorizar = await service.ObtenerEmpleadosConExtraSinAutorizarAsync(db, empresa.Id, PeriodicidadPago.Semanal, Inicio, Fin);

        Assert.Empty(sinAutorizar); // sin extra → no requiere resolución
    }

    [Fact]
    public async Task PeriodicidadFiltra_SoloLaPeriodicidadConsultada()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var semanal = CreateEmpleado(empresa.Id, PeriodicidadPago.Semanal);
        var quincenal = CreateEmpleado(empresa.Id, PeriodicidadPago.Quincenal);
        db.Empresas.Add(empresa);
        db.Empleados.AddRange(semanal, quincenal);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, semanal.Id, Dia, minutosExtra: 60));
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, quincenal.Id, Dia, minutosExtra: 60));
        await db.SaveChangesAsync();

        var service = CreateService();
        var sinAutorizarSemanal = await service.ObtenerEmpleadosConExtraSinAutorizarAsync(db, empresa.Id, PeriodicidadPago.Semanal, Inicio, Fin);
        var sinAutorizarQuincenal = await service.ObtenerEmpleadosConExtraSinAutorizarAsync(db, empresa.Id, PeriodicidadPago.Quincenal, Inicio, Fin);

        Assert.Single(sinAutorizarSemanal);
        Assert.Equal(semanal.Id, sinAutorizarSemanal[0]);
        Assert.Single(sinAutorizarQuincenal);
        Assert.Equal(quincenal.Id, sinAutorizarQuincenal[0]);
    }

    [Fact]
    public async Task ResolucionPendienteNoCuenta_ComoAutorizada()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, Dia, minutosExtra: 60));
        db.RrhhResolucionesTiempoExtraPeriodo.Add(CrearResolucion(empresa.Id, empleado.Id, RrhhResolucionPeriodoEstatus.Pendiente));
        await db.SaveChangesAsync();

        var service = CreateService();
        var sinAutorizar = await service.ObtenerEmpleadosConExtraSinAutorizarAsync(db, empresa.Id, PeriodicidadPago.Semanal, Inicio, Fin);

        Assert.Single(sinAutorizar); // Pendiente ≠ Autorizada → sigue bloqueando
    }

    private static CrmDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new CrmDbContext(options);
    }

    private static IRrhhResolucionPeriodoService CreateService()
        => new RrhhResolucionPeriodoService(new RrhhTiempoExtraResolutionService());

    private static async Task<(Empresa Empresa, Empleado Empleado)> SembrarAsync(CrmDbContext db)
    {
        var empresa = CreateEmpresa();
        var empleado = CreateEmpleado(empresa.Id, PeriodicidadPago.Semanal);
        db.Empresas.Add(empresa);
        db.Empleados.Add(empleado);
        await db.SaveChangesAsync();
        return (empresa, empleado);
    }

    private static RrhhAsistencia CrearAsistencia(Guid empresaId, Guid empleadoId, DateOnly fecha, int minutosExtra)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            Fecha = fecha,
            MinutosTrabajadosNetos = 480,
            MinutosJornadaNetaProgramada = 480,
            MinutosExtra = minutosExtra,
            MinutosRetardo = 0,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

    private static RrhhResolucionTiempoExtraPeriodo CrearResolucion(Guid empresaId, Guid empleadoId, RrhhResolucionPeriodoEstatus estatus)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            PeriodicidadPago = PeriodicidadPago.Semanal,
            AnioPeriodo = 2026,
            NumeroPeriodo = 1,
            FechaInicio = InicioDate,
            FechaFin = FinDate,
            Estatus = estatus,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

    private static Empresa CreateEmpresa() => new()
    {
        Id = Guid.NewGuid(),
        Codigo = $"EMP-{Guid.NewGuid():N}"[..12],
        RazonSocial = "Empresa Test"
    };

    private static Empleado CreateEmpleado(Guid empresaId, PeriodicidadPago periodicidad) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Codigo = "EMP-001",
        NumeroEmpleado = "001",
        Nombre = "Empleado Test",
        CodigoChecador = "3001",
        TipoNomina = TipoNomina.Semanal,
        PeriodicidadPago = periodicidad,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };
}