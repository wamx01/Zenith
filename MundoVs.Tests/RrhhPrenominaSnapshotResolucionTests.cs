using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

/// <summary>
/// Fase 7 — el snapshot de prenómina consume la resolución Autorizada del periodo (mismo helper
/// que la nómina) para que el display de la prenómina cuadre con el cálculo de nómina. Sin
/// resolución (o Pendiente/Reabierta) cae al resumen diario (comportamiento histórico).
/// </summary>
public sealed class RrhhPrenominaSnapshotResolucionTests
{
    private static readonly DateTime Inicio = new(2026, 1, 5);
    private static readonly DateTime Fin = new(2026, 1, 11);
    private static readonly DateOnly InicioDate = DateOnly.FromDateTime(Inicio);
    private static readonly DateOnly FinDate = DateOnly.FromDateTime(Fin);
    private static readonly DateOnly Dia = new(2026, 1, 6);

    [Fact]
    public async Task ResolucionAutorizada_OverrideaOvertimeYAliviaDeducciones()
    {
        var (db, connection) = await CreateDbContextAsync();
        await using var __c = connection;
        await using var __db = db;
        var (empresa, empleado) = await SembrarAsync(db);
        // Daily: MinutosExtra=180 (base), MinutosExtraAutorizadosPago=0 (dormido en el flujo por
        // periodo), retardo 30, faltante 60. La resolución Autorizada define el pago/banco/alivio.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, Dia,
            minutosExtra: 180, minutosExtraAutorizadosPago: 0, retardo: 30, faltante: 60));
        db.RrhhResolucionesTiempoExtraPeriodo.Add(CrearResolucion(
            empresa.Id, empleado.Id, RrhhResolucionPeriodoEstatus.Autorizada,
            minutosExtraPago: 120, minutosExtraDetectado: 180, minutosExtraBanco: 60,
            minutosExtraDobles: 120, minutosExtraTriples: 0, factorAplicado: 2.5m,
            minutosFaltanteNeto: 60, minutosFaltanteAbsorbido: 60,
            minutosRetardoDetectado: 30, minutosRetardoAbsorbido: 30));
        await db.SaveChangesAsync();

        var snapshot = (await CreateService().ConstruirSnapshotPeriodoAsync(db, Inicio, Fin, Configuracion()))
            .Single(s => s.Empleado.Id == empleado.Id);

        Assert.Equal(2m, snapshot.HorasExtra);             // 120 min / 60 (de la resolución, no 0 daily)
        Assert.Equal(3m, snapshot.HorasExtraBase);         // 180 min / 60
        Assert.Equal(1m, snapshot.HorasBancoAcumuladas);    // 60 min / 60
        Assert.Equal(0, snapshot.MinutosFaltanteDescontable); // 60 - 60 aliviado
        Assert.Equal(0, snapshot.MinutosRetardo);            // 30 - 30 aliviado
        Assert.Equal(2.5m, snapshot.FactorPagoTiempoExtra);
    }

    [Fact]
    public async Task SinResolucion_CaeAlResumenDiario()
    {
        var (db, connection) = await CreateDbContextAsync();
        await using var __c = connection;
        await using var __db = db;
        var (empresa, empleado) = await SembrarAsync(db);
        // Daily dormido NO: autorizado pago = 120 para validar que cae al resumen diario.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, Dia,
            minutosExtra: 120, minutosExtraAutorizadosPago: 120, retardo: 10, faltante: 0));
        await db.SaveChangesAsync();

        var snapshot = (await CreateService().ConstruirSnapshotPeriodoAsync(db, Inicio, Fin, Configuracion()))
            .Single(s => s.Empleado.Id == empleado.Id);

        Assert.Equal(2m, snapshot.HorasExtra);      // 120 min / 60 del daily
        Assert.Equal(2m, snapshot.HorasExtraBase);   // 120 min / 60 del daily
        Assert.Equal(10, snapshot.MinutosRetardo);   // daily, sin alivio
    }

    [Fact]
    public async Task CompensacionAprobada_ReduceFaltanteDescontable_SinResolucion()
    {
        var (db, connection) = await CreateDbContextAsync();
        await using var __c = connection;
        await using var __db = db;
        var (empresa, empleado) = await SembrarAsync(db);
        // Faltante 90 (jornada 480, neto 390), compensación aprobada 40 → faltante
        // descontable 50. Pre-F2a (compensación ignorada) el snapshot reportaba 90.
        var a = CrearAsistencia(empresa.Id, empleado.Id, Dia,
            minutosExtra: 0, minutosExtraAutorizadosPago: 0, retardo: 0, faltante: 90);
        a.MinutosCompensacionPermisoAprobados = 40;
        db.RrhhAsistencias.Add(a);
        await db.SaveChangesAsync();

        var snapshot = (await CreateService().ConstruirSnapshotPeriodoAsync(db, Inicio, Fin, Configuracion()))
            .Single(s => s.Empleado.Id == empleado.Id);

        Assert.Equal(50, snapshot.MinutosFaltanteDescontable); // 90 − 40 compensación
    }

    [Theory]
    [InlineData(RrhhResolucionPeriodoEstatus.Pendiente)]
    [InlineData(RrhhResolucionPeriodoEstatus.Reabierta)]
    public async Task ResolucionNoAutorizada_NoOverridea_CaeADaily(RrhhResolucionPeriodoEstatus estatus)
    {
        var (db, connection) = await CreateDbContextAsync();
        await using var __c = connection;
        await using var __db = db;
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, Dia,
            minutosExtra: 120, minutosExtraAutorizadosPago: 120, retardo: 0, faltante: 0));
        db.RrhhResolucionesTiempoExtraPeriodo.Add(CrearResolucion(
            empresa.Id, empleado.Id, estatus,
            minutosExtraPago: 999, minutosExtraDetectado: 999, minutosExtraBanco: 999));
        await db.SaveChangesAsync();

        var snapshot = (await CreateService().ConstruirSnapshotPeriodoAsync(db, Inicio, Fin, Configuracion()))
            .Single(s => s.Empleado.Id == empleado.Id);

        Assert.Equal(2m, snapshot.HorasExtra);           // daily 120/60, NO 999/60
        Assert.Equal(2m, snapshot.HorasExtraBase);        // daily, NO 999/60
        Assert.Equal(0m, snapshot.HorasBancoAcumuladas);  // daily 0, NO 999/60
    }

    private static async Task<(CrmDbContext db, SqliteConnection connection)> CreateDbContextAsync()
    {
        // SQLite in-memory: el GroupBy+Include de ObtenerEsquemasPagoPeriodoAsync no se traduce
        // en el provider InMemory. SQLite lo traduce; la conexión debe mantenerse abierta para
        // que la BD :memory: persista entre queries del mismo contexto.
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new CrmDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return (db, connection);
    }

    private static RrhhPrenominaSnapshotService CreateService()
        => new(new NominaLegalPolicyService(), new RrhhTiempoExtraResolutionService());

    private static NominaConfiguracion Configuracion() => new()
    {
        FactorHoraExtra = 2m,
        HorasExtraDoblesPorSemana = 9
    };

    private static async Task<(Empresa Empresa, Empleado Empleado)> SembrarAsync(CrmDbContext db)
    {
        var empresa = CreateEmpresa();
        var empleado = CreateEmpleado(empresa.Id);
        db.Empresas.Add(empresa);
        db.Empleados.Add(empleado);
        await db.SaveChangesAsync();
        return (empresa, empleado);
    }

    private static RrhhAsistencia CrearAsistencia(Guid empresaId, Guid empleadoId, DateOnly fecha,
        int minutosExtra, int minutosExtraAutorizadosPago, int retardo, int faltante)
    {
        var neto = 480 - faltante;
        return new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            Fecha = fecha,
            Estatus = RrhhAsistenciaEstatus.AsistenciaNormal,
            MinutosTrabajadosNetos = neto,
            MinutosJornadaNetaProgramada = 480,
            MinutosExtra = minutosExtra,
            MinutosExtraAutorizadosPago = minutosExtraAutorizadosPago,
            MinutosRetardo = retardo,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    private static RrhhResolucionTiempoExtraPeriodo CrearResolucion(
        Guid empresaId, Guid empleadoId, RrhhResolucionPeriodoEstatus estatus,
        int minutosExtraPago = 0, int minutosExtraDetectado = 0, int minutosExtraBanco = 0,
        int minutosExtraDobles = 0, int minutosExtraTriples = 0, decimal? factorAplicado = null,
        int minutosFaltanteNeto = 0, int minutosFaltanteAbsorbido = 0,
        int minutosRetardoDetectado = 0, int minutosRetardoAbsorbido = 0)
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
            MinutosExtraPago = minutosExtraPago,
            MinutosExtraDetectado = minutosExtraDetectado,
            MinutosExtraBanco = minutosExtraBanco,
            MinutosExtraDobles = minutosExtraDobles,
            MinutosExtraTriples = minutosExtraTriples,
            FactorTiempoExtraAplicado = factorAplicado,
            MinutosFaltanteNetoDetectado = minutosFaltanteNeto,
            MinutosFaltanteAbsorbidoExtra = minutosFaltanteAbsorbido,
            MinutosRetardoDetectado = minutosRetardoDetectado,
            MinutosRetardoAbsorbidoExtra = minutosRetardoAbsorbido,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

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
        SueldoSemanal = 1000m,
        FechaContratacion = new DateTime(2024, 1, 1),
        AplicaImss = true,
        AplicaIsr = true,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };
}