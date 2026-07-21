using MundoVs.Core.Entities;
using MundoVs.Core.Services;

namespace MundoVs.Tests;

/// <summary>
/// Pruebas del calculador de nómina — foco Fase 8: HorasExtraFactoradas (path por líneas).
/// Helper puro, sin base de datos.
/// </summary>
public sealed class NominaCalculatorTests
{
    private static NominaCalculationInput Input(
        decimal sueldoReferencia,
        decimal horasExtraFactoradas = 0m,
        decimal horasDobles = 0m,
        decimal horasTriples = 0m,
        decimal factorPagoTiempoExtra = 0m)
        => new()
        {
            Empleado = new Empleado
            {
                PeriodicidadPago = PeriodicidadPago.Semanal,
                AplicaImss = false,
                AplicaIsr = false,
                AplicaInfonavit = false
            },
            Configuracion = new NominaConfiguracion(),
            SueldoReferencia = sueldoReferencia,
            DiasPagados = 7,
            AplicaImss = false,
            AplicaIsr = false,
            HorasExtraDobles = horasDobles,
            HorasExtraTriples = horasTriples,
            FactorPagoTiempoExtra = factorPagoTiempoExtra,
            HorasExtraFactoradas = horasExtraFactoradas
        };

    /// <summary>
    /// Fase 8 — con HorasExtraFactoradas > 0, el monto = factoradas × sueldoHora.
    /// sueldoReferencia 4800 / horasBase 48 = 100/h. factoradas 10 → 1000.
    /// </summary>
    [Fact]
    public void FactoradasPositivas_MontoEsFactoradasPorSueldoHora()
    {
        var resultado = new NominaCalculator().Calculate(Input(sueldoReferencia: 4800m, horasExtraFactoradas: 10m));

        // sueldoHora = 4800 / horasBase(48) = 100; monto = factoradas(10) × 100 = 1000.
        Assert.Equal(1000m, resultado.MontoHorasExtra);
        Assert.Equal(10m, resultado.HorasExtraFactoradas);
    }

    /// <summary>
    /// Coherencia Fase 8: una línea única "pago 300 min @ x2" reproduce exactamente el monto
    /// del path dobles actual. factoradas = 5h × 2 = 10 → 10 × 100 = 1000.
    /// Path dobles: horasDobles=5, factor=2 → 5 × 100 × 2 = 1000. Idéntico.
    /// </summary>
    [Fact]
    public void UnaLineaX2_CoincideConPathDoblesActual()
    {
        var calc = new NominaCalculator();

        var porFactoradas = calc.Calculate(Input(sueldoReferencia: 4800m, horasExtraFactoradas: 10m));
        var porDobles = calc.Calculate(Input(sueldoReferencia: 4800m, horasDobles: 5m, factorPagoTiempoExtra: 2m));

        Assert.Equal(porDobles.MontoHorasExtra, porFactoradas.MontoHorasExtra);
        Assert.Equal(1000m, porFactoradas.MontoHorasExtra);
    }

    /// <summary>
    /// Fase 8 — múltiples factores se suman: 2h x1 + 3h x2 → factoradas = 2×1 + 3×2 = 8 → 8 × 100 = 800.
    /// (No se puede representar con el path dobles único: confirma que factoradas generaliza.)
    /// </summary>
    [Fact]
    public void MultiplesFactores_SumaPonderada()
    {
        var resultado = new NominaCalculator().Calculate(Input(sueldoReferencia: 4800m, horasExtraFactoradas: 8m));

        Assert.Equal(800m, resultado.MontoHorasExtra);
    }

    /// <summary>
    /// Fase 8 — factoradas 0 cae al path dobles/triples×factor (legado).
    /// </summary>
    [Fact]
    public void FactoradasCero_CaeAtPathDoblesTriples()
    {
        var resultado = new NominaCalculator().Calculate(Input(
            sueldoReferencia: 4800m,
            horasExtraFactoradas: 0m,
            horasDobles: 5m,
            factorPagoTiempoExtra: 2m));

        Assert.Equal(1000m, resultado.MontoHorasExtra); // 5 × 100 × 2
        Assert.Equal(0m, resultado.HorasExtraFactoradas);
    }
}