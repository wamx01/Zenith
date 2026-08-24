using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

/// <summary>
/// Tests del servicio <see cref="RrhhPermisoPorDiferenciaService"/> (Fase PermisoPorDiferenciaPeriodo).
/// Permisos sintéticos generados al cierre del periodo cuando |retardo − extra| > 0.
/// Cubre: sugerencia read-only, aplicar 3 categorías, idempotencia, validaciones server-side,
/// y reversión silenciosa (sin tocar ausencias manuales).
/// </summary>
public sealed class RrhhPermisoDiferenciaServiceTests
{
    // FechaReferencia dentro de la semana que se cierra (no el lunes posterior): el servicio
    // usa ObtenerPeriodoContenedor (semana en-curso), así que el viernes 02-01 resuelve el
    // contenedor [lun 29-12 .. dom 04-01], NumeroPeriodo=1 → "Semanal-2026-01", y DiaUno/DiaDos
    // quedan dentro del periodo. Antes se usaba 2026-01-05 (lunes), que caía en la semana
    // siguiente y dejaba el query de asistencias vacío (déficit/extra = 0).
    // English: FechaReferencia inside the week being closed (not the following Monday): the
    // service uses ObtenerPeriodoContenedor (current open week), so Friday 02-01 resolves to
    // [Mon 29-12 .. Sun 04-01], NumeroPeriodo=1 → "Semanal-2026-01", with DiaUno/DiaDos inside.
    // Previously 2026-01-05 (Monday) landed in the next week and left the asistencia query empty.
    private static readonly DateOnly FechaReferencia = new(2026, 1, 2); // viernes → contenedor [29-12 .. 04-01]
    private static readonly DateOnly DiaUno = new(2025, 12, 30);
    private static readonly DateOnly DiaDos = new(2026, 1, 2);

    private static IRrhhPermisoPorDiferenciaService CreateService()
        => new RrhhPermisoPorDiferenciaService(new RrhhTiempoExtraResolutionService());

