using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Models;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

public sealed class RrhhMarcacionZonaHorariaServiceTests
{
    [Fact]
    public async Task CorregirMarcacionesGuardadasComoHoraLocalAsync_ConvierteAMarcacionesUtcYPermiteReclasificar()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacion(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 5, 0), "in-1"),
            CreateMarcacion(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 30, 0), "out-1"));

        await db.SaveChangesAsync();

        var correctionService = new RrhhMarcacionZonaHorariaService();
        var resultado = await correctionService.CorregirMarcacionesGuardadasComoHoraLocalAsync(db, new RrhhMarcacionZonaHorariaCorrectionRequest
        {
            EmpresaId = empresa.Id,
            ChecadorId = checador.Id,
            FechaDesde = new DateOnly(2026, 1, 5),
            FechaHasta = new DateOnly(2026, 1, 5)
        });

        Assert.Equal(2, resultado.MarcacionesEncontradas);
        Assert.Equal(2, resultado.MarcacionesCorregidas);

        var marcaciones = await db.RrhhMarcaciones.OrderBy(m => m.FechaHoraMarcacionUtc).ToListAsync();
        Assert.Equal(new DateTime(2026, 1, 5, 14, 5, 0, DateTimeKind.Utc), marcaciones[0].FechaHoraMarcacionUtc);
        Assert.Equal(new DateTime(2026, 1, 5, 23, 30, 0, DateTimeKind.Utc), marcaciones[1].FechaHoraMarcacionUtc);
        Assert.All(marcaciones, m => Assert.False(m.Procesada));

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(new TimeSpan(8, 5, 0), asistencia.HoraEntradaReal);
        Assert.Equal(new TimeSpan(17, 30, 0), asistencia.HoraSalidaReal);
        Assert.Equal(RrhhAsistenciaEstatus.Retardo, asistencia.Estatus);
    }

    [Fact]
    public async Task ReconstruirHoraLocalDesdeUtcAsync_RecalculaHoraLocalSinModificarUtc()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            new RrhhMarcacion
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresa.Id,
                ChecadorId = checador.Id,
                EmpleadoId = empleado.Id,
                CodigoChecador = empleado.CodigoChecador!,
                FechaHoraMarcacionUtc = new DateTime(2026, 1, 5, 14, 5, 0, DateTimeKind.Utc),
                TipoMarcacionRaw = "0",
                Origen = "Test",
                EventoIdExterno = "rebuild-in-1",
                HashUnico = Guid.NewGuid().ToString("N"),
                Procesada = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new RrhhMarcacion
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresa.Id,
                ChecadorId = checador.Id,
                EmpleadoId = empleado.Id,
                CodigoChecador = empleado.CodigoChecador!,
                FechaHoraMarcacionUtc = new DateTime(2026, 1, 5, 23, 30, 0, DateTimeKind.Utc),
                TipoMarcacionRaw = "1",
                Origen = "Test",
                EventoIdExterno = "rebuild-out-1",
                HashUnico = Guid.NewGuid().ToString("N"),
                Procesada = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });

        await db.SaveChangesAsync();

        var correctionService = new RrhhMarcacionZonaHorariaService();
        var resultado = await correctionService.ReconstruirHoraLocalDesdeUtcAsync(db, new RrhhMarcacionZonaHorariaCorrectionRequest
        {
            EmpresaId = empresa.Id,
            ChecadorId = checador.Id,
            FechaDesde = new DateOnly(2026, 1, 5),
            FechaHasta = new DateOnly(2026, 1, 5)
        });

        Assert.Equal(2, resultado.MarcacionesEncontradas);
        Assert.Equal(2, resultado.MarcacionesCorregidas);

        var marcaciones = await db.RrhhMarcaciones.OrderBy(m => m.FechaHoraMarcacionUtc).ToListAsync();
        Assert.Equal(new DateTime(2026, 1, 5, 14, 5, 0, DateTimeKind.Utc), marcaciones[0].FechaHoraMarcacionUtc);
        Assert.Equal(new DateTime(2026, 1, 5, 23, 30, 0, DateTimeKind.Utc), marcaciones[1].FechaHoraMarcacionUtc);
        Assert.Equal(new DateTime(2026, 1, 5, 8, 5, 0), marcaciones[0].FechaHoraMarcacionLocal);
        Assert.Equal(new DateTime(2026, 1, 5, 17, 30, 0), marcaciones[1].FechaHoraMarcacionLocal);
        Assert.All(marcaciones, m => Assert.Equal("America/Mexico_City", m.ZonaHorariaAplicada));
        Assert.All(marcaciones, m => Assert.False(m.Procesada));
    }

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

    private static TurnoBase CreateTurno(Guid empresaId)
    {
        var turno = new TurnoBase
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            Nombre = "Matutino",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        turno.Detalles.Add(new TurnoBaseDetalle
        {
            Id = Guid.NewGuid(),
            TurnoBaseId = turno.Id,
            DiaSemana = DiaSemanaTurno.Lunes,
            Labora = true,
            HoraEntrada = new TimeSpan(8, 0, 0),
            HoraSalida = new TimeSpan(17, 0, 0),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        return turno;
    }

    private static RrhhChecador CreateChecador(Guid empresaId) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Nombre = "Checador Test",
        NumeroSerie = Guid.NewGuid().ToString("N"),
        ZonaHoraria = "America/Mexico_City",
        Ip = "127.0.0.1",
        Puerto = 4370,
        NumeroMaquina = 1,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static Empleado CreateEmpleado(Guid empresaId, Guid turnoId) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Codigo = "EMP-001",
        NumeroEmpleado = "001",
        Nombre = "Empleado Test",
        CodigoChecador = "3001",
        TurnoBaseId = turnoId,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static RrhhMarcacion CreateMarcacion(Guid empresaId, Guid checadorId, Empleado empleado, DateTime fechaHoraCapturada, string eventoId) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        ChecadorId = checadorId,
        EmpleadoId = empleado.Id,
        CodigoChecador = empleado.CodigoChecador!,
        FechaHoraMarcacionUtc = fechaHoraCapturada,
        TipoMarcacionRaw = "0",
        Origen = "Test",
        EventoIdExterno = eventoId,
        HashUnico = Guid.NewGuid().ToString("N"),
        Procesada = true,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };
}
