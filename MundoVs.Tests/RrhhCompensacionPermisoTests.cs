using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

// Fase 6 — compensación de permiso estructurada (columna RrhhAsistencia.MinutosCompensacionPermisoAprobados).
// Cubre: backfill one-shot desde bitácora legado (+ idempotencia + filtro empresa), reproceso
// preserva la columna (overlay del operador, no del processor) y las fórmulas del policy leen
// la columna para visible / faltante descontable / permiso sugerido.
public sealed class RrhhCompensacionPermisoTests
{
    [Fact]
    public async Task BackfillCompensacion_SiembraColumnaDesdeBitacora_YEsIdempotente()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var empleado = CreateEmpleado(empresa.Id);
        var fecha = new DateOnly(2026, 1, 5);

        db.Empresas.Add(empresa);
        db.Empleados.Add(empleado);
        db.RrhhAsistencias.Add(CreateAsistencia(empresa.Id, empleado.Id, fecha, minutosCompensacionPermisoAprobados: 0));
        db.RrhhLogsChecador.Add(CreateLogCompensacion(empresa.Id, empleado.Id, fecha, minutosCompensados: 45));
        await db.SaveChangesAsync();

        var service = new RrhhTiempoExtraResolutionService();
        var primera = await service.BackfillCompensacionDesdeBitacoraAsync(db, empresa.Id, "tester");
        await db.SaveChangesAsync();

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(45, asistencia.MinutosCompensacionPermisoAprobados);
        Assert.Equal(1, primera.FilasActualizadas);
        Assert.Equal(0, primera.FilasOmitidas);

