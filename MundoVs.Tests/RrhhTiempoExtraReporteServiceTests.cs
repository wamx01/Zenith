using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Models;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

/// <summary>
/// Pruebas del reporte de tiempo extra — Fase 8: migración a periodo + líneas.
/// SQLite in-memory (necesario para que Include + Where se traduzcan; el provider
/// InMemory no traduce includes).
/// </summary>
public sealed class RrhhTiempoExtraReporteServiceTests
{
    private static readonly DateOnly Inicio = new(2025, 12, 29);
    private static readonly DateOnly Fin = new(2026, 1, 4);
    private static readonly DateOnly DiaUno = new(2025, 12, 30);

    [Fact]
    public async Task ConResolucionPorLineas_TotalesYPeriodosPorLinea()
    {
        var (factory, connection) = await CreateFactoryAsync();
        await using (var db = await factory.CreateDbContextAsync())
        {
            var empresa = CreateEmpresa();
            var empleado = CreateEmpleado(empresa.Id, sueldoSemanal: 480m); // sueldoHora = 480/48 = 10
            db.Empresas.Add(empresa);
            db.Empleados.Add(empleado);
            db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 300));

            var resolucion = CrearResolucion(empresa.Id, empleado.Id, RrhhResolucionPeriodoEstatus.Autorizada);
            resolucion.MinutosExtraPago = 300;
            resolucion.MinutosExtraDobles = 120;
            resolucion.MinutosExtraTriples = 0;
            resolucion.MinutosExtraSimples = 180;
            resolucion.HorasExtraFactoradas = 7m;
            resolucion.Lineas.Add(NewLinea(resolucion.Id, empresa.Id, empleado.Id, 0, RrhhDestinoTiempoExtraLinea.Pago, 120, 2m));
            resolucion.Lineas.Add(NewLinea(resolucion.Id, empresa.Id, empleado.Id, 1, RrhhDestinoTiempoExtraLinea.Pago, 180, 1m));
            db.RrhhResolucionesTiempoExtraPeriodo.Add(resolucion);
            await db.SaveChangesAsync();
        }

        var service = new RrhhTiempoExtraReporteService();
        var respuesta = await service.GenerarAsync(factory, new RrhhTiempoExtraReporteRequest
        {
            EmpresaId = EmpresaIdFijo,
            FechaDesde = Inicio,
            FechaHasta = Fin,
            AgrupadoPor = "empleado"
        });

        Assert.Single(respuesta.Empleados);
        var emp = respuesta.Empleados[0];
        Assert.False(emp.SinAutorizar);
        Assert.Single(emp.Periodos);
        var periodo = emp.Periodos[0];
        Assert.Equal(2, periodo.Lineas.Count);

        // Totales desde las líneas.
        Assert.Equal(300, emp.Totales.TotalMinutosExtraAutorizadosPago);
        Assert.Equal(120 * 2 + 180 * 1, emp.Totales.TotalMinutosPagoFactorado); // 420
        Assert.Equal(2m, emp.Totales.TotalHorasExtraDobles);   // 120 min / 60 (factor 2)
        Assert.Equal(0m, emp.Totales.TotalHorasExtraTriples);
        // Monto estimado = (120/60×2×10) + (180/60×1×10) = 40 + 30 = 70.
        Assert.Equal(70m, emp.Totales.TotalMontoHorasExtraEstimado);
        Assert.Equal(70m, periodo.MontoEstimado);
    }

    [Fact]
    public async Task SinResolucion_MarcaSinAutorizarYAprobadoCero()
    {
        var (factory, connection) = await CreateFactoryAsync();
        await using (var db = await factory.CreateDbContextAsync())
        {
            var empresa = CreateEmpresa();
            var empleado = CreateEmpleado(empresa.Id, sueldoSemanal: 480m);
            db.Empresas.Add(empresa);
            db.Empleados.Add(empleado);
            db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 120)); // detección
            await db.SaveChangesAsync();
        }

        var service = new RrhhTiempoExtraReporteService();
        var respuesta = await service.GenerarAsync(factory, new RrhhTiempoExtraReporteRequest
        {
            EmpresaId = EmpresaIdFijo,
            FechaDesde = Inicio,
            FechaHasta = Fin,
            AgrupadoPor = "empleado"
        });

        var emp = Assert.Single(respuesta.Empleados);
        Assert.True(emp.SinAutorizar);
        Assert.Empty(emp.Periodos);
        Assert.Equal(120, emp.Totales.TotalMinutosExtra);      // detección visible
        Assert.Equal(0, emp.Totales.TotalMinutosExtraAutorizadosPago); // nada aprobado
        Assert.Equal(0m, emp.Totales.TotalMontoHorasExtraEstimado);
    }

    // ── helpers ──

    private static readonly Guid EmpresaIdFijo = Guid.NewGuid();

    private static async Task<(IDbContextFactory<CrmDbContext> factory, SqliteConnection connection)> CreateFactoryAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlite(connection)
            .Options;
        // Sembrar schema.
        await using (var db = new CrmDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
        }
        var factory = new StaticDbContextFactory(options);
        return (factory, connection);
    }

    private sealed class StaticDbContextFactory(DbContextOptions<CrmDbContext> options) : IDbContextFactory<CrmDbContext>
    {
        public CrmDbContext CreateDbContext() => new(options);
        public Task<CrmDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new CrmDbContext(options));
    }

    private static Empresa CreateEmpresa() => new()
    {
        Id = EmpresaIdFijo,
        Codigo = $"EMP-{Guid.NewGuid():N}"[..12],
        RazonSocial = "Empresa Test"
    };

    private static Empleado CreateEmpleado(Guid empresaId, decimal sueldoSemanal) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Codigo = "EMP-001",
        NumeroEmpleado = "001",
        Nombre = "Empleado",
        ApellidoPaterno = "Test",
        ApellidoMaterno = "Uno",
        CodigoChecador = "3001",
        TipoNomina = TipoNomina.Semanal,
        PeriodicidadPago = PeriodicidadPago.Semanal,
        SueldoSemanal = sueldoSemanal,
        FechaContratacion = new DateTime(2024, 1, 1),
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static RrhhAsistencia CrearAsistencia(Guid empresaId, Guid empleadoId, DateOnly fecha, int minutosExtra)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            Fecha = fecha,
            Estatus = RrhhAsistenciaEstatus.AsistenciaNormal,
            MinutosTrabajadosNetos = 480,
            MinutosJornadaNetaProgramada = 480,
            MinutosExtra = minutosExtra,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

    private static RrhhResolucionTiempoExtraPeriodo CrearResolucion(
        Guid empresaId, Guid empleadoId, RrhhResolucionPeriodoEstatus estatus)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            PeriodicidadPago = PeriodicidadPago.Semanal,
            AnioPeriodo = 2026,
            NumeroPeriodo = 1,
            PeriodoKey = "Semanal-2026-01",
            PeriodoEtiqueta = "Semanal 01/2026",
            FechaInicio = Inicio,
            FechaFin = Fin,
            Estatus = estatus,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

    private static RrhhResolucionTiempoExtraLinea NewLinea(
        Guid resolucionId, Guid empresaId, Guid empleadoId, int orden,
        RrhhDestinoTiempoExtraLinea destino, int minutos, decimal factor)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            ResolucionPeriodoId = resolucionId,
            Orden = orden,
            Destino = destino,
            Minutos = minutos,
            Factor = factor,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
}