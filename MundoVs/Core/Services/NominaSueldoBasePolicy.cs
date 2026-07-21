using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;

namespace MundoVs.Core.Services;

/// <summary>
/// Política de sueldo base que compone la parte Fija (días pagados Fija × sueldo diario) con la
/// parte PorHoras (minutos trabajados × sueldo hora). Decisión de política 2026-07-20: PorHoras
/// se paga por minutos trabajados (minutos/60 × sueldoHora); los minutos en festivo × factorFestivo.
/// El esquema PorHoras no genera faltante/retardo (F2) ni extra manual en festivo (F4a); su tiempo
/// trabajado va directo al sueldo base. La parte Fija mantiene el cálculo por día de
/// <see cref="INominaLegalPolicyService.CalcularSueldoBasePeriodo(decimal,PeriodicidadPago,int,NominaConfiguracion)"/>.
/// </summary>
public sealed class NominaSueldoBasePolicy : INominaSueldoBasePolicy
{
    private readonly INominaLegalPolicyService _legalPolicy;

    public NominaSueldoBasePolicy(INominaLegalPolicyService legalPolicy)
    {
        _legalPolicy = legalPolicy;
    }

    public SueldoBaseResultado Calcular(SueldoBaseInput input)
    {
        var diasPagadosFija = Math.Max(0, input.DiasPagados - input.DiasPorHorasTrabajados);

        // Parte Fija: sueldo diario × días pagados Fija. Gateada por IncluyeSueldoBase
        // (un esquema de destajo/piezas no genera base fija). Idéntico al calc anterior cuando
        // no hay días PorHoras (diasPagadosFija == diasPagados).
        var sueldoBaseFija = input.IncluyeSueldoBase
            ? _legalPolicy.CalcularSueldoBasePeriodo(input.SueldoReferencia, input.PeriodicidadPago, diasPagadosFija, input.Configuracion)
            : 0m;

        // Parte PorHoras: minutos trabajados × sueldo hora. Siempre aplica (es tiempo trabajado,
        // independiente del gate IncluyeSueldoBase que es para la base fija). Festivo × factor.
        var sueldoBasePorHoras = CalcularSueldoBasePorHoras(
            input.MinutosPorHorasNetos,
            input.MinutosPorHorasFestivoNetos,
            input.SueldoReferencia,
            input.PeriodicidadPago,
            input.FactorFestivo,
            input.Configuracion);

        return new SueldoBaseResultado
        {
            SueldoBase = Math.Round(sueldoBaseFija + sueldoBasePorHoras, 2)
        };
    }

    private static decimal CalcularSueldoBasePorHoras(
        int minutosPorHorasNetos,
        int minutosPorHorasFestivoNetos,
        decimal sueldoReferencia,
        PeriodicidadPago periodicidad,
        decimal factorFestivo,
        NominaConfiguracion configuracion)
    {
        if (minutosPorHorasNetos <= 0 && minutosPorHorasFestivoNetos <= 0)
            return 0m;

        var horasBase = Math.Max(1m, configuracion.ObtenerHorasBase(periodicidad));
        var sueldoHora = sueldoReferencia / horasBase;
        var horasNetas = minutosPorHorasNetos / 60m;
        var horasFestivo = minutosPorHorasFestivoNetos / 60m;
        var montoNeto = horasNetas * sueldoHora;
        var montoFestivo = horasFestivo * sueldoHora * Math.Max(0m, factorFestivo);
        return Math.Round(montoNeto + montoFestivo, 2);
    }
}