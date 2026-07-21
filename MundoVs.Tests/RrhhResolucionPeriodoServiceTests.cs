using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Tests;

public sealed class RrhhResolucionPeriodoServiceTests
{
    // Corte dominical: el periodo es Lun..Dom y la referencia devuelve el periodo
    // MÁS RECIENTEMENTE CERRADO. Para ref 2026-01-05 (lun) el periodo es
    // 2025-12-29 .. 2026-01-04 (clave Semanal-2026-01).
    private static readonly DateOnly FechaReferencia = new(2026, 1, 5);
    private static readonly DateOnly FechaReferenciaTardia = new(2026, 1, 6); // mismo periodo (corte 04/01)
    private static readonly DateOnly DiaUno = new(2025, 12, 30);  // martes, dentro del periodo
    private static readonly DateOnly DiaDos = new(2026, 1, 2);   // viernes, dentro del periodo

    [Fact]
    public async Task ObtenerOCrear_CreaPeriodoPendiente_YEsIdempotente()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);

        var service = CreateService();

        var periodo1 = await service.ObtenerOCrearPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia);
        await db.SaveChangesAsync();

        var periodo2 = await service.ObtenerOCrearPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferenciaTardia);
        await db.SaveChangesAsync();

        Assert.Equal(periodo1.Id, periodo2.Id);
        Assert.Equal(RrhhResolucionPeriodoEstatus.Pendiente, periodo2.Estatus);
        Assert.Equal(PeriodicidadPago.Semanal, periodo2.PeriodicidadPago);
        Assert.Equal(new DateOnly(2025, 12, 29), periodo2.FechaInicio);
        Assert.Equal(new DateOnly(2026, 1, 4), periodo2.FechaFin);
        Assert.Equal("Semanal-2026-01", periodo2.PeriodoKey);
    }

    [Fact]
    public async Task ObtenerOCrear_LanzaParaDestajo()
    {
        await using var db = CreateDbContext();
        var empresa = CreateEmpresa();
        var empleado = CreateEmpleado(empresa.Id, TipoNomina.Destajo);
        db.Empresas.Add(empresa);
        db.Empleados.Add(empleado);
        await db.SaveChangesAsync();

        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ObtenerOCrearPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia));
    }

    [Fact]
    public async Task ObtenerResumen_AgregaDeteccionYDesglosePorDia()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 60, retardo: 10));
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaDos, minutosExtra: 30, retardo: 0));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resumen = await service.ObtenerResumenPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.True(resumen.EsAplicable);
        Assert.Equal(2, resumen.Dias.Count);
        Assert.Equal(90, resumen.MinutosExtraDetectado);
        Assert.Equal(10, resumen.MinutosRetardoDetectado);
        Assert.Equal(DiaUno, resumen.Dias[0].Fecha);
        Assert.Equal(60, resumen.Dias[0].MinutosExtra);
        Assert.Equal(DiaDos, resumen.Dias[1].Fecha);
        Assert.Equal(30, resumen.Dias[1].MinutosExtra);
    }

    [Fact]
    public async Task Aplicar_CapSinNetting_RechazaSiSumaExcedeDetectado()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 60));
        await db.SaveChangesAsync();

        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            FechaReferencia = FechaReferencia,
            Resolucion = "PagarTodo",
            MinutosBasePago = 50,
            MinutosBaseBanco = 20, // 50 + 20 = 70 > 60 detectado
            UsuarioActual = "tester"
        }));
    }

    [Fact]
    public async Task Aplicar_Autoriza_YGeneraMovimientoBanco()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 120));
        await db.SaveChangesAsync();

        var service = CreateService();
        var result = await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            FechaReferencia = FechaReferencia,
            Resolucion = "MitadMitad",
            MinutosBasePago = 60,
            MinutosBaseBanco = 60,
            UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        Assert.Equal(RrhhResolucionPeriodoEstatus.Autorizada, result.Periodo.Estatus);
        Assert.Equal(60, result.MinutosBasePagoAplicados);
        Assert.Equal(60, result.MinutosBaseBancoAplicados);
        Assert.Equal(60, result.MinutosBancoAplicados); // factor 1
        Assert.Equal("tester", result.Periodo.AutorizadoPor);

        var referencia = $"Periodo:{empleado.Id:N}:Semanal-2026-01:extra-banco";
        var movimiento = await db.RrhhBancoHorasMovimientos.SingleAsync(m => m.ReferenciaTipo == referencia);
        Assert.Equal(1m, movimiento.Horas); // 60 min / 60
    }

    [Fact]
    public async Task Aplicar_IdempotenciaLedger_ReemplazaMovimientoPrevio()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 120));
        await db.SaveChangesAsync();

        var service = CreateService();

        // Primera autorización: 60 al banco.
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "BancoTodo", MinutosBaseBanco = 60, UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        // Re-autorización: 30 al banco. Debe reemplazar el movimiento previo (no duplicar).
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "BancoTodo", MinutosBaseBanco = 30, UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        var referencia = $"Periodo:{empleado.Id:N}:Semanal-2026-01:extra-banco";
        var movimientos = await db.RrhhBancoHorasMovimientos
            .Where(m => m.ReferenciaTipo == referencia && m.IsActive)
            .ToListAsync();
        Assert.Single(movimientos);
        Assert.Equal(0.5m, movimientos[0].Horas); // 30 min / 60
    }

    [Fact]
    public async Task Reabrir_RevuelveMovimientoYDejaReabierta()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 120));
        await db.SaveChangesAsync();

        var service = CreateService();
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "BancoTodo", MinutosBaseBanco = 60, UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        await service.ReabrirPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia, "sistema-reproceso");
        await db.SaveChangesAsync();

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.Equal(RrhhResolucionPeriodoEstatus.Reabierta, periodo.Estatus);
        Assert.Equal(0, periodo.MinutosExtraBanco);

        var referencia = $"Periodo:{empleado.Id:N}:Semanal-2026-01:extra-banco";
        var movimientos = await db.RrhhBancoHorasMovimientos
            .Where(m => m.ReferenciaTipo == referencia && m.IsActive)
            .ToListAsync();
        Assert.Empty(movimientos);
    }

    [Fact]
    public async Task Backfill_CreaPeriodoAutorizadoDesdeColumnasDiarias_YEsIdempotente()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        var a1 = CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 60);
        a1.MinutosExtraAutorizadosPago = 40;
        a1.MinutosExtraAutorizadosBanco = 20;
        var a2 = CrearAsistencia(empresa.Id, empleado.Id, DiaDos, minutosExtra: 30);
        a2.MinutosExtraAutorizadosPago = 30;
        db.RrhhAsistencias.Add(a1);
        db.RrhhAsistencias.Add(a2);
        await db.SaveChangesAsync();

        var service = CreateService();
        var primera = await service.BackfillDesdeAutorizacionDiariaAsync(db, null, "backfill");
        await db.SaveChangesAsync();

        Assert.Equal(1, primera.PeriodosCreados);
        Assert.Equal(0, primera.PeriodosOmitidos);

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.Equal(RrhhResolucionPeriodoEstatus.Autorizada, periodo.Estatus);
        Assert.Equal(70, periodo.MinutosExtraPago);   // 40 + 30
        Assert.Equal(20, periodo.MinutosExtraBanco);
        Assert.Equal(90, periodo.MinutosExtraDetectado);

        // Segunda corrida: idempotente, nada nuevo.
        var segunda = await service.BackfillDesdeAutorizacionDiariaAsync(db, null, "backfill");
        await db.SaveChangesAsync();
        Assert.Equal(0, segunda.PeriodosCreados);
        Assert.Equal(1, segunda.PeriodosOmitidos);
    }

    // ── Fase 2: el extra absorbe el faltante neto del periodo ──

    [Fact]
    public async Task Resumen_CuandoExtraMenorQueFaltante_TodoAbsorbeYCeroPagable()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // 60 min de faltante (jornada 480, neto 420), 30 min de extra.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 30, jornadaNeta: 480, neto: 420));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resumen = await service.ObtenerResumenPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(60, resumen.MinutosFaltanteNetoPeriodo);
        Assert.Equal(30, resumen.MinutosExtraDetectado);
        Assert.Equal(30, resumen.MinutosFaltanteAbsorbidoExtra);
        Assert.Equal(0, resumen.MinutosExtraAbsorbible); // todo el extra tapó faltante
    }

    [Fact]
    public async Task Resumen_CuandoExtraMayorQueFaltante_SobraAbsorbible()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // 30 min de faltante, 90 min de extra.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 90, jornadaNeta: 480, neto: 450));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resumen = await service.ObtenerResumenPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(30, resumen.MinutosFaltanteNetoPeriodo);
        Assert.Equal(30, resumen.MinutosFaltanteAbsorbidoExtra);
        Assert.Equal(60, resumen.MinutosExtraAbsorbible); // 90 − 30
    }

    [Fact]
    public async Task Resumen_CuandoFaltanteCubiertoPorPermiso_ExtraQuedaTotalmenteAbsorbible()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // 60 min de faltante, 60 min de extra, PERO 1h de permiso con goce cubre el día.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 60, jornadaNeta: 480, neto: 420));
        db.RrhhAusencias.Add(CrearPermisoConGoce(empresa.Id, empleado.Id, DiaUno, horas: 1m));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resumen = await service.ObtenerResumenPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(60, resumen.MinutosPermisoConGocePeriodo);
        Assert.Equal(0, resumen.MinutosFaltanteNetoPeriodo); // el permiso cubrió el faltante
        Assert.Equal(0, resumen.MinutosFaltanteAbsorbidoExtra);
        Assert.Equal(60, resumen.MinutosExtraAbsorbible); // el extra queda 100% pagable
    }

    [Fact]
    public async Task Resumen_CuandoFaltanteParcialCubiertoPorCompensacion_ReduceFaltanteNeto()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // 60 min de faltante (jornada 480, neto 420), 40 min de compensación aprobada,
        // 90 min de extra. La compensación reduce el faltante descontable a 20 → el extra
        // solo tapa 20 y quedan 70 pagables. Pre-F2a (compensación ignorada) el faltante
        // neto del periodo era 60 y el absorbible 30.
        var a = CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 90, jornadaNeta: 480, neto: 420);
        a.MinutosCompensacionPermisoAprobados = 40;
        db.RrhhAsistencias.Add(a);
        await db.SaveChangesAsync();

        var service = CreateService();
        var resumen = await service.ObtenerResumenPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(20, resumen.MinutosFaltanteNetoPeriodo); // 60 − 40 compensación
        Assert.Equal(20, resumen.MinutosFaltanteAbsorbidoExtra);
        Assert.Equal(70, resumen.MinutosExtraAbsorbible); // 90 − 20
    }

    [Fact]
    public async Task Resumen_CuandoRetardoCubiertoPorPermiso_RetardoEfectivoCeroYExtraPagable()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // retardo=60, 1h de permiso con goce cubre el día (sin faltante), extra=30.
        // Pre-F2b el retardo detectado del periodo era 60 → el extra tapaba retardo y
        // absorbible=0. Post-F2b el permiso anula el retardo → absorbible=30.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 30, retardo: 60, jornadaNeta: 480, neto: 480));
        db.RrhhAusencias.Add(CrearPermisoConGoce(empresa.Id, empleado.Id, DiaUno, horas: 1m));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resumen = await service.ObtenerResumenPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(0, resumen.MinutosRetardoDetectado); // 60 − 60 permiso
        Assert.Equal(0, resumen.MinutosRetardoAbsorbidoExtra);
        Assert.Equal(30, resumen.MinutosExtraAbsorbible);
    }

    [Fact]
    public async Task Aplicar_CapPorExtraAbsorbible_RechazaSiSumaExcedeAbsorbible()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // extra=90, faltante=30 → absorbible=60. Pago 50 + banco 20 = 70 > 60.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 90, jornadaNeta: 480, neto: 450));
        await db.SaveChangesAsync();

        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            FechaReferencia = FechaReferencia,
            Resolucion = "MitadMitad",
            MinutosBasePago = 50,
            MinutosBaseBanco = 20, // 70 > 60 absorbible
            UsuarioActual = "tester"
        }));
    }

    [Fact]
    public async Task Aplicar_PersisteFaltanteNetoYAbsorbido_SegunNeteo()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        // extra=90, faltante=30 → absorbible=60. Autorizo 60 a banco.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 90, jornadaNeta: 480, neto: 450));
        await db.SaveChangesAsync();

        var service = CreateService();
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            FechaReferencia = FechaReferencia,
            Resolucion = "BancoTodo",
            MinutosBaseBanco = 60,
            UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.Equal(30, periodo.MinutosFaltanteNetoDetectado);
        Assert.Equal(30, periodo.MinutosFaltanteAbsorbidoExtra);
        Assert.Equal(60, periodo.MinutosExtraBanco);
    }

    // ── Fase 3: el sobrante de extra tras faltante tapa el retardo del periodo ──

    [Fact]
    public async Task Resumen_CuandoExtraMenorQueRetardo_TrasFaltante_TodoAbsorbeYCeroPagable()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // Sin faltante, retardo=60, extra=30 → todo el extra tapa retardo, cero pagable.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 30, retardo: 60, jornadaNeta: 480, neto: 480));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resumen = await service.ObtenerResumenPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(60, resumen.MinutosRetardoDetectado);
        Assert.Equal(30, resumen.MinutosRetardoAbsorbidoExtra);
        Assert.Equal(0, resumen.MinutosExtraAbsorbible);
    }

    [Fact]
    public async Task Resumen_CuandoExtraMayorQueFaltanteYRetardo_SobraAbsorbible()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // faltante=30, retardo=20, extra=90 → sobrante 60 tras faltante − 20 retardo = 40 absorbible.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 90, retardo: 20, jornadaNeta: 480, neto: 450));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resumen = await service.ObtenerResumenPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(30, resumen.MinutosFaltanteAbsorbidoExtra);
        Assert.Equal(20, resumen.MinutosRetardoAbsorbidoExtra);
        Assert.Equal(40, resumen.MinutosExtraAbsorbible);
    }

    [Fact]
    public async Task Aplicar_CapPorExtraAbsorbibleConRetardo_RechazaSiSumaExcedeAbsorbible()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // faltante=30, retardo=20, extra=90 → absorbible=40. Pago 30 + banco 20 = 50 > 40.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 90, retardo: 20, jornadaNeta: 480, neto: 450));
        await db.SaveChangesAsync();

        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            FechaReferencia = FechaReferencia,
            Resolucion = "MitadMitad",
            MinutosBasePago = 30,
            MinutosBaseBanco = 20, // 50 > 40 absorbible
            UsuarioActual = "tester"
        }));
    }

    [Fact]
    public async Task Aplicar_PersisteFaltanteYRetardoAbsorbido_SegunNeteo()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        // faltante=60, retardo=20, extra=90 → faltanteAbsorbido=60, retardoAbsorbido=20, absorbible=10.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 90, retardo: 20, jornadaNeta: 480, neto: 420));
        await db.SaveChangesAsync();

        var service = CreateService();
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            FechaReferencia = FechaReferencia,
            Resolucion = "BancoTodo",
            MinutosBaseBanco = 10,
            UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.Equal(60, periodo.MinutosFaltanteAbsorbidoExtra);
        Assert.Equal(20, periodo.MinutosRetardoAbsorbidoExtra);
        Assert.Equal(10, periodo.MinutosExtraBanco);
    }

    // ── Fase 4: el sobrante de extra tras faltante+retardo repone el banco consumido ──

    [Fact]
    public async Task Resumen_CuandoExtraMenorQueBancoConsumido_TodoRestauraYCeroPagable()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        // 120 min de consumo (permiso-banco), 120 min de extra, sin faltante/retardo.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 120, jornadaNeta: 480, neto: 480));
        db.RrhhBancoHorasMovimientos.Add(CrearConsumoBanco(empresa.Id, empleado.Id, DiaUno, 120, $"permiso-banco:{Guid.NewGuid():N}"));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resumen = await service.ObtenerResumenPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(120, resumen.MinutosBancoConsumidoPeriodo);
        Assert.Equal(120, resumen.MinutosBancoRestauradoExtra);
        Assert.Equal(0, resumen.MinutosExtraAbsorbible);
    }

    [Fact]
    public async Task Resumen_CuandoExtraMayorQueFaltanteRetardoYConsumido_SobraAbsorbible()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        // faltante=30, retardo=20, consumo=60, extra=200 → sobrante 150 − 60 = 90 absorbible.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 200, retardo: 20, jornadaNeta: 480, neto: 450));
        db.RrhhBancoHorasMovimientos.Add(CrearConsumoBanco(empresa.Id, empleado.Id, DiaUno, 60, $"permiso-banco:{Guid.NewGuid():N}"));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resumen = await service.ObtenerResumenPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(30, resumen.MinutosFaltanteAbsorbidoExtra);
        Assert.Equal(20, resumen.MinutosRetardoAbsorbidoExtra);
        Assert.Equal(60, resumen.MinutosBancoRestauradoExtra);
        Assert.Equal(90, resumen.MinutosExtraAbsorbible);
    }

    [Fact]
    public async Task Resumen_CoberturaBancoSeExcluye_NoDobleConteoConFaltante()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        // Faltante crudo de 240 cubierto por banco (cobertura-banco, SIN RrhhAusencias),
        // + 60 de consumo por permiso-banco. extra=240.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 240, jornadaNeta: 480, neto: 240));
        db.RrhhBancoHorasMovimientos.Add(CrearConsumoBanco(empresa.Id, empleado.Id, DiaUno, 240, $"Asistencia:{Guid.NewGuid():N}:cobertura-banco"));
        db.RrhhBancoHorasMovimientos.Add(CrearConsumoBanco(empresa.Id, empleado.Id, DiaUno, 60, $"permiso-banco:{Guid.NewGuid():N}"));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resumen = await service.ObtenerResumenPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        // F2: el faltante (240) lo cuenta como faltante neto (no hay permiso que lo descuente).
        Assert.Equal(240, resumen.MinutosFaltanteNetoPeriodo);
        Assert.Equal(240, resumen.MinutosFaltanteAbsorbidoExtra); // el extra tapó el faltante
        // F4: solo cuenta el consumo de permiso-banco (60), NO el cobertura-banco (240).
        Assert.Equal(60, resumen.MinutosBancoConsumidoPeriodo);
        Assert.Equal(0, resumen.MinutosBancoRestauradoExtra); // no sobró extra tras faltante
        Assert.Equal(0, resumen.MinutosExtraAbsorbible);
    }

    [Fact]
    public async Task Aplicar_CreaMovimientoRestauracion_YEsIdempotente()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        // consumo=120, extra=200 → restaurado=120, absorbible=80. Autorizo 80 al banco.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 200, jornadaNeta: 480, neto: 480));
        db.RrhhBancoHorasMovimientos.Add(CrearConsumoBanco(empresa.Id, empleado.Id, DiaUno, 120, $"permiso-banco:{Guid.NewGuid():N}"));
        await db.SaveChangesAsync();

        var service = CreateService();
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "BancoTodo", MinutosBaseBanco = 80, UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        var refRestauracion = $"Periodo:{empleado.Id:N}:Semanal-2026-01:restauracion-banco";
        var refExtra = $"Periodo:{empleado.Id:N}:Semanal-2026-01:extra-banco";
        var restauracion = await db.RrhhBancoHorasMovimientos.SingleAsync(m => m.ReferenciaTipo == refRestauracion && m.IsActive);
        Assert.Equal(2m, restauracion.Horas); // 120 min / 60
        var extra = await db.RrhhBancoHorasMovimientos.SingleAsync(m => m.ReferenciaTipo == refExtra && m.IsActive);
        Assert.Equal(80m / 60m, extra.Horas);

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.Equal(120, periodo.MinutosBancoConsumidoDetectado);
        Assert.Equal(120, periodo.MinutosBancoRestauradoExtra);

        // Re-autorización con menos banco: la restauración NO cambia (consumo igual),
        // el movimiento extra-banco se reemplaza (no duplica).
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "BancoTodo", MinutosBaseBanco = 40, UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        var restauraciones = await db.RrhhBancoHorasMovimientos.Where(m => m.ReferenciaTipo == refRestauracion && m.IsActive).ToListAsync();
        var extras = await db.RrhhBancoHorasMovimientos.Where(m => m.ReferenciaTipo == refExtra && m.IsActive).ToListAsync();
        Assert.Single(restauraciones);
        Assert.Equal(2m, restauraciones[0].Horas); // 120 min, sin duplicar
        Assert.Single(extras);
        Assert.Equal(40m / 60m, extras[0].Horas);
    }

    [Fact]
    public async Task Aplicar_CapPorExtraAbsorbibleConBancoConsumido_RechazaSiSumaExcede()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        // consumo=120, extra=150 → restaurado=120, absorbible=30. Pago 20 + banco 20 = 50 > 30.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 150, jornadaNeta: 480, neto: 480));
        db.RrhhBancoHorasMovimientos.Add(CrearConsumoBanco(empresa.Id, empleado.Id, DiaUno, 120, $"permiso-banco:{Guid.NewGuid():N}"));
        await db.SaveChangesAsync();

        var service = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "MitadMitad", MinutosBasePago = 20, MinutosBaseBanco = 20, UsuarioActual = "tester"
        }));
    }

    [Fact]
    public async Task Reabrir_EliminaMovimientoRestauracionYExtra()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 200, jornadaNeta: 480, neto: 480));
        db.RrhhBancoHorasMovimientos.Add(CrearConsumoBanco(empresa.Id, empleado.Id, DiaUno, 120, $"permiso-banco:{Guid.NewGuid():N}"));
        await db.SaveChangesAsync();

        var service = CreateService();
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "BancoTodo", MinutosBaseBanco = 80, UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        await service.ReabrirPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia, "sistema");
        await db.SaveChangesAsync();

        var refPrefijo = $"Periodo:{empleado.Id:N}:Semanal-2026-01:";
        var movimientos = await db.RrhhBancoHorasMovimientos
            .Where(m => m.ReferenciaTipo != null && m.ReferenciaTipo.StartsWith(refPrefijo) && m.IsActive)
            .ToListAsync();
        Assert.Empty(movimientos); // ni extra-banco ni restauracion-banco
    }

    [Fact]
    public async Task Resumen_BancoDeshabilitado_NoRestauraBanco()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: false);
        // Existe consumo, pero el banco está deshabilitado → no se netea.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 200, jornadaNeta: 480, neto: 480));
        db.RrhhBancoHorasMovimientos.Add(CrearConsumoBanco(empresa.Id, empleado.Id, DiaUno, 120, $"permiso-banco:{Guid.NewGuid():N}"));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resumen = await service.ObtenerResumenPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia);

        Assert.Equal(0, resumen.MinutosBancoConsumidoPeriodo);
        Assert.Equal(0, resumen.MinutosBancoRestauradoExtra);
        Assert.Equal(200, resumen.MinutosExtraAbsorbible); // el extra queda 100% pagable
    }

    // ── Fase 5: split dobles/triples del PAGO persistido + techo configurable ──

    [Fact]
    public async Task Aplicar_CuandoPagoMenorQueTope_TodoDoblesCeroTriples()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // Tope default 9h = 540 min. Pago 200 min (< tope) → todo dobles.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 200));
        await db.SaveChangesAsync();

        var service = CreateService();
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "PagarTodo", MinutosBasePago = 200, UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.Equal(200, periodo.MinutosExtraDobles);
        Assert.Equal(0, periodo.MinutosExtraTriples);
    }

    [Fact]
    public async Task Aplicar_CuandoPagoExcedeTope_DoblesAlTopeYRestoTriples()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // Tope default 9h = 540 min. Pago 700 min → dobles 540, triples 160.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 700));
        await db.SaveChangesAsync();

        var service = CreateService();
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "PagarTodo", MinutosBasePago = 700, UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.Equal(540, periodo.MinutosExtraDobles); // 9h * 60
        Assert.Equal(160, periodo.MinutosExtraTriples);
    }

    [Fact]
    public async Task Aplicar_BancoNoSeReparteEnDoblesTriples()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        // Pago 200 + banco 100. El split es solo del pago; el banco queda aparte.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 300));
        await db.SaveChangesAsync();

        var service = CreateService();
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "MitadMitad", MinutosBasePago = 200, MinutosBaseBanco = 100, UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.Equal(200, periodo.MinutosExtraDobles); // split solo del pago (200 < tope 540)
        Assert.Equal(0, periodo.MinutosExtraTriples);
        Assert.Equal(100, periodo.MinutosExtraBanco);  // banco sin repartir
    }

    [Fact]
    public async Task Aplicar_TopeConfigurableCambiaElSplit()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // Tope custom 3h = 180 min. Pago 300 → dobles 180, triples 120.
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.HorasExtraDoblesPorSemana, "3"));
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 300));
        await db.SaveChangesAsync();

        var service = CreateService();
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "PagarTodo", MinutosBasePago = 300, UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.Equal(180, periodo.MinutosExtraDobles); // 3h * 60, no 9h
        Assert.Equal(120, periodo.MinutosExtraTriples);
    }

    [Fact]
    public async Task Resumen_SuperfaceSplitPersistidoCuandoAutorizado()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 700));
        await db.SaveChangesAsync();

        var service = CreateService();
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "PagarTodo", MinutosBasePago = 700, UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        var resumen = await service.ObtenerResumenPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia);
        Assert.Equal(540, resumen.MinutosExtraDobles);
        Assert.Equal(160, resumen.MinutosExtraTriples);
    }

    // ── F9: descartar el extra (aceptar sin pagar/compensar → descuento completo) ──

    [Fact]
    public async Task Aplicar_DescartarExtra_AceptaSinPagarYAnulaNeteo_DescuentoCompleto()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        // faltante=30, retardo=20, extra=90 → normalmente faltanteAbsorbido=30,
        // retardoAbsorbido=20, absorbible=40. Con DescartarExtra el neteo se anula:
        // absorbidos=0 → sourcing descuenta el faltante/retardo COMPLETO.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 90, retardo: 20, jornadaNeta: 480, neto: 450));
        await db.SaveChangesAsync();

        var service = CreateService();
        var result = await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id,
            EmpleadoId = empleado.Id,
            FechaReferencia = FechaReferencia,
            Resolucion = "Descartado",
            DescartarExtra = true,
            UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        // El periodo queda Autorizada (desbloquea el gate de prenómina) pero sin pago.
        Assert.Equal(RrhhResolucionPeriodoEstatus.Autorizada, result.Periodo.Estatus);
        Assert.True(result.Periodo.ExtraDescartado);
        Assert.Equal(0, result.MinutosBasePagoAplicados);
        Assert.Equal(0, result.MinutosBaseBancoAplicados);
        Assert.Equal(0, result.MinutosPagoAplicados);
        Assert.Equal(0, result.MinutosBancoAplicados);

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.True(periodo.ExtraDescartado);
        Assert.Equal(0, periodo.MinutosExtraPago);
        Assert.Equal(0, periodo.MinutosExtraBanco);
        // Neteo anulado: los absorbidos quedan en 0 → sourcing descuenta completo.
        Assert.Equal(0, periodo.MinutosFaltanteAbsorbidoExtra);
        Assert.Equal(0, periodo.MinutosRetardoAbsorbidoExtra);
        Assert.Equal(0, periodo.MinutosBancoRestauradoExtra);
        // La detección sigue persistida (es de solo lectura).
        Assert.Equal(90, periodo.MinutosExtraDetectado);
        Assert.Equal(30, periodo.MinutosFaltanteNetoDetectado);
        Assert.Equal(20, periodo.MinutosRetardoDetectado);

        // Sin movimientos de banco (ni extra-banco ni restauracion-banco).
        var refPrefijo = $"Periodo:{empleado.Id:N}:Semanal-2026-01:";
        var movimientos = await db.RrhhBancoHorasMovimientos
            .Where(m => m.ReferenciaTipo != null && m.ReferenciaTipo.StartsWith(refPrefijo) && m.IsActive)
            .ToListAsync();
        Assert.Empty(movimientos);
    }

    [Fact]
    public async Task Aplicar_DescartarExtra_RechazaSiSeEnviaPagoBancoOLineas()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 90));
        await db.SaveChangesAsync();

        var service = CreateService();

        // Descartar + pago base → rechazo.
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "Descartado", DescartarExtra = true, MinutosBasePago = 40, UsuarioActual = "tester"
        }));

        // Descartar + líneas → rechazo.
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "Descartado", DescartarExtra = true, UsuarioActual = "tester",
            Lineas = [new() { Destino = RrhhDestinoTiempoExtraLinea.Pago, Minutos = 40, Factor = 2m }]
        }));
    }

    [Fact]
    public async Task Reabrir_LimpiaFlagExtraDescartado()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 90, retardo: 20, jornadaNeta: 480, neto: 450));
        await db.SaveChangesAsync();

        var service = CreateService();
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "Descartado", DescartarExtra = true, UsuarioActual = "tester"
        });
        await db.SaveChangesAsync();

        await service.ReabrirPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia, "sistema");
        await db.SaveChangesAsync();

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.Equal(RrhhResolucionPeriodoEstatus.Reabierta, periodo.Estatus);
        Assert.False(periodo.ExtraDescartado);
    }

    private static CrmDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new CrmDbContext(options);
    }

    // ── Fase 8: autorización por líneas (varios factores/destinos) ──

    [Fact]
    public async Task AplicarPorLineas_DerivaEscalaresYPersisteLineas()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        // 300 min de extra, sin faltante/retardo → absorbible 300.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 300));
        await db.SaveChangesAsync();

        var service = CreateService();
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "PorLineas", UsuarioActual = "tester",
            Lineas =
            [
                new() { Destino = RrhhDestinoTiempoExtraLinea.Pago, Minutos = 120, Factor = 2m },
                new() { Destino = RrhhDestinoTiempoExtraLinea.Pago, Minutos = 180, Factor = 1m }
            ]
        });
        await db.SaveChangesAsync();

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo
            .Include(p => p.Lineas)
            .SingleAsync();

        Assert.Equal(RrhhResolucionPeriodoEstatus.Autorizada, periodo.Estatus);
        Assert.Equal(300, periodo.MinutosExtraPago);        // Σ pago.Minutos
        Assert.Equal(0, periodo.MinutosExtraBanco);
        Assert.Equal(120, periodo.MinutosExtraDobles);       // factor 2
        Assert.Equal(0, periodo.MinutosExtraTriples);
        Assert.Equal(180, periodo.MinutosExtraSimples);      // factor 1 (no dobles ni triples)
        Assert.Equal(7m, periodo.HorasExtraFactoradas);      // 120/60×2 + 180/60×1 = 4 + 3
        Assert.Null(periodo.FactorTiempoExtraAplicado);       // con líneas, el factor único no aplica
        Assert.Null(periodo.FactorAcumulacionBancoHorasAplicado);
        Assert.Equal(2, periodo.Lineas.Count);
        Assert.Equal(RrhhDestinoTiempoExtraLinea.Pago, periodo.Lineas[0].Destino);
        Assert.Equal(120, periodo.Lineas[0].Minutos);
        Assert.Equal(2m, periodo.Lineas[0].Factor);
        Assert.Equal(180, periodo.Lineas[1].Minutos);
        Assert.Equal(1m, periodo.Lineas[1].Factor);
    }

    [Fact]
    public async Task AplicarPorLineas_ReautorizacionReemplazaLineasPrevias()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 300));
        await db.SaveChangesAsync();

        var service = CreateService();
        // Primera: 1 línea.
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "PorLineas", UsuarioActual = "tester",
            Lineas = [new() { Destino = RrhhDestinoTiempoExtraLinea.Pago, Minutos = 300, Factor = 2m }]
        });
        await db.SaveChangesAsync();

        // Re-autorización: 2 líneas. Debe reemplazar (no acumular) las previas.
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "PorLineas", UsuarioActual = "tester",
            Lineas =
            [
                new() { Destino = RrhhDestinoTiempoExtraLinea.Pago, Minutos = 120, Factor = 2m },
                new() { Destino = RrhhDestinoTiempoExtraLinea.Pago, Minutos = 180, Factor = 1m }
            ]
        });
        await db.SaveChangesAsync();

        var lineas = await db.RrhhResolucionesTiempoExtraLinea.ToListAsync();
        Assert.Equal(2, lineas.Count); // reemplazó, no acumuló
    }

    [Fact]
    public async Task AplicarPorLineas_CapExcedeAbsorbibleRechaza()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        // extra 60, sin faltante → absorbible 60. Líneas suman 70 → rechazo.
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 60));
        await db.SaveChangesAsync();

        var service = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "PorLineas", UsuarioActual = "tester",
            Lineas =
            [
                new() { Destino = RrhhDestinoTiempoExtraLinea.Pago, Minutos = 50, Factor = 2m },
                new() { Destino = RrhhDestinoTiempoExtraLinea.Banco, Minutos = 20, Factor = 1m }
            ]
        }));
    }

    [Fact]
    public async Task AplicarPorLineas_BancoConFactorPorLineaGeneraMovimientoPonderado()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 60));
        await db.SaveChangesAsync();

        var service = CreateService();
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "PorLineas", UsuarioActual = "tester",
            Lineas = [new() { Destino = RrhhDestinoTiempoExtraLinea.Banco, Minutos = 60, Factor = 1.5m }]
        });
        await db.SaveChangesAsync();

        var referencia = $"Periodo:{empleado.Id:N}:Semanal-2026-01:extra-banco";
        var movimiento = await db.RrhhBancoHorasMovimientos.SingleAsync(m => m.ReferenciaTipo == referencia);
        // 60 min × factor 1.5 = 90 min factorados → 1.5 h en el ledger.
        Assert.Equal(1.5m, movimiento.Horas);

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.Equal(60, periodo.MinutosExtraBanco);
        Assert.Equal(0m, periodo.HorasExtraFactoradas); // solo banco, sin pago
    }

    [Fact]
    public async Task Reabrir_BorraLineasDeLaResolucion()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        db.RrhhAsistencias.Add(CrearAsistencia(empresa.Id, empleado.Id, DiaUno, minutosExtra: 300));
        await db.SaveChangesAsync();

        var service = CreateService();
        await service.AplicarResolucionPeriodoAsync(db, new RrhhResolucionPeriodoCommand
        {
            EmpresaId = empresa.Id, EmpleadoId = empleado.Id, FechaReferencia = FechaReferencia,
            Resolucion = "PorLineas", UsuarioActual = "tester",
            Lineas =
            [
                new() { Destino = RrhhDestinoTiempoExtraLinea.Pago, Minutos = 120, Factor = 2m },
                new() { Destino = RrhhDestinoTiempoExtraLinea.Pago, Minutos = 180, Factor = 1m }
            ]
        });
        await db.SaveChangesAsync();
        Assert.Equal(2, await db.RrhhResolucionesTiempoExtraLinea.CountAsync());

        await service.ReabrirPeriodoAsync(db, empresa.Id, empleado.Id, FechaReferencia, "sistema");
        await db.SaveChangesAsync();

        Assert.Equal(0, await db.RrhhResolucionesTiempoExtraLinea.CountAsync());
        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.Equal(RrhhResolucionPeriodoEstatus.Reabierta, periodo.Estatus);
        Assert.Equal(0m, periodo.HorasExtraFactoradas);
        Assert.Null(periodo.FactorTiempoExtraAplicado);
    }

    // ── Fase 9: backfill de líneas en resoluciones Autorizada pre-Fase 8 ──

    /// <summary>
    /// Path config (factor null, backfill desde daily): reconstruye dobles+triples por
    /// el tope con los factores de configuración. El monto (factoradas) coincide con la
    /// fórmula legada (dobles×F2 + triples×F3).
    /// </summary>
    [Fact]
    public async Task BackfillLineas_PathConfig_SiembraDoblesYTriplesSegunTope()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.FactorHoraExtraTriple, "3"));
        // Periodo Autorizada pre-Fase 8: pago 700, sin split persistido, factor null.
        db.RrhhResolucionesTiempoExtraPeriodo.Add(CrearResolucionAutorizada(
            empresa.Id, empleado.Id, minutosExtraPago: 700, minutosExtraDobles: 0, minutosExtraTriples: 0,
            factorAplicado: null, minutosExtraBanco: 0));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resultado = await service.SembrarLineasEnResolucionesAutorizadasAsync(db, null, "backfill");
        await db.SaveChangesAsync();

        Assert.Equal(1, resultado.PeriodosProcesados);
        Assert.Equal(2, resultado.LineasCreadas);

        var lineas = await db.RrhhResolucionesTiempoExtraLinea
            .OrderBy(l => l.Orden).ToListAsync();
        Assert.Equal(2, lineas.Count);
        Assert.Equal(RrhhDestinoTiempoExtraLinea.Pago, lineas[0].Destino);
        Assert.Equal(540, lineas[0].Minutos);   // tope 9h × 60
        Assert.Equal(2m, lineas[0].Factor);      // FactorHoraExtra
        Assert.Equal(160, lineas[1].Minutos);   // 700 − 540
        Assert.Equal(3m, lineas[1].Factor);       // FactorHoraExtraTriple

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.Equal(540, periodo.MinutosExtraDobles);
        Assert.Equal(160, periodo.MinutosExtraTriples);
        Assert.Equal(0, periodo.MinutosExtraSimples);
        Assert.Null(periodo.FactorTiempoExtraAplicado);

        var factoradasEsperada = 540m / 60m * 2m + 160m / 60m * 3m; // 18 + 8 = 26
        Assert.Equal(factoradasEsperada, periodo.HorasExtraFactoradas);
        Assert.Equal(26m, periodo.HorasExtraFactoradas);
    }

    /// <summary>
    /// Path override (FactorTiempoExtraAplicado set): el legado aplicaba ese factor a
    /// dobles Y triples → dos líneas @ ese factor. factoradas = (dobles+triples)/60 × F.
    /// </summary>
    [Fact]
    public async Task BackfillLineas_PathOverride_SiembraDoblesYTriplesConFactorAplicado()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhResolucionesTiempoExtraPeriodo.Add(CrearResolucionAutorizada(
            empresa.Id, empleado.Id, minutosExtraPago: 700, minutosExtraDobles: 540, minutosExtraTriples: 160,
            factorAplicado: 2.5m, minutosExtraBanco: 0));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resultado = await service.SembrarLineasEnResolucionesAutorizadasAsync(db, null, "backfill");
        await db.SaveChangesAsync();

        Assert.Equal(1, resultado.PeriodosProcesados);
        Assert.Equal(2, resultado.LineasCreadas);

        var lineas = await db.RrhhResolucionesTiempoExtraLinea
            .OrderBy(l => l.Orden).ToListAsync();
        Assert.Equal(540, lineas[0].Minutos);
        Assert.Equal(2.5m, lineas[0].Factor);
        Assert.Equal(160, lineas[1].Minutos);
        Assert.Equal(2.5m, lineas[1].Factor);

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        // factoradas = (540+160)/60 × 2.5 = 700/60 × 2.5 = 29.1666...
        var factoradasEsperada = 540m / 60m * 2.5m + 160m / 60m * 2.5m;
        Assert.Equal(factoradasEsperada, periodo.HorasExtraFactoradas);
        Assert.Null(periodo.FactorTiempoExtraAplicado); // conmutó al path por líneas
    }

    /// <summary>
    /// Sembrar también una línea de banco cuando el periodo tiene MinutosExtraBanco > 0,
    /// con el factor de acumulación persistido (FactorAcumulacionBancoHorasAplicado).
    /// </summary>
    [Fact]
    public async Task BackfillLineas_IncluyeLineaBanco_ConFactorAplicado()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db, bancoHabilitado: true, factorAcumulacion: 1m);
        db.RrhhResolucionesTiempoExtraPeriodo.Add(CrearResolucionAutorizada(
            empresa.Id, empleado.Id, minutosExtraPago: 300, minutosExtraDobles: 300, minutosExtraTriples: 0,
            factorAplicado: 2m, minutosExtraBanco: 60, factorAcumulacionBancoAplicado: 1.5m));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resultado = await service.SembrarLineasEnResolucionesAutorizadasAsync(db, null, "backfill");
        await db.SaveChangesAsync();

        Assert.Equal(1, resultado.PeriodosProcesados);
        Assert.Equal(2, resultado.LineasCreadas); // 1 pago + 1 banco

        var banco = await db.RrhhResolucionesTiempoExtraLinea
            .SingleAsync(l => l.Destino == RrhhDestinoTiempoExtraLinea.Banco);
        Assert.Equal(60, banco.Minutos);
        Assert.Equal(1.5m, banco.Factor);

        var periodo = await db.RrhhResolucionesTiempoExtraPeriodo.SingleAsync();
        Assert.Equal(300m / 60m * 2m, periodo.HorasExtraFactoradas); // solo el pago aporta
        Assert.Null(periodo.FactorAcumulacionBancoHorasAplicado);
    }

    /// <summary>
    /// Idempotencia: la segunda corrida no procesa periodos que ya tienen líneas.
    /// </summary>
    [Fact]
    public async Task BackfillLineas_EsIdempotente_SegundaCorridaNoProcesa()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhResolucionesTiempoExtraPeriodo.Add(CrearResolucionAutorizada(
            empresa.Id, empleado.Id, minutosExtraPago: 300, minutosExtraDobles: 0, minutosExtraTriples: 0,
            factorAplicado: null, minutosExtraBanco: 0));
        await db.SaveChangesAsync();

        var service = CreateService();
        var primera = await service.SembrarLineasEnResolucionesAutorizadasAsync(db, null, "backfill");
        await db.SaveChangesAsync();
        Assert.Equal(1, primera.PeriodosProcesados);

        var segunda = await service.SembrarLineasEnResolucionesAutorizadasAsync(db, null, "backfill");
        await db.SaveChangesAsync();
        Assert.Equal(0, segunda.PeriodosProcesados);
        Assert.Equal(1, segunda.PeriodosOmitidos);
        Assert.Equal(0, segunda.LineasCreadas);
    }

    /// <summary>
    /// Periodo Autorizada sin pago ni banco: nada que sembrar → omitido (no crea líneas).
    /// </summary>
    [Fact]
    public async Task BackfillLineas_SinPagoNiBanco_QuedaOmitido()
    {
        await using var db = CreateDbContext();
        var (empresa, empleado) = await SembrarAsync(db);
        db.RrhhResolucionesTiempoExtraPeriodo.Add(CrearResolucionAutorizada(
            empresa.Id, empleado.Id, minutosExtraPago: 0, minutosExtraDobles: 0, minutosExtraTriples: 0,
            factorAplicado: null, minutosExtraBanco: 0));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resultado = await service.SembrarLineasEnResolucionesAutorizadasAsync(db, null, "backfill");
        await db.SaveChangesAsync();

        Assert.Equal(0, resultado.PeriodosProcesados);
        Assert.Equal(1, resultado.PeriodosOmitidos);
        Assert.Equal(0, await db.RrhhResolucionesTiempoExtraLinea.CountAsync());
    }

    /// <summary>
    /// Filtro por empresa: solo procesa los periodos de la empresa indicada.
    /// </summary>
    [Fact]
    public async Task BackfillLineas_FiltroEmpresa_SoloProcesaEsaEmpresa()
    {
        await using var db = CreateDbContext();
        var (empresa1, empleado1) = await SembrarAsync(db);
        var (empresa2, empleado2) = await SembrarAsync(db);
        db.RrhhResolucionesTiempoExtraPeriodo.Add(CrearResolucionAutorizada(
            empresa1.Id, empleado1.Id, minutosExtraPago: 300, factorAplicado: null));
        db.RrhhResolucionesTiempoExtraPeriodo.Add(CrearResolucionAutorizada(
            empresa2.Id, empleado2.Id, minutosExtraPago: 300, factorAplicado: null));
        await db.SaveChangesAsync();

        var service = CreateService();
        var resultado = await service.SembrarLineasEnResolucionesAutorizadasAsync(db, empresa1.Id, "backfill");
        await db.SaveChangesAsync();

        Assert.Equal(1, resultado.PeriodosProcesados);
        Assert.Equal(1, await db.RrhhResolucionesTiempoExtraLinea.CountAsync());
    }

    private static RrhhResolucionTiempoExtraPeriodo CrearResolucionAutorizada(
        Guid empresaId, Guid empleadoId, int minutosExtraPago,
        int minutosExtraDobles = 0, int minutosExtraTriples = 0,
        decimal? factorAplicado = null, int minutosExtraBanco = 0,
        decimal? factorAcumulacionBancoAplicado = null)
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
            FechaInicio = new DateOnly(2025, 12, 29),
            FechaFin = new DateOnly(2026, 1, 4),
            MinutosExtraPago = minutosExtraPago,
            MinutosExtraBanco = minutosExtraBanco,
            MinutosExtraDobles = minutosExtraDobles,
            MinutosExtraTriples = minutosExtraTriples,
            HorasExtraFactoradas = 0m,
            FactorTiempoExtraAplicado = factorAplicado,
            FactorAcumulacionBancoHorasAplicado = factorAcumulacionBancoAplicado,
            Estatus = RrhhResolucionPeriodoEstatus.Autorizada,
            AutorizadoPor = "tester",
            FechaAutorizacion = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

    private static IRrhhResolucionPeriodoService CreateService()
        => new RrhhResolucionPeriodoService(new RrhhTiempoExtraResolutionService());

    private static async Task<(Empresa Empresa, Empleado Empleado)> SembrarAsync(
        CrmDbContext db, bool bancoHabilitado = false, decimal factorAcumulacion = 1m)
    {
        var empresa = CreateEmpresa();
        var empleado = CreateEmpleado(empresa.Id, TipoNomina.Semanal);
        db.Empresas.Add(empresa);
        db.Empleados.Add(empleado);
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.FactorHoraExtra, "2"));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.BancoHorasHabilitado, bancoHabilitado ? "true" : "false"));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.BancoHorasFactorAcumulacion, factorAcumulacion.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        db.AppConfigs.Add(CreateAppConfig(empresa.Id, ClavesConfiguracionNomina.BancoHorasTopeHoras, "45"));
        await db.SaveChangesAsync();
        return (empresa, empleado);
    }

    private static RrhhAsistencia CrearAsistencia(Guid empresaId, Guid empleadoId, DateOnly fecha, int minutosExtra = 0, int retardo = 0, int? jornadaNeta = null, int? neto = null)
    {
        var neta = jornadaNeta ?? 480;
        var netoVal = neto ?? 480;
        return new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            Fecha = fecha,
            MinutosTrabajadosNetos = netoVal,
            MinutosJornadaNetaProgramada = neta,
            MinutosExtra = minutosExtra,
            MinutosRetardo = retardo,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    private static RrhhAusencia CrearPermisoConGoce(Guid empresaId, Guid empleadoId, DateOnly fecha, decimal horas)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            Tipo = TipoAusenciaRrhh.PermisoConGoce,
            Estatus = EstatusAusenciaRrhh.Aprobada,
            FechaInicio = fecha,
            FechaFin = fecha,
            Dias = 1,
            Horas = horas,
            ConGocePago = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

    // Consumo del banco: Horas NEGATIVAS (convención del ledger). referenciaTipo
    // distingue el origen: "permiso-banco:..." (incluido en Fase 4) vs
    // "Asistencia:{id}:cobertura-banco" (excluido — Fase 2 ya lo cuenta).
    private static RrhhBancoHorasMovimiento CrearConsumoBanco(
        Guid empresaId, Guid empleadoId, DateOnly fecha, int minutos, string referenciaTipo)
        => new()
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            EmpleadoId = empleadoId,
            Fecha = fecha,
            TipoMovimiento = TipoMovimientoBancoHorasRrhh.Consumo,
            Horas = -(minutos / 60m),
            ReferenciaTipo = referenciaTipo,
            EsAutomatico = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

    private static Empresa CreateEmpresa() => new()
    {
        Id = Guid.NewGuid(),
        Codigo = $"EMP-{Guid.NewGuid():N}"[..12],
        RazonSocial = "Empresa Test"
    };

    private static Empleado CreateEmpleado(Guid empresaId, TipoNomina tipoNomina) => new()
    {
        Id = Guid.NewGuid(),
        EmpresaId = empresaId,
        Codigo = "EMP-001",
        NumeroEmpleado = "001",
        Nombre = "Empleado Test",
        CodigoChecador = "3001",
        TipoNomina = tipoNomina,
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