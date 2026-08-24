using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Models;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

/// <summary>
/// Caso concreto del usuario (sesión de revisión de lógica de descuento):
///   Marcajes: 11:10 (entrada) y 18:43 (salida) — sólo 2 marcas, descanso NO marcado.
///   Turno:    11:30 → 19:00 (JornadaProg = 450 min).
///   Descanso: D1 14:00 → 14:15 (15 min, NO pagado).
///   Umbral MinutosMinimosTiempoExtra = 15 (default de NominaConfiguracion).
///
/// Cubre las 4 combinaciones (modo × no-descontar D1) para mostrar el ORDEN en que
/// el procesador decide el descuento y el extra. Ver cabecera de cada test.
/// </summary>
public sealed class RrhhAsistenciaCasoUsuarioTests
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

    // Turno 11:30 → 19:00 con D1 14:00 → 14:15 (no pagado). Día Lunes.
    private static TurnoBase CreateTurnoUsuario(Guid empresaId)
    {
        var turno = new TurnoBase
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            Nombre = "Tarde",
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

    private static RrhhMarcacion CreateMarcacionLocal(Guid empresaId, Guid checadorId, Empleado empleado, DateTime fechaHoraLocal, string eventoId, TipoClasificacionMarcacionRrhh clasificacion) => new()
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

    private static async Task<(CrmDbContext db, Empresa empresa, Empleado empleado, DateOnly fecha)> SembrarCasoBaseAsync(string? modoDefault = null)
    {
        var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurnoUsuario(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);
        // Método de cálculo por defecto del empleado (null = Vs horario, "MarcajeReloj" = tal
        // cual reloj). English: employee default calc method (null = Vs schedule, "MarcajeReloj"
        // = clock punch as-is).
        empleado.ModoSugerenciaExtraDefault = modoDefault;

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 11, 10, 0), "in-1", TipoClasificacionMarcacionRrhh.Entrada),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 18, 43, 0), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);
        await db.SaveChangesAsync();

        return (db, empresa, empleado, new DateOnly(2026, 1, 5));
    }

    private static async Task<RrhhAsistencia> ReprocesarAsync(CrmDbContext db, Guid empresaId, DateOnly fecha)
    {
        var processor = new RrhhAsistenciaProcessor();
        await processor.ReprocesarRangoAsync(db, empresaId, fecha, fecha);
        await db.SaveChangesAsync();
        return await db.RrhhAsistencias.SingleAsync();
    }

    // ───────────────────────────────────────────────────────────────────────
    // Combo 1: modo DEFAULT (EntradaSalida, sin recalcular) + D1 NO no-descontar.
    // D1 no marcado → se aplica el descanso programado de 15 min.
    // Extra auto = excedente neto (438 − 435 = 3) ≤ tolerancia 15 → 0.
    // Esperado: Neto = 438, Extra = 0, DescansoNoPagado = 15.
    // ───────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CasoUsuario_DefaultSinNoDescontar_DescuentaProgramadoYSinExtra()
    {
        var (db, empresa, _, fecha) = await SembrarCasoBaseAsync();
        var a = await ReprocesarAsync(db, empresa.Id, fecha);

        Assert.Equal(450, a.MinutosJornadaProgramada);
        Assert.Equal(435, a.MinutosJornadaNetaProgramada); // 450 − 15 (D1 no pagado)
        Assert.Equal(453, a.MinutosTrabajadosBrutos);      // 18:43 − 11:10 = 453
        Assert.Equal(15, a.MinutosDescansoNoPagado);      // D1 programado aplicado
        Assert.Equal(438, a.MinutosTrabajadosNetos);       // 453 − 15
        Assert.Equal(0, a.MinutosExtra);                  // excedente 3 ≤ umbral 15
        Assert.Null(a.ModoSugerenciaExtra);               // nunca se persistió modo
    }

    // ───────────────────────────────────────────────────────────────────────
    // Combo 2: modo MarcajeReloj + D1 NO marcado + sin no-descontar.  ← el fix.
    // En MarcajeReloj el descanso NO marcado no se descuenta (el reloj dice que se trabajó).
    // El extra SÍ respeta el umbral: 18 ≥ 15 → cuenta.
    // Esperado: Neto = 453 (suma de segmentos, sin descuento), Extra = 18, visible 7:15.
    // No hace falta activar no-descontar; el modo basta.
    // ───────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CasoUsuario_MarcajeRelojSinNoDescontar_DescansoNoMarcadoNoSeDescuenta()
    {
        var (db, empresa, _, fecha) = await SembrarCasoBaseAsync();

        // Simula "Recalcular este día" con MarcajeReloj (sin tocar no-descontar).
        var asistencia = await db.RrhhAsistencias.SingleAsync();
        asistencia.ModoSugerenciaExtra = "MarcajeReloj";
        await db.SaveChangesAsync();

        var a = await ReprocesarAsync(db, empresa.Id, fecha);

        Assert.Equal("MarcajeReloj", a.ModoSugerenciaExtra);
        Assert.Equal(0, a.MinutosDescansoNoPagado); // D1 no marcado → no se descuenta en MarcajeReloj
        Assert.Equal(453, a.MinutosTrabajadosNetos); // suma de segmentos = bruto (sin descuento)
        Assert.Equal(18, a.MinutosExtra);            // 453 − 435 = 18 (≥ umbral 15 → cuenta)
        Assert.Equal(435, RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(a, 0, 0)); // 7:15
    }

    // ───────────────────────────────────────────────────────────────────────
    // Combo 3: modo DEFAULT (EntradaSalida) + D1 SÍ no-descontar.
    // D1 → rama (c) no-descontar → MinutosAplicados=0 → AutoDescuento=FALSE.
    // Cae a rama EntradaSalida (default): Extra = Max(0, Neto − JornadaNeta) = 453 − 435 = 18.
    // Esperado: Neto = 453 (sin descuento), Extra = 18, DescansoNoPagado = 0.
    // ───────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CasoUsuario_DefaultConNoDescontar_NoDescuentoYExtraDeNetoVsPlaneado()
    {
        var (db, empresa, _, fecha) = await SembrarCasoBaseAsync();

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        asistencia.DescansosNoDescontar = "1"; // no descontar D1
        await db.SaveChangesAsync();

        var a = await ReprocesarAsync(db, empresa.Id, fecha);

        Assert.Equal(0, a.MinutosDescansoNoPagado); // D1 no descontado
        Assert.Equal(453, a.MinutosTrabajadosNetos); // 453 sin descontar
        Assert.Equal(18, a.MinutosExtra);           // 453 − 435 = 18 (≥15)
    }

    // ───────────────────────────────────────────────────────────────────────
    // Combo 4: modo MarcajeReloj + D1 SÍ no-descontar.
    // Con el fix, el descanso no marcado ya no se descuenta en MarcajeReloj por el modo
    // mismo, así que activar no-descontar es redundante pero coherente: mismo resultado.
    // Extra = Max(0, Neto − JornadaNeta) = Max(0, 453 − 435) = 18 (≥ umbral 15 → cuenta).
    // Esperado: Neto = 453, Extra = 18, DescansoNoPagado = 0.
    // ───────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CasoUsuario_MarcajeRelojConNoDescontar_ElModoCorreYDaExtra18()
    {
        var (db, empresa, _, fecha) = await SembrarCasoBaseAsync();

        var asistencia = await db.RrhhAsistencias.SingleAsync();
        asistencia.ModoSugerenciaExtra = "MarcajeReloj";
        asistencia.DescansosNoDescontar = "1"; // no descontar D1
        await db.SaveChangesAsync();

        var a = await ReprocesarAsync(db, empresa.Id, fecha);

        Assert.Equal("MarcajeReloj", a.ModoSugerenciaExtra);
        Assert.Equal(0, a.MinutosDescansoNoPagado); // D1 no descontado
        Assert.Equal(453, a.MinutosTrabajadosNetos); // 453 sin descontar
        Assert.Equal(18, a.MinutosExtra);            // 453 − 435 = 18 (≥15)
    }

    // ───────────────────────────────────────────────────────────────────────
    // Rediseño EntradaSalida: ahora extra = Max(0, Neto − JornadaNeta) con umbral 15.
    // Marcajes con segundos (11:10:00 / 18:43:59) → bruto redondea a 454 (no 453).
    // JornadaNeta planeada = 450 − 15 = 435 (7:15). D1 no-descontar activo.
    //
    // Ambos modos (EntradaSalida y MarcajeReloj) ahora calculan el mismo extra neto:
    // Extra = Max(0, 454 − 435) = 19. Base = Min(454 − 19, 435) = 435 → visible = 7:15.
    // La antigua asimetría (EntradaSalida daba 7:14) desaparece.
    // ───────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task CasoUsuario_ConSegundos_AmbosModosDan715_PorCalculoNeto()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var turno = CreateTurnoUsuario(empresa.Id);
        var checador = CreateChecador(empresa.Id);
        var empleado = CreateEmpleado(empresa.Id, turno.Id);

        db.Empresas.Add(empresa);
        db.TurnosBase.Add(turno);
        db.RrhhChecadores.Add(checador);
        db.Empleados.Add(empleado);
        // 18:43:59 para que el bruto redondee a 454 (como en el reloj real con segundos).
        db.RrhhMarcaciones.AddRange(
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 11, 10, 0), "in-1", TipoClasificacionMarcacionRrhh.Entrada),
            CreateMarcacionLocal(empresa.Id, checador.Id, empleado, new DateTime(2026, 1, 5, 18, 43, 59), "out-1", TipoClasificacionMarcacionRrhh.Salida));

        await db.SaveChangesAsync();
        var fecha = new DateOnly(2026, 1, 5);

        var processor = new RrhhAsistenciaProcessor();
        await processor.ProcesarMarcacionesPendientesAsync(db, empresa.Id, checador.Id);
        await db.SaveChangesAsync();

        // --- Modo EntradaSalida (default) + no-descontar D1 ---
        var aDef = await db.RrhhAsistencias.SingleAsync();
        aDef.DescansosNoDescontar = "1";
        await db.SaveChangesAsync();
        aDef = await ReprocesarAsync(db, empresa.Id, fecha);

        Assert.Equal(454, aDef.MinutosTrabajadosBrutos);        // bruto redondeado con segundos
        Assert.Equal(435, aDef.MinutosJornadaNetaProgramada);    // jornada planeada = 7:15
        Assert.Equal(454, aDef.MinutosTrabajadosNetos);         // no-descontar → sin descuento
        Assert.Equal(19, aDef.MinutosExtra);                    // 454 − 435 = 19 (≥15)
        // Visible = Min(Neto − Extra, JornadaNeta) = Min(454−19, 435) = 435 = 7:15
        Assert.Equal(435, RrhhTiempoExtraPolicy.ObtenerMinutosBasePagada(aDef));
        Assert.Equal(435, RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(aDef, 0, 0));

        // --- Modo MarcajeReloj + no-descontar D1 ---
        aDef.ModoSugerenciaExtra = "MarcajeReloj";
        await db.SaveChangesAsync();
        aDef = await ReprocesarAsync(db, empresa.Id, fecha);

        Assert.Equal(19, aDef.MinutosExtra);                   // 454 − 435 = 19
        // Visible = Min(454 − 19, 435) = 435 = 7:15 (la jornada planeada)
        Assert.Equal(435, RrhhTiempoExtraPolicy.ObtenerMinutosBasePagada(aDef));
        Assert.Equal(435, RrhhTiempoExtraPolicy.ObtenerMinutosTiempoVisible(aDef, 0, 0));
    }

    // ───────────────────────────────────────────────────────────────────────
    // Método de cálculo por defecto por empleado (Marcaje de reloj vs Vs horario).
    // El empleado tiene un default en Empleado.ModoSugerenciaExtraDefault (null = Vs horario,
    // "MarcajeReloj" = tal cual reloj). El "Recalcular por periodo" (forzarDefaultEmpleado=true)
    // impone el default a todos los días y pisa overrides manuales; el recálculo por día /
    // incremental (forzar=false) preserva el override y usa el default como fallback.
    // English: per-employee default calc method. "Recalculate by period" (forzarDefaultEmpleado
    // =true) enforces the default on every day, overriding per-day overrides; per-day/incremental
    // recalc (forzar=false) preserves the override and uses the default as a fallback.
    // ───────────────────────────────────────────────────────────────────────
    private static async Task<RrhhAsistencia> ReprocesarForzandoAsync(CrmDbContext db, Guid empresaId, DateOnly fecha)
    {
        var processor = new RrhhAsistenciaProcessor();
        await processor.ReprocesarRangoAsync(db, empresaId, fecha, fecha, forzarDefaultEmpleado: true);
        await db.SaveChangesAsync();
        return await db.RrhhAsistencias.SingleAsync();
    }

    [Fact]
    public async Task EmpleadoDefaultMarcajeReloj_DiaNuevo_CalculaComoMarcajeReloj()
    {
        // Default MarcajeReloj; día nuevo (incremental, forzar=false, sin override) → el default
        // aplica como fallback y se calcula sin reglas. English: MarcajeReloj default; new day
        // (incremental, forzar=false, no override) → default applies as fallback, no rules.
        var (db, empresa, _, fecha) = await SembrarCasoBaseAsync(modoDefault: "MarcajeReloj");
        var a = await db.RrhhAsistencias.SingleAsync();

        Assert.Equal("MarcajeReloj", a.ModoSugerenciaExtra);
        Assert.Equal(0, a.MinutosRetardo);              // sin reglas
        Assert.Equal(0, a.MinutosSalidaAnticipada);     // sin reglas
        Assert.Equal(0, a.MinutosDescansoNoPagado);     // descanso no marcado no descuenta
        Assert.Equal(453, a.MinutosTrabajadosNetos);    // 18:43 − 11:10 = 453, sin descuento
        Assert.Equal(18, a.MinutosExtra);               // 453 − 435 = 18 (≥ umbral 15 → cuenta)
    }

    [Fact]
    public async Task EmpleadoDefaultNull_DiaNuevo_AplicaReglasEntradaSalida()
    {
        // Default null = "Vs horario": aplican salida anticipada, descanso no marcado, etc.
        // English: default null = "Vs schedule": early-leave, unmarked-break rules apply.
        var (db, empresa, _, fecha) = await SembrarCasoBaseAsync(modoDefault: null);
        var a = await db.RrhhAsistencias.SingleAsync();

        Assert.Null(a.ModoSugerenciaExtra);
        Assert.Equal(17, a.MinutosSalidaAnticipada);    // 19:00 − 18:43 = 17 (reglas)
        Assert.Equal(15, a.MinutosDescansoNoPagado);    // D1 no marcado descuenta
        Assert.Equal(438, a.MinutosTrabajadosNetos);    // 453 − 15
    }

    [Fact]
    public async Task ReprocesoPorPeriodo_ForzarDefault_PisaOverrideManual()
    {
        // Default null; override manual por día a "MarcajeReloj" (como hace el modal). Al
        // "Recalcular por periodo" (forzarDefaultEmpleado=true) el default del empleado gana y
        // pisa el override → el día vuelve a EntradaSalida (reglas). English: default null; per-
        // day override "MarcajeReloj". "Recalculate by period" (forzar=true) → default wins,
        // overrides the per-day override → day reverts to EntradaSalida (rules).
        var (db, empresa, _, fecha) = await SembrarCasoBaseAsync(modoDefault: null);
        var asistencia = await db.RrhhAsistencias.SingleAsync();
        asistencia.ModoSugerenciaExtra = "MarcajeReloj";
        await db.SaveChangesAsync();

        var a = await ReprocesarForzandoAsync(db, empresa.Id, fecha);

        Assert.Null(a.ModoSugerenciaExtra);            // default gana, override pisado
        Assert.Equal(17, a.MinutosSalidaAnticipada);    // reglas EntradaSalida restauradas
        Assert.Equal(15, a.MinutosDescansoNoPagado);
    }

    [Fact]
    public async Task RecalculoPorDia_SinForzar_PreservaOverrideManual()
    {
        // Default null; override manual "MarcajeReloj". Recálculo por día (forzar=false,
        // default): preserva el override (no lo pisa). Contrasta con el reproceso por periodo.
        // English: default null; manual override "MarcajeReloj". Per-day recalc (forzar=false):
        // preserves the override. Contrast with the period-reprocess test.
        var (db, empresa, _, fecha) = await SembrarCasoBaseAsync(modoDefault: null);
        var asistencia = await db.RrhhAsistencias.SingleAsync();
        asistencia.ModoSugerenciaExtra = "MarcajeReloj";
        await db.SaveChangesAsync();

        var a = await ReprocesarAsync(db, empresa.Id, fecha);

        Assert.Equal("MarcajeReloj", a.ModoSugerenciaExtra);  // override preservado
        Assert.Equal(0, a.MinutosSalidaAnticipada);           // MarcajeReloj: sin reglas
        Assert.Equal(0, a.MinutosDescansoNoPagado);
    }

    [Fact]
    public async Task ReprocesoPorPeriodo_AplicaNuevoDefaultCambiado()
    {
        // El día quedó en EntradaSalida (default null). El cliente cambia el default del empleado
        // a MarcajeReloj y "Recalcula por periodo": el nuevo default se impone a todos los días.
        // English: day left in EntradaSalida (default null). The client changes the employee
        // default to MarcajeReloj and "Recalculates by period": the new default is enforced.
        var (db, empresa, empleado, fecha) = await SembrarCasoBaseAsync(modoDefault: null);
        var antes = await db.RrhhAsistencias.SingleAsync();
        Assert.Null(antes.ModoSugerenciaExtra);

        empleado.ModoSugerenciaExtraDefault = "MarcajeReloj";
        await db.SaveChangesAsync();

        var a = await ReprocesarForzandoAsync(db, empresa.Id, fecha);

        Assert.Equal("MarcajeReloj", a.ModoSugerenciaExtra);  // nuevo default aplicado
        Assert.Equal(0, a.MinutosSalidaAnticipada);           // MarcajeReloj: sin reglas
        Assert.Equal(0, a.MinutosDescansoNoPagado);
    }

    // ───────────────────────────────────────────────────────────────────────
    // Forzar un método concreto en el reproceso por periodo (modoCalculoForzado): el operador
    // elige Vs horario o Marcaje de reloj en el diálogo, y ese método gana SOBRE el default del
    // empleado y sobre overrides por día. Distinto de forzarDefaultEmpleado (que usa el default
    // del empleado). English: force a concrete method in the period reprocess (modoCalculoForzado):
    // the operator picks Vs schedule or Clock punch in the dialog, and that method wins OVER the
    // employee default and per-day overrides. Distinct from forzarDefaultEmpleado (which uses
    // the employee default).
    // ───────────────────────────────────────────────────────────────────────
    private static async Task<RrhhAsistencia> ReprocesarForzandoModoAsync(CrmDbContext db, Guid empresaId, DateOnly fecha, RrhhModoCalculoForzado modo)
    {
        var processor = new RrhhAsistenciaProcessor();
        await processor.ReprocesarRangoAsync(db, empresaId, fecha, fecha, modoCalculoForzado: modo);
        await db.SaveChangesAsync();
        return await db.RrhhAsistencias.SingleAsync();
    }

    [Fact]
    public async Task ReprocesoPorPeriodo_ForzarModoMarcajeReloj_IgnoraDefaultDelEmpleado()
    {
        // Default null (Vs horario); el operador fuerza "Marcaje de reloj" en el diálogo. El
        // método forzado gana sobre el default → el día se calcula sin reglas aunque el
        // default del empleado sea Vs horario. English: default null (Vs schedule); the operator
        // forces "Clock punch" in the dialog. The forced method wins over the default → the day
        // computes without rules even though the employee default is Vs schedule.
        var (db, empresa, _, fecha) = await SembrarCasoBaseAsync(modoDefault: null);
        var antes = await db.RrhhAsistencias.SingleAsync();
        Assert.Null(antes.ModoSugerenciaExtra);            // default = Vs horario
        Assert.Equal(17, antes.MinutosSalidaAnticipada);    // reglas aplicadas inicialmente

        var a = await ReprocesarForzandoModoAsync(db, empresa.Id, fecha, RrhhModoCalculoForzado.MarcajeReloj);

        Assert.Equal("MarcajeReloj", a.ModoSugerenciaExtra);  // modo forzado, no el default null
        Assert.Equal(0, a.MinutosSalidaAnticipada);           // sin reglas
        Assert.Equal(0, a.MinutosDescansoNoPagado);
        Assert.Equal(18, a.MinutosExtra);                    // 453 − 435 = 18 (≥ umbral 15 → cuenta)
    }

    [Fact]
    public async Task ReprocesoPorPeriodo_ForzarModoVsHorario_IgnoraDefaultMarcajeReloj()
    {
        // Default "MarcajeReloj"; el operador fuerza "Vs horario" en el diálogo. El método
        // forzado gana sobre el default → el día se calcula con reglas aunque el default del
        // empleado sea Marcaje de reloj. English: default "MarcajeReloj"; the operator forces
        // "Vs schedule" in the dialog. The forced method wins over the default → the day
        // computes with rules even though the employee default is Clock punch.
        var (db, empresa, _, fecha) = await SembrarCasoBaseAsync(modoDefault: "MarcajeReloj");
        var antes = await db.RrhhAsistencias.SingleAsync();
        Assert.Equal("MarcajeReloj", antes.ModoSugerenciaExtra);  // default = reloj
        Assert.Equal(0, antes.MinutosSalidaAnticipada);           // sin reglas inicialmente

        var a = await ReprocesarForzandoModoAsync(db, empresa.Id, fecha, RrhhModoCalculoForzado.VsHorario);

        Assert.Null(a.ModoSugerenciaExtra);                 // modo forzado = Vs horario (null)
        Assert.Equal(17, a.MinutosSalidaAnticipada);        // reglas restauradas
        Assert.Equal(15, a.MinutosDescansoNoPagado);        // D1 no marcado descuenta
    }
}