    // ─── CalcularSugerenciaAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task CalcularSugerencia_DiffPositivo_DevuelveDiferenciaCorrecta()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // Retardo 30 + 60 = 90, Extra 30 + 0 = 30 → diferencia = 60
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 30, retardo: 30));
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaDos, minutosExtra: 0, retardo: 60));
        await db.SaveChangesAsync();

        var svc = CreateService();
        var sug = await svc.CalcularSugerenciaAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(60, sug.DiferenciaMinutos);
        Assert.Equal(0, sug.BancoDisponibleMinutos);
        Assert.Equal("Semanal-2026-01", sug.PeriodoKey);
    }

    [Fact]
    public async Task CalcularSugerencia_DiffCero_DevuelveCero()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // Retardo 60, Extra 60 → diferencia = 0
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 60, retardo: 60));
        await db.SaveChangesAsync();

        var svc = CreateService();
        var sug = await svc.CalcularSugerenciaAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(0, sug.DiferenciaMinutos);
    }

    [Fact]
    public async Task CalcularSugerencia_ExtraMayorQueRetardo_DevuelveCero()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // Retardo 30, Extra 120 → diferencia = max(0, 30-120) = 0
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 120, retardo: 30));
        await db.SaveChangesAsync();

        var svc = CreateService();
        var sug = await svc.CalcularSugerenciaAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(0, sug.DiferenciaMinutos);
    }

    [Fact]
    public async Task CalcularSugerencia_FaltanteAusenciaSinExtra_DevuelveDiferenciaConFaltante()
    {
        // Caso del bug: un dia con ausencia (jornada 480, trabajado 0 → faltante 480),
        // sin extra ni retardo. El neto del periodo es −480, asi que el permiso por
        // diferencia debe sugerir 480 min. Antes del fix (solo retardo−extra) daba 0.
        // Bug case: an absence day (jornada 480, worked 0 → faltante 480) with no extra
        // and no retardo. Period neto is −480, so the permiso should suggest 480 min.
        // Before the fix (retardo−extra only) this returned 0.
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno,
            minutosExtra: 0, retardo: 0, minutosJornadaNetaProgramada: 480, minutosTrabajadosNetos: 0));
        await db.SaveChangesAsync();

        var svc = CreateService();
        var sug = await svc.CalcularSugerenciaAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(480, sug.DiferenciaMinutos);
    }

    [Fact]
    public async Task CalcularSugerencia_FaltanteCubiertoPorPermisoConGoce_NoGeneraDiferencia()
    {
        // Una ausencia ya cubierta por un permiso con goce (descontable=0) no debe generar
        // permiso por diferencia (no se doble-contabiliza). El faltante descontable resta
        // el permiso con goce, igual que el neteo F2.
        // An absence already covered by a con-goce permit (descontable=0) must NOT yield a
        // permiso por diferencia (no double-counting). Descontable faltante subtracts the
        // con-goce permit, mirroring neteo F2.
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // Ausencia: jornada 480, trabajado 0 → faltante bruto 480.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno,
            minutosExtra: 0, retardo: 0, minutosJornadaNetaProgramada: 480, minutosTrabajadosNetos: 0));
        // Permiso con goce aprobado que cubre todo el dia (480 min).
        db.RrhhAusencias.Add(new RrhhAusencia
        {
            Id = Guid.NewGuid(), EmpresaId = empresa.Id, EmpleadoId = empleado.Id,
            Tipo = TipoAusenciaRrhh.PermisoConGoce, Estatus = EstatusAusenciaRrhh.Aplicada,
            FechaInicio = DiaUno, FechaFin = DiaUno, Dias = 1, Horas = 8m,
            ConGocePago = true, DescuentaBancoHoras = false,
            OrigenAusencia = OrigenAusenciaRrhh.Manual, PeriodoKey = null,
            CreatedAt = DateTime.UtcNow, IsActive = true
        });
        await db.SaveChangesAsync();

        var svc = CreateService();
        var sug = await svc.CalcularSugerenciaAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        // Faltante descontable = 480 − 480 (permiso) = 0 → diferencia 0.
        Assert.Equal(0, sug.DiferenciaMinutos);
    }

    [Fact]
    public async Task CalcularSugerencia_BancoDisponible_EsLecturaDirectaDelSaldo()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // Diferencia > 0 (retardo 60, extra 0)
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 0, retardo: 60));
        // Saldo banco = 2h = 120 min
        db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            Fecha = DiaUno,
            TipoMovimiento = TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra,
            Horas = 2m,
            EsAutomatico = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var svc = CreateService();
        var sug = await svc.CalcularSugerenciaAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(60, sug.DiferenciaMinutos);
        Assert.Equal(120, sug.BancoDisponibleMinutos);
    }

    // ─── AplicarPermisosAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task AplicarPermisos_CreaTresFilas_BancoConGoceSinBancoSinGoce()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true);
        db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
        {
            Id = Guid.NewGuid(), EmpresaId = empresa.Id, EmpleadoId = empleado.Id,
            Fecha = DiaUno, TipoMovimiento = TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra,
            Horas = 2m, EsAutomatico = true, CreatedAt = DateTime.UtcNow, IsActive = true
        });
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 0, retardo: 90));
        await db.SaveChangesAsync();

        var svc = CreateService();
        var ausencias = await svc.AplicarPermisosAsync(
            db, empresa.Id, empleado.Id, FechaReferencia,
            inputs: new[]
            {
                new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.Banco,          Minutos = 30, Observaciones = "30 del banco" },
                new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.ConGoceSinBanco, Minutos = 30, Observaciones = "30 con goce" },
                new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.SinGoce,         Minutos = 30, Observaciones = "30 sin goce" }
            },
            usuarioActual: "tester");

        Assert.Equal(3, ausencias.Count);
        Assert.All(ausencias, a => Assert.Equal(TipoAusenciaRrhh.PermisoPorDiferenciaPeriodo, a.Tipo));
        Assert.All(ausencias, a => Assert.Equal(OrigenAusenciaRrhh.SinteticoPorPeriodo, a.OrigenAusencia));
        Assert.All(ausencias, a => Assert.Equal("Semanal-2026-01", a.PeriodoKey));
        Assert.All(ausencias, a => Assert.Equal(EstatusAusenciaRrhh.Aplicada, a.Estatus));

        var porBanco = ausencias.First(a => a.DescuentaBancoHoras);
        Assert.Equal(30, (int)Math.Round(porBanco.Horas * 60m));
        Assert.True(porBanco.ConGocePago);

        var conGoce = ausencias.First(a => !a.DescuentaBancoHoras && a.ConGocePago);
        Assert.Equal(30, (int)Math.Round(conGoce.Horas * 60m));

        var sinGoce = ausencias.First(a => !a.ConGocePago);
        Assert.Equal(30, (int)Math.Round(sinGoce.Horas * 60m));
        Assert.False(sinGoce.DescuentaBancoHoras);

        // Banco: 1 movimiento de consumo (referencia prefijo "permiso-banco:")
        var movimientosBanco = await db.RrhhBancoHorasMovimientos
            .Where(m => m.TipoMovimiento == TipoMovimientoBancoHorasRrhh.Consumo
                && (m.ReferenciaTipo ?? "").StartsWith("permiso-banco:"))
            .ToListAsync();
        Assert.Single(movimientosBanco);
        Assert.Equal(-0.5m, movimientosBanco[0].Horas); // 30 min = 0.5h negativo
    }

    [Fact]
    public async Task AplicarPermisos_SoloBanco_NoCreaFilasVaciasParaConGoceOSinGoce()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true);
        db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
        {
            Id = Guid.NewGuid(), EmpresaId = empresa.Id, EmpleadoId = empleado.Id,
            Fecha = DiaUno, TipoMovimiento = TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra,
            Horas = 2m, EsAutomatico = true, CreatedAt = DateTime.UtcNow, IsActive = true
        });
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 0, retardo: 30));
        await db.SaveChangesAsync();

        var svc = CreateService();
        var ausencias = await svc.AplicarPermisosAsync(
            db, empresa.Id, empleado.Id, FechaReferencia,
            inputs: new[]
            {
                new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.Banco,          Minutos = 30 },
                new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.ConGoceSinBanco, Minutos = 0 },
                new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.SinGoce,         Minutos = 0 }
            },
            usuarioActual: "tester");

        Assert.Single(ausencias);
        Assert.True(ausencias[0].DescuentaBancoHoras);
    }

    [Fact]
    public async Task AplicarPermisos_ReplicaBorraPreviasYCreaNuevas_Idempotente()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true);
        db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
        {
            Id = Guid.NewGuid(), EmpresaId = empresa.Id, EmpleadoId = empleado.Id,
            Fecha = DiaUno, TipoMovimiento = TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra,
            Horas = 2m, EsAutomatico = true, CreatedAt = DateTime.UtcNow, IsActive = true
        });
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 0, retardo: 90));
        await db.SaveChangesAsync();

        var svc = CreateService();

        // Primera aplicación: 60 banco + 30 con goce
        var primera = await svc.AplicarPermisosAsync(
            db, empresa.Id, empleado.Id, FechaReferencia,
            inputs: new[]
            {
                new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.Banco, Minutos = 60 },
                new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.ConGoceSinBanco, Minutos = 30 }
            },
            usuarioActual: "tester");
        await db.SaveChangesAsync();
        Assert.Equal(2, primera.Count);

        // Segunda aplicación (re-autorización): 90 sin goce (distinta repartición)
        var segunda = await svc.AplicarPermisosAsync(
            db, empresa.Id, empleado.Id, FechaReferencia,
            inputs: new[]
            {
                new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.SinGoce, Minutos = 90 }
            },
            usuarioActual: "tester");
        await db.SaveChangesAsync();

        // Solo 1 ausencia del periodo (las 2 anteriores se revirtieron)
        var totalAusenciasSinteticas = await db.RrhhAusencias
            .CountAsync(a => a.EmpresaId == empresa.Id
                && a.EmpleadoId == empleado.Id
                && a.OrigenAusencia == OrigenAusenciaRrhh.SinteticoPorPeriodo);
        Assert.Equal(1, totalAusenciasSinteticas);
        Assert.Single(segunda);
        Assert.False(segunda[0].ConGocePago);

        // Solo 1 movimiento de banco de los nuestros (los de la primera tanda se borraron)
        var movsBanco = await db.RrhhBancoHorasMovimientos
            .CountAsync(m => m.ReferenciaTipo.StartsWith("permiso-banco:"));
        Assert.Equal(0, movsBanco); // SinGoce no genera consumo de banco
    }

    [Fact]
    public async Task AplicarPermisos_BancoExcedeDisponible_LanzaExcepcion()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true);
        db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
        {
            Id = Guid.NewGuid(), EmpresaId = empresa.Id, EmpleadoId = empleado.Id,
            Fecha = DiaUno, TipoMovimiento = TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra,
            Horas = 0.25m, EsAutomatico = true, CreatedAt = DateTime.UtcNow, IsActive = true
        });
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 0, retardo: 90));
        await db.SaveChangesAsync();

        var svc = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.AplicarPermisosAsync(
            db, empresa.Id, empleado.Id, FechaReferencia,
            inputs: new[] { new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.Banco, Minutos = 60 } },
            usuarioActual: "tester"));
    }

    [Fact]
    public async Task AplicarPermisos_SumaExcedeDiferencia_LanzaExcepcion()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 0, retardo: 30));
        await db.SaveChangesAsync();

        var svc = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.AplicarPermisosAsync(
            db, empresa.Id, empleado.Id, FechaReferencia,
            inputs: new[] { new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.SinGoce, Minutos = 60 } },
            usuarioActual: "tester"));
    }

    [Fact]
    public async Task AplicarPermisos_FactorMenorAUno_LanzaExcepcion()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 0, retardo: 30));
        await db.SaveChangesAsync();

        var svc = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.AplicarPermisosAsync(
            db, empresa.Id, empleado.Id, FechaReferencia,
            inputs: new[] { new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.SinGoce, Minutos = 15, Factor = 0.5m } },
            usuarioActual: "tester"));
    }

    [Fact]
    public async Task AplicarPermisos_MinutosNegativos_LanzaExcepcion()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 0, retardo: 30));
        await db.SaveChangesAsync();

        var svc = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.AplicarPermisosAsync(
            db, empresa.Id, empleado.Id, FechaReferencia,
            inputs: new[] { new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.SinGoce, Minutos = -10 } },
            usuarioActual: "tester"));
    }

    [Fact]
    public async Task AplicarPermisos_TotalCero_ReverteSinteticasPreviasYNadaMas()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true);
        db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
        {
            Id = Guid.NewGuid(), EmpresaId = empresa.Id, EmpleadoId = empleado.Id,
            Fecha = DiaUno, TipoMovimiento = TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra,
            Horas = 2m, EsAutomatico = true, CreatedAt = DateTime.UtcNow, IsActive = true
        });
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 0, retardo: 60));
        await db.SaveChangesAsync();

        var svc = CreateService();

        // Primera aplicación: 60 banco
        await svc.AplicarPermisosAsync(
            db, empresa.Id, empleado.Id, FechaReferencia,
            inputs: new[] { new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.Banco, Minutos = 60 } },
            usuarioActual: "tester");
        await db.SaveChangesAsync();
        Assert.Equal(1, await db.RrhhAusencias.CountAsync(a => a.OrigenAusencia == OrigenAusenciaRrhh.SinteticoPorPeriodo));

        // Segunda con total 0 → reversión
        var segunda = await svc.AplicarPermisosAsync(
            db, empresa.Id, empleado.Id, FechaReferencia,
            inputs: new[]
            {
                new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.Banco, Minutos = 0 },
                new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.ConGoceSinBanco, Minutos = 0 },
                new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.SinGoce, Minutos = 0 }
            },
            usuarioActual: "tester");

        Assert.Empty(segunda);
        Assert.Equal(0, await db.RrhhAusencias.CountAsync(a => a.OrigenAusencia == OrigenAusenciaRrhh.SinteticoPorPeriodo));
        Assert.Equal(0, await db.RrhhBancoHorasMovimientos.CountAsync(m => m.ReferenciaTipo.StartsWith("permiso-banco:")));
    }

    // ─── RevertirPermisosAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task RevertirPermisos_BorraSinteticasYMovimientosBanco_NoTocaManuales()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true);
        db.RrhhBancoHorasMovimientos.Add(new RrhhBancoHorasMovimiento
        {
            Id = Guid.NewGuid(), EmpresaId = empresa.Id, EmpleadoId = empleado.Id,
            Fecha = DiaUno, TipoMovimiento = TipoMovimientoBancoHorasRrhh.GeneradoPorHorasExtra,
            Horas = 2m, EsAutomatico = true, CreatedAt = DateTime.UtcNow, IsActive = true
        });
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 0, retardo: 60));

        // Manual: una falta injustificada (NO debe tocarse)
        var manual = new RrhhAusencia
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            Tipo = TipoAusenciaRrhh.FaltaInjustificada,
            Estatus = EstatusAusenciaRrhh.Aplicada,
            FechaInicio = DiaUno,
            FechaFin = DiaUno,
            Dias = 1,
            Horas = 8m,
            ConGocePago = false,
            OrigenAusencia = OrigenAusenciaRrhh.Manual,
            PeriodoKey = null,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        db.RrhhAusencias.Add(manual);
        await db.SaveChangesAsync();

        var svc = CreateService();
        await svc.AplicarPermisosAsync(
            db, empresa.Id, empleado.Id, FechaReferencia,
            inputs: new[] { new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.Banco, Minutos = 60 } },
            usuarioActual: "tester");
        await db.SaveChangesAsync();
        Assert.Equal(2, await db.RrhhAusencias.CountAsync());

        await svc.RevertirPermisosAsync(db, empresa.Id, empleado.Id, FechaReferencia);
        await db.SaveChangesAsync();

        // Solo queda la manual
        var restantes = await db.RrhhAusencias.ToListAsync();
        Assert.Single(restantes);
        Assert.Equal(TipoAusenciaRrhh.FaltaInjustificada, restantes[0].Tipo);
        Assert.Equal(manual.Id, restantes[0].Id);

        // El movimiento de banco de la sintética se borró
        Assert.Equal(0, await db.RrhhBancoHorasMovimientos.CountAsync(m => m.ReferenciaTipo.StartsWith("permiso-banco:")));
        // El saldo del banco de la precarga sigue intacto
        Assert.Equal(1, await db.RrhhBancoHorasMovimientos.CountAsync());
    }

    [Fact]
    public async Task RevertirPermisos_SinSinteticas_NoHaceNada()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);

        var svc = CreateService();
        await svc.RevertirPermisosAsync(db, empresa.Id, empleado.Id, FechaReferencia);
        await db.SaveChangesAsync();

        Assert.Equal(0, await db.RrhhAusencias.CountAsync());
        Assert.Equal(0, await db.RrhhBancoHorasMovimientos.CountAsync());
    }

    // ─── Meta semanal (Fija sin turno) ───────────────────────────────────────────

    [Fact]
    public async Task CalcularSugerencia_MetaSemanal_ConDeficit_NoGeneraPermiso()
    {
        // Fija sin turno: el déficit semanal descuenta sueldo como FaltanteDescontable, NO
        // genera permiso por diferencia. Aunque el periodo tenga 8h de déficit (40h vs 48h),
        // la detección se anula (0,0) → DiferenciaMinutos = 0 (panel oculto).
        // English: Fija with no shift: the weekly deficit docks salary as FaltanteDescontable,
        // it does NOT generate a permiso por diferencia. Even with an 8h deficit (40h vs 48h),
        // detection is zeroed (0,0) → DiferenciaMinutos = 0 (panel hidden).
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // 2 días × 1200 min = 2400 min (40h) → déficit 480 (8h) bajo la meta de 2880 (48h).
        db.RrhhAsistencias.Add(CrearAsistenciaMetaSemanal(empresa.Id, empleado.Id, DiaUno, neto: 1200));
        db.RrhhAsistencias.Add(CrearAsistenciaMetaSemanal(empresa.Id, empleado.Id, DiaDos, neto: 1200));
        await db.SaveChangesAsync();

        var svc = CreateService();
        var sug = await svc.CalcularSugerenciaAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(0, sug.DiferenciaMinutos);
    }

    [Fact]
    public async Task AplicarPermisos_MetaSemanas_ConDeficit_RechazaInputPositivo()
    {
        // Cualquier input positivo se rechaza porque diferenciaMax = 0 para meta semanal.
        // English: Any positive input is rejected because diferenciaMax = 0 for weekly meta.
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true);
        db.RrhhAsistencias.Add(CrearAsistenciaMetaSemanal(empresa.Id, empleado.Id, DiaUno, neto: 1200));
        db.RrhhAsistencias.Add(CrearAsistenciaMetaSemanal(empresa.Id, empleado.Id, DiaDos, neto: 1200));
        await db.SaveChangesAsync();

        var svc = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.AplicarPermisosAsync(
            db, empresa.Id, empleado.Id, FechaReferencia,
            inputs: new[] { new PermisoDiferenciaInput { Categoria = CategoriaPermisoDiferencia.SinGoce, Minutos = 60 } },
            usuarioActual: "tester"));
    }

    // ─── helpers ─────────────────────────────────────────────────────────────────

    private static CrmDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"permiso-dif-{Guid.NewGuid():N}")
            .Options;
        return new CrmDbContext(options);
    }

    private static async Task<(Empresa Empresa, Empleado Empleado)> SembrarAsync(
        CrmDbContext db, bool bancoHabilitado = false, decimal factorAcumulacion = 1m)
    {
        var empresa = CreateEmpresa();
        var empleado = CreateEmpleado(empresa.Id);
        db.Empresas.Add(empresa);
        db.Empleados.Add(empleado);
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.FactorHoraExtra, "2"));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.BancoHorasHabilitado, bancoHabilitado ? "true" : "false"));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.BancoHorasFactorAcumulacion, factorAcumulacion.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.BancoHorasTopeHoras, "45"));
        await db.SaveChangesAsync();
        return (empresa, empleado);
    }

    private static RrhhAsistencia CrearAsistencia(Guid empresaId, Guid empleadoId, DateOnly fecha,
        int minutosExtra = 0, int retardo = 0, int minutosJornadaNetaProgramada = 480, int minutosTrabajadosNetos = 480)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            Fecha = fecha,
            // TurnoBaseId asignado → Fija-con-turno (NO meta semanal). Sin este setter todas las
            // asistencias quedarían como Fija-sin-turno y el overlay de meta semanal anularía la
            // detección (0,0), rompiendo los tests existentes de retardo/extra/faltante per-día.
            // English: TurnoBaseId set → Fija-with-shift (NOT weekly meta). Without this setter all
            // asistencias would be Fija-with-no-shift and the weekly-meta overlay would zero the
            // detection (0,0), breaking the existing per-day retardo/extra/faltante tests.
            TurnoBaseId = Guid.NewGuid(),
            EsPorHoras = false,
            MinutosTrabajadosNetos = minutosTrabajadosNetos,
            MinutosJornadaNetaProgramada = minutosJornadaNetaProgramada,
            MinutosExtra = minutosExtra,
            MinutosRetardo = retardo,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

    // Asistencia Fija sin turno (TurnoBaseId null, EsPorHoras false) → rige la meta semanal 48h.
    // English: Fija with no shift (TurnoBaseId null, EsPorHoras false) → governed by the 48h weekly meta.
    private static RrhhAsistencia CrearAsistenciaMetaSemanal(Guid empresaId, Guid empleadoId, DateOnly fecha, int neto)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            Fecha = fecha,
            TurnoBaseId = null,
            EsPorHoras = false,
            MinutosJornadaNetaProgramada = 0,
            MinutosTrabajadosNetos = neto,
            MinutosExtra = 0,
            MinutosRetardo = 0,
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
}