using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;
using Zenith.Contracts.Asistencia;

namespace MundoVs.Tests;

public sealed class RrhhMarcacionIngestionServiceTests
{
    [Fact]
    public async Task IngerirLoteAsync_CuentaNuevasDuplicadasYFallidasCorrectamente()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, "1001");

        db.Empresas.Add(empresa);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        await db.SaveChangesAsync();

        var service = new RrhhMarcacionIngestionService();
        var fecha = new DateTime(2026, 1, 5, 8, 0, 0, DateTimeKind.Utc);

        var batch = new MarcacionSyncBatchDto
        {
            EmpresaId = empresa.Id,
            ChecadorId = checador.Id,
            Marcaciones =
            [
                new MarcacionRawDto
                {
                    EmpresaId = empresa.Id,
                    ChecadorId = checador.Id,
                    CodigoChecador = "1001",
                    FechaHoraMarcacionUtc = fecha,
                    TipoMarcacionRaw = "0",
                    Origen = "Test",
                    EventoIdExterno = "evt-1",
                    PayloadRaw = "ok"
                },
                new MarcacionRawDto
                {
                    EmpresaId = empresa.Id,
                    ChecadorId = checador.Id,
                    CodigoChecador = "1001",
                    FechaHoraMarcacionUtc = fecha,
                    TipoMarcacionRaw = "0",
                    Origen = "Test",
                    EventoIdExterno = "evt-1",
                    PayloadRaw = "ok"
                },
                new MarcacionRawDto
                {
                    EmpresaId = empresa.Id,
                    ChecadorId = checador.Id,
                    CodigoChecador = "9999",
                    FechaHoraMarcacionUtc = fecha.AddHours(1),
                    TipoMarcacionRaw = "1",
                    Origen = "Test",
                    EventoIdExterno = "evt-2",
                    PayloadRaw = "unknown"
                },
                new MarcacionRawDto
                {
                    EmpresaId = empresa.Id,
                    ChecadorId = checador.Id,
                    CodigoChecador = " ",
                    FechaHoraMarcacionUtc = fecha.AddHours(2),
                    TipoMarcacionRaw = "1",
                    Origen = "Test",
                    EventoIdExterno = "evt-3"
                }
            ]
        };

        var result = await service.IngerirLoteAsync(db, batch);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Response.Leidas);
        Assert.Equal(2, result.Response.Enviadas);
        Assert.Equal(1, result.Response.Duplicadas);
        Assert.Equal(1, result.Response.Fallidas);

        var marcaciones = await db.RrhhMarcaciones.OrderBy(x => x.CodigoChecador).ToListAsync();
        Assert.Equal(2, marcaciones.Count);
        Assert.Equal(empleado.Id, marcaciones.Single(x => x.CodigoChecador == "1001").EmpleadoId);
        Assert.Null(marcaciones.Single(x => x.CodigoChecador == "9999").EmpleadoId);
        Assert.Equal(fecha, marcaciones.Single(x => x.CodigoChecador == "1001").FechaHoraMarcacionLocal);
        Assert.Equal("UTC", marcaciones.Single(x => x.CodigoChecador == "1001").ZonaHorariaAplicada);

        var log = await db.RrhhLogsChecador.SingleAsync();
        Assert.Equal("Warning", log.Nivel);
        Assert.Contains("fallidas: 1", log.Mensaje, StringComparison.OrdinalIgnoreCase);

        var checadorActualizado = await db.RrhhChecadores.SingleAsync();
        Assert.NotNull(checadorActualizado.UltimaSincronizacionUtc);
        Assert.Equal("evt-2", checadorActualizado.UltimoEventoLeido);
    }

    [Fact]
    public async Task IngerirLoteAsync_MarcaComoDuplicadaUnaMarcacionYaPersistida()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var checador = CreateChecador(empresa.Id);

        db.Empresas.Add(empresa);
        db.RrhhChecadores.Add(checador);
        await db.SaveChangesAsync();

        var service = new RrhhMarcacionIngestionService();
        var batch = new MarcacionSyncBatchDto
        {
            EmpresaId = empresa.Id,
            ChecadorId = checador.Id,
            Marcaciones =
            [
                new MarcacionRawDto
                {
                    EmpresaId = empresa.Id,
                    ChecadorId = checador.Id,
                    CodigoChecador = "2001",
                    FechaHoraMarcacionUtc = new DateTime(2026, 1, 5, 8, 0, 0, DateTimeKind.Utc),
                    TipoMarcacionRaw = "0",
                    Origen = "Test",
                    EventoIdExterno = "evt-dup",
                    PayloadRaw = "dup"
                }
            ]
        };

        var first = await service.IngerirLoteAsync(db, batch);
        var second = await service.IngerirLoteAsync(db, batch);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(1, second.Response.Leidas);
        Assert.Equal(0, second.Response.Enviadas);
        Assert.Equal(1, second.Response.Duplicadas);
        Assert.Equal(0, second.Response.Fallidas);
        Assert.Equal(1, await db.RrhhMarcaciones.CountAsync());
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

    private static RrhhChecador CreateChecador(Guid empresaId) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Nombre = "Checador Test",
        NumeroSerie = Guid.NewGuid().ToString("N"),
        ZonaHoraria = "UTC",
        Ip = "127.0.0.1",
        Puerto = 4370,
        NumeroMaquina = 1,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static Empleado CreateEmpleado(Guid empresaId, string codigoChecador) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Codigo = $"COD-{codigoChecador}",
        NumeroEmpleado = codigoChecador,
        Nombre = "Empleado",
        CodigoChecador = codigoChecador,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };
}
