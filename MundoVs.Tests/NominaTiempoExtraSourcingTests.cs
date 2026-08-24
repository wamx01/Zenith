using MundoVs.Core.Entities;
using MundoVs.Core.Services;

namespace MundoVs.Tests;

/// <summary>
/// Pruebas del sourcing de tiempo extra para la nómina (Fase 5.5 — cutover
/// nómina → resolución por periodo; Fase 7 — reutilizado por el snapshot de nómina).
/// Helper puro, sin base de datos.
/// </summary>
public sealed class NominaTiempoExtraSourcingTests
{
    private static NominaConfiguracion Config(int doblesPorSemana = 9)
        => new() { HorasExtraDoblesPorSemana = doblesPorSemana };

    private static NominaDetalle Incidencia(
        decimal horasExtra = 0m, decimal horasExtraBase = 0m, decimal horasBanco = 0m,
        int minutosRetardo = 0, int minutosSalidaAnticipada = 0,
        int minutosPerdonadosManual = 0, int minutosFaltanteDescontable = 0,
        int minutosDescuentoManual = 0, decimal factor = 0m)
        => new()
        {
            EmpleadoId = Guid.NewGuid(),
            HorasExtra = horasExtra,
            HorasExtraBase = horasExtraBase,
            HorasBancoAcumuladas = horasBanco,
            MinutosRetardo = minutosRetardo,
            MinutosSalidaAnticipada = minutosSalidaAnticipada,
            MinutosPerdonadosManual = minutosPerdonadosManual,
            MinutosFaltanteDescontable = minutosFaltanteDescontable,
            MinutosDescuentoManual = minutosDescuentoManual,
            FactorPagoTiempoExtra = factor
        };

    private static NominaOvertimeSourcingInput Input(NominaDetalle? incidencia)
        => NominaTiempoExtraSourcing.InputFrom(incidencia ?? new NominaDetalle());

    private static RrhhResolucionTiempoExtraPeriodo Resolucion(
        RrhhResolucionPeriodoEstatus estatus,
        int minutosExtraPago = 0, int minutosExtraDetectado = 0, int minutosExtraBanco = 0,
        int minutosExtraDobles = 0, int minutosExtraTriples = 0,
        decimal? factorAplicado = null,
        int minutosFaltanteNeto = 0, int minutosFaltanteAbsorbido = 0,
        int minutosRetardoDetectado = 0, int minutosRetardoAbsorbido = 0,
        int minutosSalidaAnticipadaDetectado = 0, int minutosSalidaAnticipadaAbsorbido = 0,
        int minutosExtraSimples = 0, decimal horasExtraFactoradas = 0m)
        => new()
        {
            EmpresaId = Guid.NewGuid(),
            EmpleadoId = Guid.NewGuid(),
            Estatus = estatus,
            MinutosExtraPago = minutosExtraPago,
            MinutosExtraDetectado = minutosExtraDetectado,
            MinutosExtraBanco = minutosExtraBanco,
            MinutosExtraDobles = minutosExtraDobles,
            MinutosExtraTriples = minutosExtraTriples,
            MinutosExtraSimples = minutosExtraSimples,
            HorasExtraFactoradas = horasExtraFactoradas,
            FactorTiempoExtraAplicado = factorAplicado,
            MinutosFaltanteNetoDetectado = minutosFaltanteNeto,
            MinutosFaltanteAbsorbidoExtra = minutosFaltanteAbsorbido,
            MinutosRetardoDetectado = minutosRetardoDetectado,
            MinutosRetardoAbsorbidoExtra = minutosRetardoAbsorbido,
            MinutosSalidaAnticipadaDetectado = minutosSalidaAnticipadaDetectado,
            MinutosSalidaAnticipadaAbsorbidoExtra = minutosSalidaAnticipadaAbsorbido
        };

