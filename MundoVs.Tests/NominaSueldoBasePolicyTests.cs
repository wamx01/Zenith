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

    [Fact]
    public void Calcular_PagoFijoPorLabor_DuracionIrrelevante_PagaSueldoDiarioPorDia()
    {
        // Perfil #4 (limpieza): el snapshot redirige los días PorHoras con flag PagoFijoPorLabor
        // al bucket Fija → el input llega con DiasPorHorasTrabajados=0 y MinutosPorHorasNetos=0,
        // y el día queda en DiasPagados. El sueldo base depende sólo de los días pagados, NO de
        // los minutos trabajados: dure 1h o 5h, cobra lo mismo (sueldoDiario × día).
        // English: profile #4 (cleaning): the snapshot routes PorHoras days with the
        // PagoFijoPorLabor flag to the Fija bucket → the input arrives with
        // DiasPorHorasTrabajados=0 and MinutosPorHorasNetos=0, the day stays in DiasPagados.
        // Sueldo base depends only on paid days, NOT on worked minutes — 1h or 5h pays the same.
        // Con flag: 2 días redirigidos al bucket Fija → sueldoDiario(1000/7) × 2 = 285.71.
        var conFlag = _policy.Calcular(Input(diasPagados: 2, diasPorHorasTrabajados: 0, minutosPorHorasNetos: 0));
        Assert.Equal(285.71m, conFlag.SueldoBase);

        // Sin flag: los mismos 2 días PorHoras con 360 min trabajados (1h + 5h) se pagan por
        // minuto → 360 min = 6h × (1000/48) = 125.00. Distinto al fijo: el flag cambia el bucket
        // y la duración SÍ importa (cobra por el tiempo real).
        // English: without the flag, the same 2 PorHoras days with 360 worked min (1h + 5h) are
        // paid by minute → 125.00. Different from fixed: the flag changes the bucket and duration
        // DOES matter (paid by actual time).
        var sinFlag = _policy.Calcular(Input(diasPagados: 2, diasPorHorasTrabajados: 2, minutosPorHorasNetos: 360));
        Assert.Equal(125.00m, sinFlag.SueldoBase);
        Assert.NotEqual(conFlag.SueldoBase, sinFlag.SueldoBase);
    }
}