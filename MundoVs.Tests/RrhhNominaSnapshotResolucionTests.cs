using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

/// <summary>
/// Fase 7 — el snapshot de nómina consume la resolución Autorizada del periodo (mismo helper
/// que el cálculo de nómina) para que el display cuadre con el cálculo. Sin resolución
/// (o Pendiente/Reabierta) cae al resumen diario (comportamiento histórico).
/// </summary>
public sealed class RrhhNominaSnapshotResolucionTests
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
        // Neteo NetoVsNeto consumido del batch canónico (el MISMO que Asistencia Semanal): el pool
        // de 120 min de extra tapa el retardo de 10 → el snapshot sobreescribe la deducción con
        // (detectado − absorbido) = 10 − 10 = 0, IGUAL que Asistencia Semanal. Sin resolución
        // Autorizada el sourcing cae al path incidencia (passthrough) y devuelve ese 0 ya neteado.
        // English: NetoVsNeto netting consumed from the canonical batch (the SAME one Asistencia
        // Semanal uses): the 120-min extra pool covers the 10-min late → the snapshot overwrites
        // the deduction with (detected − absorbed) = 10 − 10 = 0, SAME as Asistencia Semanal.
        // Without an Autorizada resolution the sourcing falls to the incidencia path (passthrough)
        // and returns that already-netted 0.
        Assert.Equal(0, snapshot.MinutosRetardo);   // 10 − 10 absorbido por el extra (batch canónico)
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

    [Fact]
    public async Task BajoUmbral_NeteaFaltante_SinResolucionAutorizada()
    {
        // Caso exacto del usuario: extra BAJO umbral (no pagadero) tapa un faltante de OTRO día,
        // SIN resolución Autorizada. El batch del neteo carga su propia config de la BD
        // (NominaConfiguracionLoader → AppConfigs de la empresa); en el test la empresa no tiene
        // config guardada → MinutosMinimosTiempoExtra=0 → se normaliza al default 15
        // (RrhhTiempoExtraPolicy.NormalizarMinutosMinimosTiempoExtra). Día 1: neto 490 (jornada 480)
        // → excedente 10 < 15 → bajo umbral, MinutosExtra 0 (no pagadero, pero entra al POOL del
        // neteo). Día 2: faltante 10 (neto 470). Pool = 0 detectado + 10 bajo = 10 → absorbe el
        // faltante 10 → el snapshot deja MinutosFaltanteDescontable = 0, IGUAL que Asistencia Semanal
        // muestra. Antes del refactor, el path "incidencia" del sourcing no veía el bajo-umbral
        // (MinutosExtra 0) y devolvía el faltante crudo (10) → la nómina divergía de Asistencia Semanal.
        // English: The user's exact case: BELOW-threshold extra (not payable) covers ANOTHER day's
        // shortage, with NO Autorizada resolution. The neteo batch loads its own config from the DB
        // (NominaConfiguracionLoader → the company's AppConfigs); in the test the company has no saved
        // config → MinutosMinimosTiempoExtra=0 → normalized to the default 15
        // (RrhhTiempoExtraPolicy.NormalizarMinutosMinimosTiempoExtra). Day 1: net 490 (jornada 480)
        // → surplus 10 < 15 → below threshold, MinutosExtra 0 (not payable, but feeds the net POOL).
        // Day 2: shortage 10 (net 470). Pool = 0 detected + 10 below = 10 → absorbs the shortage 10
        // → the snapshot leaves MinutosFaltanteDescontable = 0, SAME as Asistencia Semanal shows.
        // Before the refactor, the sourcing "incidencia" path didn't see the below-threshold
        // (MinutosExtra 0) and returned the raw shortage (10) → nómina diverged from Asistencia Semanal.
        var (db, connection) = await CreateDbContextAsync();
        await using var __c = connection;
        await using var __db = db;
        var (empresa, empleado) = await SembrarAsync(db);
        // Día 1: excedente 10 bajo umbral (15) → MinutosExtra 0 (el processor lo zeroa bajo umbral);
        // faltante: -10 ⇒ neto 480 - (-10) = 490.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, Dia,
            minutosExtra: 0, minutosExtraAutorizadosPago: 0, retardo: 0, faltante: -10));
        // Día 2: faltante 10 ⇒ neto 470.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, Dia.AddDays(1),
            minutosExtra: 0, minutosExtraAutorizadosPago: 0, retardo: 0, faltante: 10));
        await db.SaveChangesAsync();

        var snapshot = (await CreateService().ConstruirSnapshotPeriodoAsync(db, Inicio, Fin, Configuracion()))
            .Single(s => s.Empleado.Id == empleado.Id);

        Assert.Equal(0, snapshot.MinutosFaltanteDescontable); // 10 − 10 absorbido por el bajo-umbral
        Assert.Equal(0m, snapshot.HorasExtra);               // bajo umbral → no pagado
        Assert.Equal(0m, snapshot.HorasExtraBase);           // MinutosExtra 0 → 0
    }

    // ─── Meta semanal (Fija sin turno) ───────────────────────────────────────────

    [Fact]
    public async Task MetaSemanal_BajoMeta_SinResolucion_DescuentaDeficitComoFaltante()
    {
        // Fija sin turno, 5 días × 8h = 40h (2400 min) contra meta 48h (2880 min) → déficit 480
        // (8h). Sin resolución, el overlay del snapshot (path "incidencia") reporta el déficit
        // como MinutosFaltanteDescontable para que descuente sueldo, y NO paga extra (0).
        // English: Fija with no shift, 5 days × 8h = 40h (2400 min) vs 48h meta (2880 min) → 480
        // deficit (8h). Without a resolution, the snapshot overlay ("incidencia" path) reports
        // the deficit as MinutosFaltanteDescontable so it docks salary, and pays NO extra (0).
        var (db, connection) = await CreateDbContextAsync();
        await using var __c = connection;
        await using var __db = db;
        var (empresa, empleado) = await SembrarAsync(db);
        for (var i = 0; i < 5; i++)
            db.RrhhAsistencias.Add(CrearAsistenciaMetaSemanal(empresa.Id, empleado.Id, InicioDate.AddDays(i), neto: 480));
        await db.SaveChangesAsync();

        var snapshot = (await CreateService().ConstruirSnapshotPeriodoAsync(db, Inicio, Fin, ConfiguracionMetaSemanal()))
            .Single(s => s.Empleado.Id == empleado.Id);

        Assert.Equal(480, snapshot.MinutosFaltanteDescontable); // déficit 8h descuenta sueldo
        Assert.Equal(0, snapshot.MinutosRetardo);               // sin turno → sin retardo
        Assert.Equal(0, snapshot.MinutosSalidaAnticipada);      // sin turno → sin salida anticipada
        Assert.Equal(0m, snapshot.HorasExtra);                   // sin resolución → no se paga extra
        Assert.Equal(0m, snapshot.HorasExtraBase);               // bajo la meta → no hay extra detectado
    }

    [Fact]
    public async Task MetaSemanal_SobreMeta_SinResolucion_ExtraDetectadoNoPagado()
    {
        // Fija sin turno, 5 días × 10h = 50h (3000 min) contra meta 48h (2880 min) → extra 120
        // (2h) detectado. Sin resolución, el overlay reporta el extra como HorasExtraBase
        // (detectado) pero NO lo paga (HorasExtra=0): el pago requiere autorización en la
        // resolución de periodo (path "periodo" del sourcing). No hay déficit.
        // English: Fija with no shift, 5 days × 10h = 50h (3000 min) vs 48h meta (2880 min) → 120
        // extra (2h) detected. Without a resolution, the overlay reports the extra as
        // HorasExtraBase (detected) but does NOT pay it (HorasExtra=0): payment requires
        // authorization in the period resolution (sourcing "periodo" path). No deficit.
        var (db, connection) = await CreateDbContextAsync();
        await using var __c = connection;
        await using var __db = db;
        var (empresa, empleado) = await SembrarAsync(db);
        for (var i = 0; i < 5; i++)
            db.RrhhAsistencias.Add(CrearAsistenciaMetaSemanal(empresa.Id, empleado.Id, InicioDate.AddDays(i), neto: 600));
        await db.SaveChangesAsync();

        var snapshot = (await CreateService().ConstruirSnapshotPeriodoAsync(db, Inicio, Fin, ConfiguracionMetaSemanal()))
            .Single(s => s.Empleado.Id == empleado.Id);

        Assert.Equal(2m, snapshot.HorasExtraBase);   // 120 min / 60 = 2h detectado sobre la meta
        Assert.Equal(0m, snapshot.HorasExtra);         // sin resolución → no pagado
        Assert.Equal(0, snapshot.MinutosFaltanteDescontable); // sobre la meta → sin déficit
    }

    [Fact]
    public async Task MetaSemanal_ExcedenteBajoUmbral_ConConfigCero_NoCuentaComoExtra()
    {
        // Consistencia "Ver detalle" vs "Aceptar tiempo": la config trae MinutosMinimosTiempoExtra
        // = 0. El cálculo por día normaliza 0 → 15 (default); el overlay de meta semanal debe
        // aplicar el MISMO umbral para no reportar extra que el detalle por día perdona. 5 días ×
        // 578 = 2890 min vs meta 2880 → excedente 10 < 15 → 0 extra (no 10). Sin la normalización
        // el meta-semanal pasaba 0 crudo y reportaba 10, inconsistente con el detalle.
        // English: "Ver detalle" vs "Aceptar tiempo" consistency: config has
        // MinutosMinimosTiempoExtra = 0. The per-day calc normalizes 0 → 15 (default); the
        // weekly-meta overlay must apply the SAME threshold so it doesn't report extra the
        // per-day detail forgives. 5 days × 578 = 2890 min vs 2880 meta → 10 surplus < 15 → 0
        // extra (not 10). Without normalization the meta-semanal path passed 0 raw and reported
        // 10, inconsistent with the detail.
        var (db, connection) = await CreateDbContextAsync();
        await using var __c = connection;
        await using var __db = db;
        var (empresa, empleado) = await SembrarAsync(db);
        for (var i = 0; i < 5; i++)
            db.RrhhAsistencias.Add(CrearAsistenciaMetaSemanal(empresa.Id, empleado.Id, InicioDate.AddDays(i), neto: 578));
        await db.SaveChangesAsync();

        var config = ConfiguracionMetaSemanal();
        config.MinutosMinimosTiempoExtra = 0;
        var snapshot = (await CreateService().ConstruirSnapshotPeriodoAsync(db, Inicio, Fin, config))
            .Single(s => s.Empleado.Id == empleado.Id);

        Assert.Equal(0m, snapshot.HorasExtraBase);        // 10 min < umbral 15 (normalizado de 0) → 0
        Assert.Equal(0m, snapshot.HorasExtra);              // sin resolución → no pagado
        Assert.Equal(0, snapshot.MinutosFaltanteDescontable); // sobre la meta → sin déficit
    }

    // ─── Pago fijo por labor (perfil #4: limpieza) — routing del snapshot ──────────

    [Fact]
    public async Task PagoFijoPorLabor_DiaPorHorasConFlag_NoEntraBucketPorHoras_QuedaEnDiasPagados()
    {
        // 2 días PorHoras CON flag PagoFijoPorLabor (1h=60min y 5h=300min). El snapshot los
        // EXCLUYE del bucket "por horas" → DiasPorHorasTrabajados=0 y MinutosPorHorasNetos=0.
        // Los días quedan en DiasPagados (= días del periodo, aquí 7) → fluyen a la parte Fija
        // (sueldoDiario × día) en NominaSueldoBasePolicy. Así dura 1h o 5h, cobra lo mismo.
        // English: 2 PorHoras days WITH the PagoFijoPorLabor flag (1h=60min and 5h=300min). The
        // snapshot EXCLUDES them from the "by hours" bucket → DiasPorHorasTrabajados=0 and
        // MinutosPorHorasNetos=0. The days stay in DiasPagados (= period days, here 7) → flow to
        // the Fija part (daily salary × day) in NominaSueldoBasePolicy. 1h or 5h pays the same.
        var (db, connection) = await CreateDbContextAsync();
        await using var __c = connection;
        await using var __db = db;
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistenciaPorHoras(empresa.Id, empleado.Id, InicioDate, neto: 60, pagoFijoPorLabor: true));
        db.RrhhAsistencias.Add(CrearAsistenciaPorHoras(empresa.Id, empleado.Id, InicioDate.AddDays(1), neto: 300, pagoFijoPorLabor: true));
        await db.SaveChangesAsync();

        var snapshot = (await CreateService().ConstruirSnapshotPeriodoAsync(db, Inicio, Fin, Configuracion()))
            .Single(s => s.Empleado.Id == empleado.Id);

        Assert.Equal(0, snapshot.DiasPorHorasTrabajados);   // excluidos del bucket por horas
        Assert.Equal(0, snapshot.MinutosPorHorasNetos);      // los 360 min no se pagan por minuto
        Assert.Equal(7, snapshot.DiasPagados);               // los días siguen contando como pagados
    }

    [Fact]
    public async Task PorHoras_SinFlag_SiEntraBucketPorHoras()
    {
        // Mismos 2 días PorHoras (60min + 300min) SIN flag → van al bucket "por horas":
        // DiasPorHorasTrabajados=2 y MinutosPorHorasNetos=360 (pago por minuto, comportamiento
        // normal). Contrasta con el test anterior: el flag es lo que redirige el día.
        // English: same 2 PorHoras days (60min + 300min) WITHOUT the flag → go to the "by hours"
        // bucket: DiasPorHorasTrabajados=2 and MinutosPorHorasNetos=360 (by-minute pay, normal
        // behavior). Contrasts with the previous test: the flag is what redirects the day.
        var (db, connection) = await CreateDbContextAsync();
        await using var __c = connection;
        await using var __db = db;
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistenciaPorHoras(empresa.Id, empleado.Id, InicioDate, neto: 60, pagoFijoPorLabor: false));
        db.RrhhAsistencias.Add(CrearAsistenciaPorHoras(empresa.Id, empleado.Id, InicioDate.AddDays(1), neto: 300, pagoFijoPorLabor: false));
        await db.SaveChangesAsync();

        var snapshot = (await CreateService().ConstruirSnapshotPeriodoAsync(db, Inicio, Fin, Configuracion()))
            .Single(s => s.Empleado.Id == empleado.Id);

        Assert.Equal(2, snapshot.DiasPorHorasTrabajados);
        Assert.Equal(360, snapshot.MinutosPorHorasNetos);
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
        // Desactiva el enforcement de FK en SQLite: los helpers de test asignan TurnoBaseId a un
        // Guid aleatorio (para modelar Fija-con-turno y no caer en el overlay de meta semanal) sin
        // sembrar la fila TurnoBase. El provider InMemory (otros archivos de test) no enforcea FK;
        // esto iguala la semántica para que el fixture sea mínimo.
        // English: Disable FK enforcement in SQLite: test helpers assign TurnoBaseId to a random
        // Guid (to model Fija-con-shift and avoid the weekly-meta overlay) without seeding the
        // TurnoBase row. The InMemory provider (other test files) does not enforce FK; this
        // matches that semantics so the fixture stays minimal.
        await using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA foreign_keys = OFF;";
            await pragma.ExecuteNonQueryAsync();
        }
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new CrmDbContext(options);
        await db.Database.EnsureCreatedAsync();
        return (db, connection);
    }

    // El snapshot ahora consume el neteo canónico del batch (el MISMO que Asistencia Semanal), así
    // que depende de IRrhhResolucionPeriodoService. La cadena es barata: RrhhTiempoExtraResolutionService
    // sin deps; RrhhPermisoPorDiferenciaService toma sólo IRrhhTiempoExtraResolutionService.
    // English: The snapshot now consumes the canonical batch neteo (the SAME one Asistencia Semanal
    // uses), so it depends on IRrhhResolucionPeriodoService. The chain is cheap: the resolution
    // service has no deps; the permiso-diferencia service takes only IRrhhTiempoExtraResolutionService.
    private static RrhhNominaSnapshotService CreateService()
    {
        var tiempoExtra = new RrhhTiempoExtraResolutionService();
        return new RrhhNominaSnapshotService(
            new NominaLegalPolicyService(),
            tiempoExtra,
            new RrhhResolucionPeriodoService(tiempoExtra, new RrhhPermisoPorDiferenciaService(tiempoExtra)));
    }

    private static NominaConfiguracion Configuracion() => new()
    {
        FactorHoraExtra = 2m,
        HorasExtraDoblesPorSemana = 9
    };

    // Configuración con meta semanal explícita (48h → 2880 min). El Configuracion() por defecto
    // deja HorasBaseSemanal=0 (int default), lo que anularía la meta; por eso los tests de meta
    // semanal usan esta variante.
    // English: Configuration with explicit weekly meta (48h → 2880 min). The default Configuracion()
    // leaves HorasBaseSemanal=0 (int default), which would zero the meta; that's why weekly-meta
    // tests use this variant.
    private static NominaConfiguracion ConfiguracionMetaSemanal() => new()
    {
        FactorHoraExtra = 2m,
        HorasExtraDoblesPorSemana = 9,
        HorasBaseSemanal = 48
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
            // TurnoBaseId asignado → Fija-con-turno (NO meta semanal). Sin este setter las
            // asistencias quedarían como Fija-sin-turno y el overlay de meta semanal reescribiría
            // extra/faltante/retardo a nivel de periodo, rompiendo los tests per-día existentes.
            // English: TurnoBaseId set → Fija-with-shift (NOT weekly meta). Without this setter
            // asistencias would be Fija-with-no-shift and the weekly-meta overlay would rewrite
            // extra/faltante/retardo at the period level, breaking the existing per-day tests.
            TurnoBaseId = Guid.NewGuid(),
            EsPorHoras = false,
            MinutosTrabajadosNetos = neto,
            MinutosJornadaNetaProgramada = 480,
            MinutosExtra = minutosExtra,
            MinutosExtraAutorizadosPago = minutosExtraAutorizadosPago,
            MinutosRetardo = retardo,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    // Asistencia Fija sin turno (TurnoBaseId null, EsPorHoras false) → rige la meta semanal 48h.
    // El neto es el tiempo realmente trabajado (sin jornada programada per-día).
    // English: Fija with no shift (TurnoBaseId null, EsPorHoras false) → governed by the 48h
    // weekly meta. neto is actually-worked time (no per-day scheduled jornada).
    private static RrhhAsistencia CrearAsistenciaMetaSemanal(Guid empresaId, Guid empleadoId, DateOnly fecha, int neto)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            Fecha = fecha,
            Estatus = RrhhAsistenciaEstatus.AsistenciaNormal,
            TurnoBaseId = null,
            EsPorHoras = false,
            MinutosJornadaNetaProgramada = 0,
            MinutosTrabajadosNetos = neto,
            MinutosExtra = 0,
            MinutosRetardo = 0,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

    // Asistencia PorHoras (EsPorHoras true, TurnoBaseId null). Con pagoFijoPorLabor=true el
    // snapshot debe EXCLUIR el día del bucket "por horas" (minutos) y dejarlo en DiasPagados.
    // neto = minutos realmente trabajados (sin jornada programada).
    // English: PorHoras attendance (EsPorHoras true, TurnoBaseId null). With pagoFijoPorLabor=true
    // the snapshot must EXCLUDE the day from the "by hours" bucket (minutes) and leave it in
    // DiasPagados. neto = actually-worked minutes (no scheduled jornada).
    private static RrhhAsistencia CrearAsistenciaPorHoras(Guid empresaId, Guid empleadoId, DateOnly fecha, int neto, bool pagoFijoPorLabor)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            Fecha = fecha,
            Estatus = RrhhAsistenciaEstatus.TrabajadoPorHoras,
            TurnoBaseId = null,
            EsPorHoras = true,
            EsPagoFijoPorLabor = pagoFijoPorLabor,
            MinutosJornadaNetaProgramada = 0,
            MinutosTrabajadosNetos = neto,
            MinutosExtra = 0,
            MinutosRetardo = 0,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

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