    [Fact]
    public void PeriodoAutorizado_UsaValoresDelPeriodo_NoIncidencia()
    {
        var incidencia = Incidencia(horasExtra: 5m, horasExtraBase: 4m, horasBanco: 1m,
            minutosRetardo: 40, minutosSalidaAnticipada: 20, minutosFaltanteDescontable: 80,
            minutosDescuentoManual: 15, factor: 9m);
        var resolucion = Resolucion(RrhhResolucionPeriodoEstatus.Autorizada,
            minutosExtraPago: 120, minutosExtraDetectado: 180, minutosExtraBanco: 60,
            minutosExtraDobles: 120, minutosExtraTriples: 0, factorAplicado: 2.5m,
            minutosFaltanteNeto: 60, minutosFaltanteAbsorbido: 60,
            minutosRetardoDetectado: 30, minutosRetardoAbsorbido: 30);

        var s = NominaTiempoExtraSourcing.Source(Input(incidencia), resolucion, Config(), factorPersistido: 0m);

        Assert.Equal("periodo", s.Origen);
        // El extra a PAGAR sí viene de la resolución autorizada (pago/banco/dobles/triples/factor).
        // English: The extra to PAY does come from the authorized resolution.
        Assert.Equal(2m, s.HorasExtra);            // 120 min
        Assert.Equal(3m, s.HorasExtraBase);        // 180 min
        Assert.Equal(1m, s.HorasExtraBanco);       // 60 min
        Assert.Equal(2m, s.HorasExtraDobles);      // 120 min
        Assert.Equal(0m, s.HorasExtraTriples);
        Assert.Equal(2.5m, s.FactorPagoTiempoExtra);
        // DEDUCCIONES: vienen del input (incidencia), ya neteadas por el snapshot (fuente única
        // = el batch canónico de Asistencia Semanal). El sourcing NO recomputa (detectado −
        // absorbido) desde la resolución, aunque sea Autorizada — el alivio autoritativo es el
        // VIVO del periodo, no el congelado al autorizar (que puede estar stale/zeroado por un
        // DescartarExtra). Así nómina = Asistencia Semanal. La resolución de arriba trae
        // faltanteAbsorbido=60 y retardoAbsorbido=30, pero el sourcing los IGNORA: pasa el
        // input (80/40/20) íntegro.
        // English: DEDUCTIONS come from the input (incidencia), already netted by the snapshot
        // (single source = Asistencia Semanal's canonical batch). Sourcing does NOT recompute
        // (detected − absorbed) from the resolution, even if Autorizada — the authoritative
        // relief is the LIVE one for the period, not the one frozen at authorization (which can
        // be stale/zeroed by a DescartarExtra). So nómina = Asistencia Semanal. The resolution
        // above carries faltanteAbsorbido=60 and retardoAbsorbido=30, but sourcing IGNORES them:
        // it passes the input (80/40/20) intact.
        Assert.Equal(80, s.MinutosFaltanteDescontable);   // input (80), NO 60−60=0
        Assert.Equal(40, s.MinutosRetardo);               // input (40), NO 30−30=0
        Assert.Equal(20, s.MinutosSalidaAnticipada);     // input (20), NO 0−0=0
        Assert.Equal(15, s.MinutosDescuentoManual);
        Assert.Equal(0, s.MinutosPerdonadosManual);
    }

    [Fact]
    public void PeriodoAutorizado_IgnoraAlivioZeroadoPorDescartarExtra()
    {
        // Caso Abigail #107 (bug 2026-08-22): resolución Autorizada con faltanteAbsorbido=0
        // porque el operador descartó el extra (DescartarExtra anula el PAGO, no el neteo de
        // deducciones). El sourcing ANTES recomputaba (detectado − absorbido) = 60 − 0 = 60
        // → la nómina divergía de Asistencia Semanal (que netea en VIVO → 0). Ahora el sourcing
        // toma las deducciones del input (snapshot neteado en vivo) → 0, igual que Asistencia
        // Semanal, aunque la resolución persistida tenga el alivio zeroado.
        // English: Abigail #107 (bug 2026-08-22): Autorizada resolution with faltanteAbsorbido=0
        // because the operator discarded the extra (DescartarExtra annuls the PAYMENT, not the
        // deduction neteo). Sourcing USED to recompute (detected − absorbed) = 60 − 0 = 60 →
        // nómina diverged from Asistencia Semanal (which nets LIVE → 0). Now sourcing takes
        // deductions from the input (live-netted snapshot) → 0, matching Asistencia Semanal,
        // even though the persisted resolution has the relief zeroed.
        var incidencia = Incidencia(minutosFaltanteDescontable: 0); // snapshot ya neteó a 0
        var resolucion = Resolucion(RrhhResolucionPeriodoEstatus.Autorizada,
            minutosExtraPago: 120, minutosExtraDetectado: 120,
            minutosFaltanteNeto: 60, minutosFaltanteAbsorbido: 0);  // zeroado por DescartarExtra

        var s = NominaTiempoExtraSourcing.Source(Input(incidencia), resolucion, Config(), factorPersistido: 0m);

        Assert.Equal("periodo", s.Origen);
        Assert.Equal(2m, s.HorasExtra);            // el extra a pagar sí viene de la resolución
        Assert.Equal(0, s.MinutosFaltanteDescontable); // input (0), NO 60−0=60
    }

