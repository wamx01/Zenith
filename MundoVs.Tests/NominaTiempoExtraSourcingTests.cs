using MundoVs.Core.Entities;
using MundoVs.Core.Services;

namespace MundoVs.Tests;

/// <summary>
/// Pruebas del sourcing de tiempo extra para la nómina (Fase 5.5 — cutover
/// nómina → resolución por periodo; Fase 7 — reutilizado por el snapshot de prenómina).
/// Helper puro, sin base de datos.
/// </summary>
public sealed class NominaTiempoExtraSourcingTests
{
    private static NominaConfiguracion Config(int doblesPorSemana = 9)
        => new() { HorasExtraDoblesPorSemana = doblesPorSemana };

    private static PrenominaDetalle Incidencia(
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

    private static NominaOvertimeSourcingInput Input(PrenominaDetalle? incidencia)
        => NominaTiempoExtraSourcing.InputFrom(incidencia ?? new PrenominaDetalle());

    private static RrhhResolucionTiempoExtraPeriodo Resolucion(
        RrhhResolucionPeriodoEstatus estatus,
        int minutosExtraPago = 0, int minutosExtraDetectado = 0, int minutosExtraBanco = 0,
        int minutosExtraDobles = 0, int minutosExtraTriples = 0,
        decimal? factorAplicado = null,
        int minutosFaltanteNeto = 0, int minutosFaltanteAbsorbido = 0,
        int minutosRetardoDetectado = 0, int minutosRetardoAbsorbido = 0,
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
            MinutosRetardoAbsorbidoExtra = minutosRetardoAbsorbido
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
        Assert.Equal(2m, s.HorasExtra);            // 120 min
        Assert.Equal(3m, s.HorasExtraBase);        // 180 min
        Assert.Equal(1m, s.HorasExtraBanco);       // 60 min
        Assert.Equal(2m, s.HorasExtraDobles);      // 120 min
        Assert.Equal(0m, s.HorasExtraTriples);
        Assert.Equal(2.5m, s.FactorPagoTiempoExtra);
        // Alivio: lo absorbido no se descuenta.
        Assert.Equal(0, s.MinutosFaltanteDescontable);   // 60 - 60
        Assert.Equal(0, s.MinutosRetardo);               // 30 - 30
        // Salida anticipada, descuento manual y perdon vienen de la incidencia.
        Assert.Equal(20, s.MinutosSalidaAnticipada);
        Assert.Equal(15, s.MinutosDescuentoManual);
        Assert.Equal(0, s.MinutosPerdonadosManual);
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
        Assert.Equal(20, s.MinutosFaltanteDescontable);
        Assert.Equal(10, s.MinutosRetardo);
        Assert.Equal(5, s.MinutosSalidaAnticipada);
        Assert.Equal(8, s.MinutosDescuentoManual);
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
    public void PeriodoAliviaFaltanteAbsorbido()
    {
        var resolucion = Resolucion(RrhhResolucionPeriodoEstatus.Autorizada,
            minutosFaltanteNeto: 100, minutosFaltanteAbsorbido: 40);

        var s = NominaTiempoExtraSourcing.Source(Input(Incidencia(minutosFaltanteDescontable: 200)),
            resolucion, Config(), factorPersistido: 0m);

        Assert.Equal(60, s.MinutosFaltanteDescontable); // 100 - 40, no 200
    }

    [Fact]
    public void PeriodoAliviaRetardoAbsorbido_SalidaAnticipadaIntacta()
    {
        var incidencia = Incidencia(minutosSalidaAnticipada: 25, minutosDescuentoManual: 12);
        var resolucion = Resolucion(RrhhResolucionPeriodoEstatus.Autorizada,
            minutosRetardoDetectado: 50, minutosRetardoAbsorbido: 20);

        var s = NominaTiempoExtraSourcing.Source(Input(incidencia), resolucion, Config(), factorPersistido: 0m);

        Assert.Equal(30, s.MinutosRetardo);            // 50 - 20
        Assert.Equal(25, s.MinutosSalidaAnticipada);   // intacta (no participa en el neteo)
        Assert.Equal(12, s.MinutosDescuentoManual);    // intacto
    }

    [Fact]
    public void PeriodoSinAbsorcion_DeduccionCompleta()
    {
        var resolucion = Resolucion(RrhhResolucionPeriodoEstatus.Autorizada,
            minutosFaltanteNeto: 80, minutosFaltanteAbsorbido: 0,
            minutosRetardoDetectado: 35, minutosRetardoAbsorbido: 0);

        var s = NominaTiempoExtraSourcing.Source(Input(Incidencia()),
            resolucion, Config(), factorPersistido: 0m);

        Assert.Equal(80, s.MinutosFaltanteDescontable);
        Assert.Equal(35, s.MinutosRetardo);
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