        // Segunda corrida: la columna ya refleja el valor del bitácora → no cambia nada.
        var segunda = await service.BackfillCompensacionDesdeBitacoraAsync(db, empresa.Id, "tester");
        Assert.Equal(0, segunda.FilasActualizadas);
        Assert.Equal(1, segunda.FilasOmitidas);
        Assert.Equal(45, (await db.RrhhAsistencias.SingleAsync()).MinutosCompensacionPermisoAprobados);
    }

    [Fact]
    public async Task BackfillCompensacion_FiltroEmpresa_SoloProcesaEsaEmpresa()
    {
        await using var db = CreateDbContext();
        var empresaA = CreateEmpresa();
        var empresaB = CreateEmpresa();
        var empleadoA = CreateEmpleado(empresaA.Id);
        var empleadoB = CreateEmpleado(empresaB.Id);
        var fecha = new DateOnly(2026, 1, 6);

        db.Empresas.AddRange(empresaA, empresaB);
        db.Empleados.AddRange(empleadoA, empleadoB);
        db.RrhhAsistencias.Add(CreateAsistencia(empresaA.Id, empleadoA.Id, fecha, 0));
        db.RrhhAsistencias.Add(CreateAsistencia(empresaB.Id, empleadoB.Id, fecha, 0));
        db.RrhhLogsChecador.Add(CreateLogCompensacion(empresaA.Id, empleadoA.Id, fecha, 30));
        db.RrhhLogsChecador.Add(CreateLogCompensacion(empresaB.Id, empleadoB.Id, fecha, 60));
        await db.SaveChangesAsync();

        var service = new RrhhTiempoExtraResolutionService();
        var resultado = await service.BackfillCompensacionDesdeBitacoraAsync(db, empresaA.Id, "tester");
        await db.SaveChangesAsync();

        var a = await db.RrhhAsistencias.SingleAsync(x => x.EmpresaId == empresaA.Id);
        var b = await db.RrhhAsistencias.SingleAsync(x => x.EmpresaId == empresaB.Id);
        Assert.Equal(30, a.MinutosCompensacionPermisoAprobados);
        Assert.Equal(0, b.MinutosCompensacionPermisoAprobados); // no tocada por el filtro
        Assert.Equal(1, resultado.FilasActualizadas);
        Assert.Equal(0, resultado.FilasOmitidas); // B quedó fuera del filtro (no cargada → no omitida)
    }

    [Fact]
    public async Task ReprocesarRango_PreservaColumnaCompensacionAprobada_NoLaResetea()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurno(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id);
        var fecha = new DateOnly(2026, 1, 5); // lunes

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        empleado.TurnoBaseId = turno.Id;
        db.Empleados.Add(empleado);
        // Asistencia pre-existente con compensación aprobada por el operador (overlay Fase 6).
        db.RrhhAsistencias.Add(CreateAsistencia(empresa.Id, empleado.Id, fecha, minutosCompensacionPermisoAprobados: 45));
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 20, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));
        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ReprocesarRangoAsync(db, empresa.Id, fecha, fecha);
        await db.SaveChangesAsync();

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(45, asistencia.MinutosCompensacionPermisoAprobados);
    }

    [Fact]
    public void Policy_VisibleYFaltanteConsumenColumnaDeCompensacion()
    {
        var asistencia = CreateAsistenciaConTurno(
            minutosJornadaNetaProgramada: 480,
            minutosTrabajadosNetos: 420,
            minutosCompensacionPermisoAprobados: 30);

        var visible = RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(asistencia, minutosPermisoConGoceDia: 0, asistencia.MinutosCompensacionPermisoAprobados);
        var faltanteDescontable = RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteDescontable(asistencia, minutosPermisoAplicados: 0, asistencia.MinutosCompensacionPermisoAprobados);
        var permisoSugerido = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoSugeridos(asistencia, asistencia.MinutosCompensacionPermisoAprobados);

        // base visible = min(420 - 0 extra, 480) = 420; visible = 420 + 0 permiso + 30 comp + 0 extra = 450.
        Assert.Equal(450, visible);
        // faltante banco = 480 - 420 = 60; descontable = max(0, 60 - 0 - 30) = 30.
        Assert.Equal(30, faltanteDescontable);
        // permiso sugerido = max(0, 60 - 30) = 30.
        Assert.Equal(30, permisoSugerido);

        // Sin compensación (columna en 0) el faltante descontable vuelve a 60 y visible a 420.
        var sinComp = CreateAsistenciaConTurno(480, 420, 0);
        Assert.Equal(60, RrhhTiempoExtraPolicy.ObtenerMinutosFaltanteDescontable(sinComp, 0, 0));
        Assert.Equal(420, RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(sinComp, 0, 0));
    }

    [Fact]
    public async Task BackfillCompensacion_SinBitacora_NoHaceNada()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var empleado = CreateEmpleado(empresa.Id);
        var fecha = new DateOnly(2026, 1, 7);

        db.Empresas.Add(empresa);
        db.Empleados.Add(empleado);
        db.RrhhAsistencias.Add(CreateAsistencia(empresa.Id, empleado.Id, fecha, 0));
        await db.SaveChangesAsync();

        var service = new RrhhTiempoExtraResolutionService();
        var resultado = await service.BackfillCompensacionDesdeBitacoraAsync(db, empresa.Id, "tester");

        Assert.Equal(0, resultado.FilasActualizadas);
        Assert.Equal(0, resultado.FilasOmitidas);
        Assert.Equal(0, (await db.RrhhAsistencias.SingleAsync()).MinutosCompensacionPermisoAprobados);
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

    private static Empleado CreateEmpleado(Guid empresaId) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Codigo = "EMP-001",
        NumeroEmpleado = "001",
        Nombre = "Empleado Test",
        CodigoChecador = "3001",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static RrhhAsistencia CreateAsistencia(Guid empresaId, Guid empleadoId, DateOnly fecha, int minutosCompensacionPermisoAprobados) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        EmpleadoId = empleadoId,
        Fecha = fecha,
        TurnoBaseId = Guid.NewGuid(),
        HoraEntradaProgramada = new TimeSpan(8, 0, 0),
        HoraSalidaProgramada = new TimeSpan(17, 0, 0),
        MinutosJornadaNetaProgramada = 480,
        MinutosTrabajadosNetos = 420,
        MinutosCompensacionPermisoAprobados = minutosCompensacionPermisoAprobados,
        ModoSugerenciaExtra = "EntradaSalida",
        Estatus = RrhhAsistenciaEstatus.Pendiente,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static RrhhAsistencia CreateAsistenciaConTurno(int minutosJornadaNetaProgramada, int minutosTrabajadosNetos, int minutosCompensacionPermisoAprobados) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = Guid.NewGuid(),
        EmpleadoId = Guid.NewGuid(),
        Fecha = new DateOnly(2026, 1, 5),
        TurnoBaseId = Guid.NewGuid(),
        MinutosJornadaNetaProgramada = minutosJornadaNetaProgramada,
        MinutosTrabajadosNetos = minutosTrabajadosNetos,
        MinutosExtra = 0,
        MinutosExtraAutorizadosPago = 0,
        MinutosExtraAutorizadosBanco = 0,
        MinutosCubiertosBancoHoras = 0,
        MinutosCompensacionPermisoAprobados = minutosCompensacionPermisoAprobados,
        ModoSugerenciaExtra = "EntradaSalida",
        Estatus = RrhhAsistenciaEstatus.Pendiente,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    private static RrhhLogChecador CreateLogCompensacion(Guid empresaId, Guid empleadoId, DateOnly fecha, int minutosCompensados) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        FechaUtc = new DateTime(fecha, new TimeOnly(15, 0, 0), DateTimeKind.Utc),
        Nivel = "Info",
        Mensaje = "Se aplicó corrección de asistencia: compensación aprobada de permiso.",
        Detalle = $"empleado={empleadoId};fecha={fecha:yyyy-MM-dd};minutosCompensados={minutosCompensados};permisoActual=0;saldoBanco=0;usuario=tester",
        CreatedAt = DateTime.UtcNow,
        IsActive = true
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
            CantidadDescansos = 0,
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

    private static RrhhMarcacion CreateMarcacionLocal(Guid empresaId, Guid checadorId, Empleado empleado, DateTime fechaHoraLocal, string eventoId, TipoClasificacionMarcacionRrhh clasificacion = TipoClasificacionMarcacionRrhh.Entrada) => new()
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
        Origen = "Test",
        EventoIdExterno = eventoId,
        HashUnico = Guid.NewGuid().ToString("N"),
        ClasificacionOperativa = clasificacion,
        Procesada = false,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };
}