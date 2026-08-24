using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Models;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

/// <summary>
/// Tests del modo "Marcaje de Reloj" (antes NetoVsNeto), rediseñados desde cero
/// a partir de la especificación canónica 2026-07-27:
/// - Parte del reloj tal cual.
/// - Sin retardo/salida anticipada (no usa el turno para reglas de entrada/salida).
/// - El extra SÍ respeta el umbral mínimo (MinutosMinimosTiempoExtra, default 15):
///   excedente < umbral → 0; excedente ≥ umbral → todo el excedente (igual que EntradaSalida).
/// - Pausa = par intermedio real; por defecto se descuenta.
/// - Sin par intermedio = trabajo continuo; el descanso planeado no marcado NO se descuenta.
/// - Extra = Max(0, Trabajado - Planeado) con umbral mínimo.
/// - Tiempo Acreditado = Min(Trabajado + Permiso, Planeado).
/// - Extra aprobado es semanal (se prueba en RrhhAsistenciaNeteoSemanalTests).
/// </summary>
public sealed class RrhhAsistenciaMarcajeRelojTests
{
    [Fact]
    public async Task SinParIntermedio_DescansoNoMarcado_NoSeDescuenta_Extra18_Acreditado435()
    {
        // Caso usuario: marco 11:10/18:43, turno 11:30-19:00, D1 14:00-14:15 no pagado.
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = new TurnoBase
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            Nombre = "11:30-19:00",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        turno.Detalles.Add(new TurnoBaseDetalle
        {
            Id = Guid.NewGuid(),
            TurnoBaseId = turno.Id,
            DiaSemana = DiaSemanaTurno.Lunes,
            Labora = true,
            HoraEntrada = new TimeSpan(11, 30, 0),
            HoraSalida = new TimeSpan(19, 0, 0),
            CantidadDescansos = 1,
            Descanso1Inicio = new TimeSpan(14, 0, 0),
            Descanso1Fin = new TimeSpan(14, 15, 0),
            Descanso1EsPagado = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhAsistencias.Add(new RrhhAsistencia
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            Fecha = new DateOnly(2026, 1, 5),
            ModoSugerenciaExtra = "MarcajeReloj",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 11, 10, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 18, 43, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));
        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ReprocesarRangoAsync(db, empresa.Id, new DateOnly(2026, 1, 5), new DateOnly(2026, 1, 5), empleado.Id);

        var a = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(453, a.MinutosTrabajadosNetos);        // 18:43-11:10
        Assert.Equal(435, a.MinutosJornadaNetaProgramada);  // 480 - 45
        Assert.Equal(0, a.MinutosDescansoNoPagado);         // no marcado -> no descuenta
        Assert.Equal(18, a.MinutosExtra);                    // 453 - 435
        Assert.Equal(435, RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(a, 0)); // Acreditado
        Assert.Equal(0, a.MinutosRetardo);                   // modo no usa turno
        Assert.Equal(0, a.MinutosSalidaAnticipada);          // modo no usa turno
    }

    [Fact]
    public async Task ExcedenteMenorAlUmbral_NoCuentaComoExtra_Da0()
    {
        // Jornada 11:30-19:00, D1 14:00-14:15 NO pagado → neta 435. Sin par intermedio →
        // el descanso no marcado NO se descuenta. Marcas 11:20/18:45 → neto 445, excedente 10.
        // 10 < umbral 15 → NO cuenta como extra (antes, sin umbral, daba 10). El tiempo
        // acreditado queda en Min(445, 435) = 435.
        // English: 11:30-19:00 shift, unpaid D1 14:00-14:15 → net 435. No intermediate pair →
        // unmarked break is NOT deducted. Marks 11:20/18:45 → net 445, surplus 10. 10 < threshold
        // 15 → does NOT count as extra (previously, without threshold, it gave 10). Credited time
        // stays at Min(445, 435) = 435.
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = new TurnoBase
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            Nombre = "11:30-19:00",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        turno.Detalles.Add(new TurnoBaseDetalle
        {
            Id = Guid.NewGuid(),
            TurnoBaseId = turno.Id,
            DiaSemana = DiaSemanaTurno.Lunes,
            Labora = true,
            HoraEntrada = new TimeSpan(11, 30, 0),
            HoraSalida = new TimeSpan(19, 0, 0),
            CantidadDescansos = 1,
            Descanso1Inicio = new TimeSpan(14, 0, 0),
            Descanso1Fin = new TimeSpan(14, 15, 0),
            Descanso1EsPagado = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhAsistencias.Add(new RrhhAsistencia
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            Fecha = new DateOnly(2026, 1, 5),
            ModoSugerenciaExtra = "MarcajeReloj",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 11, 20, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 18, 45, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));
        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ReprocesarRangoAsync(db, empresa.Id, new DateOnly(2026, 1, 5), new DateOnly(2026, 1, 5), empleado.Id);

        var a = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(445, a.MinutosTrabajadosNetos);        // 18:45-11:20
        Assert.Equal(435, a.MinutosJornadaNetaProgramada);   // 480 - 45
        Assert.Equal(0, a.MinutosDescansoNoPagado);         // no marcado -> no descuenta
        Assert.Equal(0, a.MinutosExtra);                    // excedente 10 < umbral 15 -> 0
        Assert.Equal(435, RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(a, 0)); // Acreditado Min(445,435)
    }

    [Fact]
    public async Task ParIntermedio_SinClasificar_CuentaComoPausa_Real_Descuenta44_Extra58()
    {
        // Aralim: D1 marcado (27 min), D2 como par intermedio SinClasificar (44 min).
        // El par intermedio por defecto es pausa -> se descuenta la duracion real.
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = new TurnoBase
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            Nombre = "Aralim 08-1815",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        turno.Detalles.Add(new TurnoBaseDetalle
        {
            Id = Guid.NewGuid(),
            TurnoBaseId = turno.Id,
            DiaSemana = DiaSemanaTurno.Martes,
            Labora = true,
            HoraEntrada = new TimeSpan(8, 0, 0),
            HoraSalida = new TimeSpan(18, 15, 0),
            CantidadDescansos = 2,
            Descanso1Inicio = new TimeSpan(10, 0, 0),
            Descanso1Fin = new TimeSpan(10, 30, 0),
            Descanso1EsPagado = false,
            Descanso2Inicio = new TimeSpan(14, 0, 0),
            Descanso2Fin = new TimeSpan(14, 45, 0),
            Descanso2EsPagado = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhAsistencias.Add(new RrhhAsistencia
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            Fecha = new DateOnly(2026, 7, 21),
            ModoSugerenciaExtra = "MarcajeReloj",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 7, 21, 7, 9, 0), "in-1", TipoClasificacionMarcacionRrhh.Entrada),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 7, 21, 11, 3, 0), "break-1-out", TipoClasificacionMarcacionRrhh.InicioDescanso),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 7, 21, 11, 30, 0), "break-1-in", TipoClasificacionMarcacionRrhh.FinDescanso),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 7, 21, 14, 0, 0), "unc-1", TipoClasificacionMarcacionRrhh.SinClasificar),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 7, 21, 14, 44, 0), "unc-2", TipoClasificacionMarcacionRrhh.SinClasificar),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 7, 21, 18, 18, 0), "unc-3", TipoClasificacionMarcacionRrhh.SinClasificar));
        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ReprocesarRangoAsync(db, empresa.Id, new DateOnly(2026, 7, 21), new DateOnly(2026, 7, 21), empleado.Id);

        var a = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal(598, a.MinutosTrabajadosNetos);   // 669 - 27 - 44
        Assert.Equal(58, a.MinutosExtra);               // 598 - 540
        Assert.Equal(71, a.MinutosDescansoTomado);      // pausas reales totales
    }

    [Fact]
    public async Task PermisoCubreFaltante_Acreditado530_Faltante10()
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
        db.RrhhAsistencias.Add(new RrhhAsistencia
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            Fecha = new DateOnly(2026, 1, 5),
            ModoSugerenciaExtra = "MarcajeReloj",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 10, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 16, 50, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));
        var permiso = new RrhhAusencia
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            Tipo = TipoAusenciaRrhh.Permiso,
            Estatus = EstatusAusenciaRrhh.Aplicada,
            FechaInicio = new DateOnly(2026, 1, 5),
            FechaFin = new DateOnly(2026, 1, 5),
            Dias = 1,
            Horas = 0.5m, // 30 min
            ConGocePago = true,
            Motivo = "Permiso parcial",
            FechaAprobacion = DateTime.UtcNow,
            AprobadoPor = "tester",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        db.RrhhAusencias.Add(permiso);
        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ReprocesarRangoAsync(db, empresa.Id, new DateOnly(2026, 1, 5), new DateOnly(2026, 1, 5), empleado.Id);

        var a = await db.RrhhAsistencias.SingleAsync();
        var permisoDia = RrhhTiempoExtraPolicy.ObtenerMinutosPermisoConGocePorDia(permiso);
        // Bruto = 8:10 -> 16:50 = 520 min. Planeado = 540. Permiso = 30.
        // Faltante = 540 - 520 = 20. El permiso cubre esos 20 y sobran 10 (no se pagan).
        // Tiempo Acreditado = Min(520 + 30, 540) = 540. No hay faltante remanente.
        Assert.Equal(520, a.MinutosTrabajadosNetos);
        Assert.Equal(540, a.MinutosJornadaNetaProgramada);
        Assert.Equal(0, a.MinutosExtra);                              // no excede
        Assert.Equal(30, permisoDia);
        Assert.Equal(540, RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(a, permisoDia, 0)); // Min(Trabajado+Permiso, Planeado)
    }

    [Fact]
    public async Task DescansoPagado_NoSeDescuenta_TrabajadoIncluyePausa()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = new TurnoBase
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            Nombre = "Con descanso pagado",
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
            CantidadDescansos = 1,
            Descanso1Inicio = new TimeSpan(12, 0, 0),
            Descanso1Fin = new TimeSpan(13, 0, 0),
            Descanso1EsPagado = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhAsistencias.Add(new RrhhAsistencia
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            Fecha = new DateOnly(2026, 1, 5),
            ModoSugerenciaExtra = "MarcajeReloj",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 8, 0, 0), "in-1"),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 12, 0, 0), "break-out", TipoClasificacionMarcacionRrhh.InicioDescanso),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 13, 0, 0), "break-in", TipoClasificacionMarcacionRrhh.FinDescanso),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 17, 0, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));
        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ReprocesarRangoAsync(db, empresa.Id, new DateOnly(2026, 1, 5), new DateOnly(2026, 1, 5), empleado.Id);

        var a = await db.RrhhAsistencias.SingleAsync();
        // Bruto = 9h = 540. Pausa pagada no descuenta. Planeado = 480 (540 - 60 descanso pagado?)
        // Planeado = JornadaNeta: jornada bruta 9h (540) - descanso no pagado (0, porque es pagado) = 540.
        Assert.Equal(540, a.MinutosTrabajadosNetos);
        Assert.Equal(540, a.MinutosJornadaNetaProgramada);
        Assert.Equal(0, a.MinutosDescansoNoPagado);
        Assert.Equal(0, a.MinutosExtra);
    }

    // helpers (copia simplificada de RrhhAsistenciaProcessorTests)
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

    private static RrhhMarcacion CreateMarcacionLocal(Guid empresaId, Guid checadorId, Empleado empleado, DateTime fechaHoraLocal, string eventoId, TipoClasificacionMarcacionRrhh clasificacion = TipoClasificacionMarcacionRrhh.Entrada, string? payloadRaw = null) => new()
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
        PayloadRaw = payloadRaw,
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };
}