    [Theory]
    [InlineData(RrhhResolucionPeriodoEstatus.Pendiente)]
    [InlineData(RrhhResolucionPeriodoEstatus.Reabierta)]
    public void PeriodoNoAutorizado_CaeAIncidencia(RrhhResolucionPeriodoEstatus estatus)
    {
        var incidencia = Incidencia(horasExtra: 3m, horasExtraBase: 3.5m, horasBanco: 0.5m,
            minutosRetardo: 10, minutosSalidaAnticipada: 5, minutosFaltanteDescontable: 20,
            minutosDescuentoManual: 8, factor: 0m);
        var resolucion = Resolucion(estatus,
            minutosExtraPago: 999, minutosExtraDobles: 999, factorAplicado: 9m,
            minutosFaltanteAbsorbido: 999, minutosRetardoAbsorbido: 999);

        var s = NominaTiempoExtraSourcing.Source(Input(incidencia), resolucion, Config(doblesPorSemana: 9), factorPersistido: 1.5m);

        Assert.Equal("incidencia", s.Origen);
        Assert.Equal(3m, s.HorasExtra);
        Assert.Equal(3.5m, s.HorasExtraBase);
        Assert.Equal(0.5m, s.HorasExtraBanco);
        // dobles = min(9, min(3.5, 3)) = 3; triples = 0
        Assert.Equal(3m, s.HorasExtraDobles);
        Assert.Equal(0m, s.HorasExtraTriples);
        Assert.Equal(1.5m, s.FactorPagoTiempoExtra); // preserva el persistido
        // El path incidencia es passthrough de las deducciones: el neteo NetoVsNeto ya quedó
        // aplicado en el snapshot (que consume el resumen canónico de Asistencia Semanal), así
        // que el sourcing NO recalcula — devuelve las sumas crudas congeladas. La nómina no
        // diverge de Asistencia Semanal porque el snapshot las neteó antes de congelar.
        // English: The incidencia path is passthrough for deductions: the NetoVsNeto netting was
        // already applied at the snapshot (which consumes Asistencia Semanal's canonical resumen),
        // so sourcing does NOT recompute — returns the frozen raw sums. Nómina doesn't diverge
        // from Asistencia Semanal because the snapshot netted them before freezing.
        Assert.Equal(20, s.MinutosFaltanteDescontable);   // crudo (el snapshot ya neteó)
        Assert.Equal(10, s.MinutosRetardo);
        Assert.Equal(5, s.MinutosSalidaAnticipada);
        Assert.Equal(8, s.MinutosDescuentoManual);
    }

    [Fact]
    public void Incidencia_SinExtra_NoNetea()
    {
        // Sin pool de extra el neteo no absorbe nada → las deducciones crudas pasan intactas
        // (passthrough). El neteo mismo vive en el snapshot, no aquí.
        // English: With no extra pool the netting absorbs nothing → raw deductions pass through
        // intact (passthrough). The netting itself lives in the snapshot, not here.
        var input = new NominaOvertimeSourcingInput
        {
            HorasExtra = 0m,
            HorasExtraBase = 0m,
            MinutosFaltanteDescontable = 10,
            MinutosRetardo = 4,
            MinutosSalidaAnticipada = 3
        };

        var s = NominaTiempoExtraSourcing.Source(input, resolucion: null, Config(), factorPersistido: 0m);

        Assert.Equal("incidencia", s.Origen);
        Assert.Equal(10, s.MinutosFaltanteDescontable); // sin extra → sin alivio
        Assert.Equal(4, s.MinutosRetardo);
        Assert.Equal(3, s.MinutosSalidaAnticipada);
    }

    [Fact]
    public void SinPeriodo_CaeAIncidencia()
    {
        var incidencia = Incidencia(horasExtra: 12m, horasExtraBase: 12m, factor: 0m);

        var s = NominaTiempoExtraSourcing.Source(Input(incidencia), resolucion: null, Config(doblesPorSemana: 9), factorPersistido: 0m);

        Assert.Equal("incidencia", s.Origen);
        Assert.Equal(9m, s.HorasExtraDobles);   // tope 9
        Assert.Equal(3m, s.HorasExtraTriples);  // 12 - 9
    }

    [Fact]
    public void Periodo_DoblesMasTriplesIgualHorasExtra()
    {
        var resolucion = Resolucion(RrhhResolucionPeriodoEstatus.Autorizada,
            minutosExtraPago: 600, minutosExtraDobles: 540, minutosExtraTriples: 60);

        var s = NominaTiempoExtraSourcing.Source(Input(null), resolucion, Config(), factorPersistido: 0m);

        Assert.Equal(10m, s.HorasExtra);
        Assert.Equal(9m, s.HorasExtraDobles);
        Assert.Equal(1m, s.HorasExtraTriples);
        Assert.Equal(s.HorasExtraDobles + s.HorasExtraTriples, s.HorasExtra);
    }

