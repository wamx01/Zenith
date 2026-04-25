using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Models;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;
using Microsoft.VSDiagnostics;
using System;
using System.Threading.Tasks;

namespace MundoVs.Benchmarks;
[CPUUsageDiagnoser]
public class RrhhChecadorCorreccionBenchmarks
{
    private readonly RrhhAsistenciaProcessor _processor = new();
    private readonly RrhhTiempoExtraResolutionService _resolutionService = new();
    [Params(1, 10)]
    public int EmployeeCount { get; set; }

    [Benchmark]
    public async Task<int> ReprocesarRangoAsync_UnDia()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var checador = CreateChecador(empresa.Id);
        db.Empresas.Add(empresa);
        db.RrhhChecadores.Add(checador);
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.MinutosMinimosTiempoExtra, "30"));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, RrhhAsistenciaDescansoSettings.ToleranciaCoincidenciaBrutaKey, "15"));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, RrhhAsistenciaDescansoSettings.ToleranciaCoincidenciaNetaKey, "15"));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, RrhhAsistenciaDescansoSettings.ZonaAmbiguaHastaKey, "30"));
        var fecha = new DateOnly(2026, 1, 5);
        for (var i = 0; i < EmployeeCount; i++)
        {
            var turno = CreateTurno(empresa.Id, i);
            var empleado = CreateEmpleado(empresa.Id, turno.Id, i);
            db.TurnosBase.Add(turno);
            db.Empleados.Add(empleado);
            db.RrhhMarcaciones.AddRange(CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), $"in-{i}-1"), CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 12, 0, 0), $"break-out-{i}-1"), CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 12, 30, 0), $"break-in-{i}-1"), CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 15, 0), $"out-{i}-1", TipoClasificacionMarcacionRrhh.Salida));
            db.RrhhAsistencias.Add(CreateAsistenciaBase(empresa.Id, empleado.Id, fecha));
        }

        await db.SaveChangesAsync();
        return await _processor.ReprocesarRangoAsync(db, empresa.Id, fecha, fecha);
    }

    [Benchmark]
    public async Task<int> ObtenerConfiguracionTiempoExtra_5Lecturas()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var empleado = CreateEmpleado(empresa.Id, Guid.NewGuid(), 0);
        db.Empresas.Add(empresa);
        db.Empleados.Add(empleado);
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.BancoHorasTopeHoras, "40"));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.FactorHoraExtra, "2"));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.BancoHorasHabilitado, "true"));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.BancoHorasFactorAcumulacion, "1"));
        db.RrhhBancoHorasMovimientos.Add(CreateMovimientoBanco(empresa.Id, empleado.Id, new DateOnly(2026, 1, 1), 8m, TipoMovimientoBancoHorasRrhh.AjusteManual, "saldo-inicial"));
        await db.SaveChangesAsync();
        var saldo = await _resolutionService.ObtenerSaldoBancoHorasAsync(db, empresa.Id, empleado.Id);
        var tope = await _resolutionService.ObtenerTopeBancoHorasAsync(db, empresa.Id);
        var factor = await _resolutionService.ObtenerFactorTiempoExtraAsync(db, empresa.Id);
        var habilitado = await _resolutionService.ObtenerBancoHorasHabilitadoAsync(db, empresa.Id);
        var factorBanco = await _resolutionService.ObtenerFactorAcumulacionBancoHorasAsync(db, empresa.Id);
        return saldo + tope + (int)factor + (habilitado ? 1 : 0) + (int)factorBanco;
    }

    private static CrmDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString("N")).Options;
        return new CrmDbContext(options);
    }

    private static Empresa CreateEmpresa() => new()
    {
        Id = Guid.NewGuid(),
        Codigo = $"EMP-{Guid.NewGuid():N}"[..12],
        RazonSocial = "Empresa Benchmark"
    };
    private static TurnoBase CreateTurno(Guid empresaId, int index)
    {
        var turno = new TurnoBase
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            Nombre = $"Matutino-{index}",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        turno.Detalles.Add(new TurnoBaseDetalle { Id = Guid.NewGuid(), TurnoBaseId = turno.Id, DiaSemana = DiaSemanaTurno.Lunes, Labora = true, HoraEntrada = new TimeSpan(8, 0, 0), HoraSalida = new TimeSpan(17, 0, 0), CantidadDescansos = 1, Descanso1Inicio = new TimeSpan(12, 0, 0), Descanso1Fin = new TimeSpan(12, 30, 0), Descanso1EsPagado = false, CreatedAt = DateTime.UtcNow, IsActive = true });
        return turno;
    }

    private static Empleado CreateEmpleado(Guid empresaId, Guid turnoId, int index) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Codigo = $"EMP-{index:000}",
        NumeroEmpleado = $"{index:000}",
        Nombre = $"Empleado {index}",
        CodigoChecador = $"3{index:000}",
        TurnoBaseId = turnoId,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };
    private static RrhhChecador CreateChecador(Guid empresaId) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Nombre = "Checador Benchmark",
        NumeroSerie = Guid.NewGuid().ToString("N"),
        ZonaHoraria = "America/Mexico_City",
        Ip = "127.0.0.1",
        Puerto = 4370,
        NumeroMaquina = 1,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };
    private static AppConfig CreateAppConfig(Guid empresaId, string clave, string valor) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Clave = clave,
        Valor = valor,
        CreatedAt = DateTime.UtcNow
    };
    private static RrhhAsistencia CreateAsistenciaBase(Guid empresaId, Guid empleadoId, DateOnly fecha) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        EmpleadoId = empleadoId,
        Fecha = fecha,
        Estatus = RrhhAsistenciaEstatus.AsistenciaNormal,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };
    private static RrhhBancoHorasMovimiento CreateMovimientoBanco(Guid empresaId, Guid empleadoId, DateOnly fecha, decimal horas, TipoMovimientoBancoHorasRrhh tipo, string referencia)
    {
        return new RrhhBancoHorasMovimiento
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            Fecha = fecha,
            TipoMovimiento = tipo,
            Horas = horas,
            ReferenciaTipo = referencia,
            Observaciones = "Benchmark",
            EsAutomatico = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    private static RrhhMarcacion CreateMarcacionLocal(Guid empresaId, Guid checadorId, Empleado empleado, DateTime fechaHoraLocal, string eventoId, TipoClasificacionMarcacionRrhh clasificacion = TipoClasificacionMarcacionRrhh.Entrada)
    {
        return new RrhhMarcacion
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            ChecadorId = checadorId,
            EmpleadoId = empleado.Id,
            CodigoChecador = empleado.CodigoChecador!,
            FechaHoraMarcacionLocal = fechaHoraLocal,
            FechaHoraMarcacionUtc = DateTime.SpecifyKind(fechaHoraLocal.AddHours(6), DateTimeKind.Utc),
            ZonaHorariaAplicada = "America/Mexico_City",
            TipoMarcacionRaw = "0",
            Origen = "Benchmark",
            EventoIdExterno = eventoId,
            HashUnico = Guid.NewGuid().ToString("N"),
            ClasificacionOperativa = clasificacion,
            Procesada = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }
}