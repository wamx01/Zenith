using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;
using MundoVs.Core.Services;

namespace MundoVs.Tests;

/// <summary>
/// Tests de NominaSueldoBasePolicy: composición Fija (días × sueldo diario) + PorHoras (minutos × sueldo hora,
/// festivo × factor). Casos puros Fija, puros PorHoras, mixtos, festivo y gate IncluyeSueldoBase.
/// Config default: DiasBaseSemanal=7, HorasBaseSemanal=48, FactorFestivoTrabajado=2.
/// sueldoReferencia=1000 → sueldoDiario=1000/7=142.857..., sueldoHora=1000/48=20.8333...
/// </summary>
public sealed class NominaSueldoBasePolicyTests
{
    private readonly NominaSueldoBasePolicy _policy = new(new NominaLegalPolicyService());
    private static NominaConfiguracion Config() => new();
    private static SueldoBaseInput Input(
        int diasPagados,
        int diasPorHorasTrabajados = 0,
        int minutosPorHorasNetos = 0,
        int minutosPorHorasFestivoNetos = 0,
        bool incluyeSueldoBase = true,
        decimal sueldoReferencia = 1000m,
        decimal factorFestivo = 2m) => new()
    {
        PeriodicidadPago = PeriodicidadPago.Semanal,
        IncluyeSueldoBase = incluyeSueldoBase,
        SueldoReferencia = sueldoReferencia,
        DiasPagados = diasPagados,
        DiasPorHorasTrabajados = diasPorHorasTrabajados,
        MinutosPorHorasNetos = minutosPorHorasNetos,
        MinutosPorHorasFestivoNetos = minutosPorHorasFestivoNetos,
        FactorFestivo = factorFestivo,
        Configuracion = Config()
    };

    [Fact]
    public void Calcular_PuroFija_ReproduceCalcActual_SinPorHoras()
    {
        // 7 días pagados, 0 PorHoras → sueldoBase = sueldoSemanal (igual al calc anterior).
        var r = _policy.Calcular(Input(diasPagados: 7));
        Assert.Equal(1000m, r.SueldoBase);
    }

    [Fact]
    public void Calcular_PuroPorHoras_SinFestivo_PagaSoloMinutosNetos()
    {
        // 7 días PorHoras, 300 min (5h) netos, sin festivo. base Fija=0, base PorHoras=5h×sueldoHora.
        // 5h × (1000/48) = 104.1666... → 104.17
        var r = _policy.Calcular(Input(diasPagados: 7, diasPorHorasTrabajados: 7, minutosPorHorasNetos: 300));
        Assert.Equal(104.17m, r.SueldoBase);
    }

    [Fact]
    public void Calcular_PuroPorHoras_ConFestivo_AplicaFactorFestivoALosMinutos()
    {
        // 240 min (4h) netos + 180 min (3h) festivo, factor 2.
        // 4h×20.8333 + 3h×20.8333×2 = 83.3333 + 125 = 208.3333 → 208.33
        var r = _policy.Calcular(Input(diasPagados: 7, diasPorHorasTrabajados: 7,
            minutosPorHorasNetos: 240, minutosPorHorasFestivoNetos: 180));
        Assert.Equal(208.33m, r.SueldoBase);
    }

    [Fact]
    public void Calcular_MixtoFijaPorHoras_CombinaBaseDiariaYMinutos()
    {
        // 7 días: 3 PorHoras (4 Fija). 180 min (3h) PorHoras netos + 120 min (2h) PorHoras festivo, factor 2.
        // Fija: 1000/7 × 4 = 571.4285 → 571.43
        // PorHoras: 3h×20.8333 + 2h×20.8333×2 = 62.5 + 83.3333 = 145.8333 → 145.83
        // Total: 571.43 + 145.83 = 717.26
        var r = _policy.Calcular(Input(diasPagados: 7, diasPorHorasTrabajados: 3,
            minutosPorHorasNetos: 180, minutosPorHorasFestivoNetos: 120));
        Assert.Equal(717.26m, r.SueldoBase);
    }

    [Fact]
    public void Calcular_IncluyeSueldoBaseFalso_NoAplicaBaseFija_PeroSiMinutosPorHoras()
    {
        // Destajo-type: IncluyeSueldoBase=false. La parte Fija=0, pero los minutos PorHoras sí se pagan
        // (son tiempo trabajado, no base fija). 300 min (5h) → 104.17.
        var r = _policy.Calcular(Input(diasPagados: 7, diasPorHorasTrabajados: 7,
            minutosPorHorasNetos: 300, incluyeSueldoBase: false));
        Assert.Equal(104.17m, r.SueldoBase);
    }

    [Fact]
    public void Calcular_IncluyeSueldoBaseFalso_PuroFija_DaCero()
    {
        // Sin PorHoras y sin base fija → 0 (equivale al detalle.SueldoBase=0 actual).
        var r = _policy.Calcular(Input(diasPagados: 7, incluyeSueldoBase: false));
        Assert.Equal(0m, r.SueldoBase);
    }
}