    [Fact]
    public void Periodo_BancoPreFactorEsInformacional()
    {
        var resolucion = Resolucion(RrhhResolucionPeriodoEstatus.Autorizada, minutosExtraBanco: 90);

        var s = NominaTiempoExtraSourcing.Source(Input(null), resolucion, Config(), factorPersistido: 0m);

        Assert.Equal(1.5m, s.HorasExtraBanco); // 90 min base, sin factor de acumulación
    }

    [Fact]
    public void Periodo_FactorNullCaeACeroParaConfig()
    {
        var resolucion = Resolucion(RrhhResolucionPeriodoEstatus.Autorizada, factorAplicado: null);

        var s = NominaTiempoExtraSourcing.Source(Input(null), resolucion, Config(), factorPersistido: 7m);

        Assert.Equal(0m, s.FactorPagoTiempoExtra); // el calculador cae a FactorHoraExtra/Triple de config
    }

    /// <summary>
    /// Fase 8 — resolución autorizada por líneas: el sourcing expone HorasExtraFactoradas
    /// (Σ pago.Minutos/60 × Factor) y FactorPagoTiempoExtra=0 (señal al calculador para usar
    /// factoradas). Dobles/triples vienen de los escalares derivados por línea.
    /// </summary>
    [Fact]
    public void PeriodoPorLineas_ExponeFactoradasYFactorCero()
    {
        // 2 líneas de pago: 120 min @ x2 (→ 4 h ponderadas) + 180 min @ x1 (→ 3 h ponderadas).
        // HorasExtraFactoradas = 4 + 3 = 7. Dobles = 120 min (factor 2), Simples = 180 min (factor 1).
        var resolucion = Resolucion(RrhhResolucionPeriodoEstatus.Autorizada,
            minutosExtraPago: 300, minutosExtraDobles: 120, minutosExtraTriples: 0,
            minutosExtraSimples: 180, horasExtraFactoradas: 7m);

        var s = NominaTiempoExtraSourcing.Source(Input(null), resolucion, Config(), factorPersistido: 0m);

        Assert.Equal("periodo", s.Origen);
        Assert.Equal(5m, s.HorasExtra);              // 300 min / 60
        Assert.Equal(7m, s.HorasExtraFactoradas);   // 4 (120/60×2) + 3 (180/60×1)
        Assert.Equal(2m, s.HorasExtraDobles);       // 120 min / 60
        Assert.Equal(0m, s.HorasExtraTriples);
        Assert.Equal(0m, s.FactorPagoTiempoExtra);  // señal: el calculador usa factoradas
    }

    [Fact]
    public void PeriodoPorLineas_LineaBancoSumaAlBanco()
    {
        // Solo banco: 60 min @ x1.5 (acumulación por línea). Sin pago → factoradas 0.
        var resolucion = Resolucion(RrhhResolucionPeriodoEstatus.Autorizada,
            minutosExtraBanco: 60, horasExtraFactoradas: 0m);

        var s = NominaTiempoExtraSourcing.Source(Input(null), resolucion, Config(), factorPersistido: 0m);

        Assert.Equal(0m, s.HorasExtra);             // sin pago
        Assert.Equal(1m, s.HorasExtraBanco);        // 60 min / 60 (base; el factor de acumulación lo aplica el ledger al autorizar)
        Assert.Equal(0m, s.HorasExtraFactoradas);   // no hay líneas de pago
        Assert.Equal(0m, s.FactorPagoTiempoExtra);   // sin líneas de pago, factoradas 0 → calculador cae a dobles/triples (0)
    }

    [Fact]
    public void PeriodoLegadoSinLineas_MantienePathEscalar()
    {
        // Resolución pre-Fase 8: sin HorasExtraFactoradas, con factor aplicado → path escalar.
        var resolucion = Resolucion(RrhhResolucionPeriodoEstatus.Autorizada,
            minutosExtraPago: 300, minutosExtraDobles: 300, factorAplicado: 2m,
            horasExtraFactoradas: 0m);

        var s = NominaTiempoExtraSourcing.Source(Input(null), resolucion, Config(), factorPersistido: 0m);

        Assert.Equal(5m, s.HorasExtra);
        Assert.Equal(0m, s.HorasExtraFactoradas);   // legado: no hay factoradas
        Assert.Equal(2m, s.FactorPagoTiempoExtra);  // legado: usa el factor aplicado
    }
}