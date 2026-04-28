using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;

namespace MundoVs.Core.Services;

public class NominaCalculator : INominaCalculator
{
    public NominaCalculationResult Calculate(NominaCalculationInput input)
    {
        var diasBase = Math.Max(1, input.Configuracion.ObtenerDiasBase(input.Empleado.PeriodicidadPago));
        var sueldoDiario = input.SueldoReferencia / diasBase;
        var sueldoBase = input.SueldoBaseOverride ?? CalcularSueldoBasePeriodo(input.SueldoReferencia, input.Empleado.PeriodicidadPago, input.DiasPagados, input.Configuracion);
        var montoFestivoTrabajado = CalcularMontoFestivoTrabajado(input.DiasFestivoTrabajado, sueldoDiario, input.FactorFestivo);
        var montoDescansoTrabajado = CalcularMontoDescansoTrabajado(input.DiasDescansoTrabajado, sueldoDiario, input.Configuracion.FactorDescansoTrabajado);
        var montoPrimaDominical = CalcularMontoPrimaDominical(input.DiasDomingoTrabajado, sueldoDiario);
        var montoPrimaVacacional = input.Configuracion.FormaPagoPrimaVacacional == FormaPagoPrimaVacacionalRrhh.AlTomarVacaciones
            ? Math.Round(sueldoDiario * input.DiasVacaciones * input.Configuracion.PrimaVacacionalMinima, 2)
            : 0m;
        var (horasDobles, horasTriples) = ObtenerHorasExtraLegales(input);
        var montoHorasExtra = CalcularMontoHorasExtra(input.Empleado, horasDobles, horasTriples, input.SueldoReferencia, input.Configuracion);
        var montoDescuentoMinutos = CalcularMontoDescuentoMinutos(input.Empleado, input.MinutosDescuento, sueldoDiario, input.Configuracion);
        var diasVacacionesAnuales = input.DiasVacacionesAnualesOverride ?? input.Configuracion.ObtenerDiasVacacionesPorAntiguedad(input.AniosServicio);
        var cuotasImss = CalcularCuotasImss(input.AplicaImss, sueldoDiario, input.DiasPagados, diasVacacionesAnuales, input.Configuracion, input.AplicaSalarioMinimoFrontera);
        var montoInfonavit = CalcularMontoInfonavit(input.Empleado, sueldoBase);

        // Base gravable ISR = todas las percepciones ordinarias gravadas.
        // NOTA: se asume que percepciones manuales son gravables; si son exentas deben capturarse en otra cuenta.
        var totalPercepciones = sueldoBase + input.MontoDestajo + input.MontoBono + montoFestivoTrabajado
            + montoDescansoTrabajado + montoPrimaDominical + montoPrimaVacacional + input.ComplementoSalarioMinimo
            + montoHorasExtra + input.MontoPercepcionesManuales;

        var (retencionIsr, subsidioEmpleo) = (input.AplicaIsr && input.Configuracion.RetencionIsrHabilitada)
            ? CalcularIsrYSubsidio(totalPercepciones, input.Empleado.PeriodicidadPago, input.DiasPagados, input.Configuracion)
            : (0m, 0m);
        var (aguinaldoProv, primaVacProv) = CalcularProvisiones(sueldoDiario, input.DiasPagados, diasVacacionesAnuales, input.Configuracion);
        var totalObligacionesTerceros = retencionIsr + cuotasImss.obrera + montoInfonavit;
        var totalAportacionesPatronales = cuotasImss.patronal;
        var totalProvisiones = aguinaldoProv + primaVacProv;
        var costoEmpresa = totalPercepciones + subsidioEmpleo + totalAportacionesPatronales + totalProvisiones;

        return new NominaCalculationResult
        {
            SueldoBase = sueldoBase,
            SueldoDiario = sueldoDiario,
            MontoDestajo = input.MontoDestajo,
            MontoBono = input.MontoBono,
            MontoFestivoTrabajado = montoFestivoTrabajado,
            MontoDescansoTrabajado = montoDescansoTrabajado,
            MontoPrimaDominical = montoPrimaDominical,
            MontoPrimaVacacional = montoPrimaVacacional,
            ComplementoSalarioMinimo = input.ComplementoSalarioMinimo,
            HorasExtraBase = input.HorasExtraBase > 0 ? input.HorasExtraBase : input.HorasExtra,
            HorasExtraDobles = horasDobles,
            HorasExtraTriples = horasTriples,
            HorasExtraBanco = input.HorasExtraBanco,
            MontoHorasExtra = montoHorasExtra,
            MontoPercepcionesManuales = input.MontoPercepcionesManuales,
            MontoDeducciones = input.MontoDeducciones,
            MontoDescuentoMinutos = montoDescuentoMinutos,
            CuotaImssObrera = cuotasImss.obrera,
            CuotaImssPatronal = cuotasImss.patronal,
            MontoInfonavit = montoInfonavit,
            RetencionIsr = retencionIsr,
            SubsidioEmpleo = subsidioEmpleo,
            AguinaldoProvision = aguinaldoProv,
            PrimaVacacionalProvision = primaVacProv,
            TotalObligacionesTerceros = totalObligacionesTerceros,
            TotalAportacionesPatronales = totalAportacionesPatronales,
            TotalProvisiones = totalProvisiones,
            CostoEmpresa = costoEmpresa,
            TotalPagar = sueldoBase + input.MontoDestajo + input.MontoBono + montoFestivoTrabajado + montoDescansoTrabajado + montoPrimaDominical + montoPrimaVacacional
                + input.ComplementoSalarioMinimo + montoHorasExtra + input.MontoPercepcionesManuales + subsidioEmpleo
                - input.MontoDeducciones - montoDescuentoMinutos - cuotasImss.obrera - montoInfonavit - retencionIsr
        };
    }

    private static decimal CalcularSueldoBasePeriodo(decimal sueldoReferencia, PeriodicidadPago periodicidadPago, int diasPagados, NominaConfiguracion configuracion)
    {
        var diasBase = Math.Max(1, configuracion.ObtenerDiasBase(periodicidadPago));
        var sueldoDiario = sueldoReferencia / diasBase;
        return Math.Round(sueldoDiario * Math.Max(0, diasPagados), 2);
    }

    private static (decimal horasDobles, decimal horasTriples) ObtenerHorasExtraLegales(NominaCalculationInput input)
    {
        var horasBase = Math.Max(0m, input.HorasExtraBase > 0 ? input.HorasExtraBase : input.HorasExtra);
        var horasPagables = Math.Max(0m, input.HorasExtra);
        if (input.HorasExtraDobles > 0 || input.HorasExtraTriples > 0)
        {
            return (Math.Max(0m, input.HorasExtraDobles), Math.Max(0m, input.HorasExtraTriples));
        }

        var horasLegales = Math.Min(horasBase, horasPagables);
        var horasDobles = Math.Min(9m, horasLegales);
        var horasTriples = Math.Max(0m, horasLegales - horasDobles);
        return (horasDobles, horasTriples);
    }

    private static decimal CalcularMontoHorasExtra(Empleado empleado, decimal horasDobles, decimal horasTriples, decimal sueldoReferencia, NominaConfiguracion configuracion)
    {
        if (horasDobles <= 0 && horasTriples <= 0)
            return 0m;

        var horasBase = Math.Max(1, configuracion.ObtenerHorasBase(empleado.PeriodicidadPago));
        var sueldoHora = sueldoReferencia / horasBase;
        var montoDobles = horasDobles * sueldoHora * Math.Max(configuracion.FactorHoraExtra, 0m);
        var montoTriples = horasTriples * sueldoHora * Math.Max(configuracion.FactorHoraExtraTriple, 0m);
        return Math.Round(montoDobles + montoTriples, 2);
    }

    private static decimal CalcularMontoFestivoTrabajado(int diasFestivoTrabajado, decimal sueldoDiario, decimal factorFestivo)
        => diasFestivoTrabajado <= 0 || sueldoDiario <= 0
            ? 0m
            : Math.Round(diasFestivoTrabajado * sueldoDiario * Math.Max(0m, factorFestivo), 2);

    private static decimal CalcularMontoDescansoTrabajado(int diasDescansoTrabajado, decimal sueldoDiario, decimal factorDescansoTrabajado)
        => diasDescansoTrabajado <= 0 || sueldoDiario <= 0
            ? 0m
            : Math.Round(diasDescansoTrabajado * sueldoDiario * Math.Max(0m, factorDescansoTrabajado), 2);

    private static decimal CalcularMontoPrimaDominical(int diasDomingoTrabajado, decimal sueldoDiario)
        => diasDomingoTrabajado <= 0 || sueldoDiario <= 0
            ? 0m
            : Math.Round(diasDomingoTrabajado * sueldoDiario * 0.25m, 2);

    private static decimal CalcularMontoInfonavit(Empleado empleado, decimal sueldoBasePeriodo)
    {
        if (!empleado.AplicaInfonavit || empleado.FactorDescuentoInfonavit <= 0)
            return 0m;

        return empleado.FactorDescuentoInfonavit <= 1m
            ? Math.Round(Math.Max(0m, sueldoBasePeriodo) * empleado.FactorDescuentoInfonavit, 2)
            : Math.Round(empleado.FactorDescuentoInfonavit, 2);
    }

    private static decimal CalcularMontoDescuentoMinutos(Empleado empleado, int minutosDescuento, decimal sueldoDiario, NominaConfiguracion configuracion)
    {
        if (minutosDescuento <= 0 || sueldoDiario <= 0)
            return 0m;

        var diasBase = Math.Max(1m, configuracion.ObtenerDiasBase(empleado.PeriodicidadPago));
        var horasBase = Math.Max(1m, configuracion.ObtenerHorasBase(empleado.PeriodicidadPago));
        var minutosJornadaBase = Math.Max(1m, (horasBase / diasBase) * 60m);
        return Math.Round(sueldoDiario * (minutosDescuento / minutosJornadaBase), 2);
    }

    private static (decimal obrera, decimal patronal) CalcularCuotasImss(bool aplicaImss, decimal sueldoDiario, int diasPagados, decimal diasVacacionesAnuales, NominaConfiguracion configuracion, bool aplicaSalarioMinimoFrontera)
    {
        if (!aplicaImss || sueldoDiario <= 0 || diasPagados <= 0)
            return (0m, 0m);

        var factorAguinaldo = (configuracion.DiasAguinaldoMinimo * sueldoDiario) / 365m;
        var factorVacaciones = (diasVacacionesAnuales * configuracion.PrimaVacacionalMinima * sueldoDiario) / 365m;
        var sdi = sueldoDiario + factorAguinaldo + factorVacaciones;
        var tope = Math.Max(0m, configuracion.TopeSbcEnUma) * Math.Max(0m, configuracion.UmaDiaria);
        var minimo = Math.Max(0m, configuracion.ObtenerSalarioMinimo(aplicaSalarioMinimoFrontera));
        var sbc = sdi;

        if (tope > 0)
            sbc = Math.Min(sbc, tope);

        if (minimo > 0)
            sbc = Math.Max(sbc, minimo);

        var basePeriodo = sbc * diasPagados;
        var obrera = Math.Round(basePeriodo * Math.Max(0m, configuracion.TasaImssObrera), 2);
        var patronal = Math.Round(basePeriodo * (Math.Max(0m, configuracion.TasaImssPatronal) + Math.Max(0m, configuracion.PrimaRiesgoTrabajo)), 2);
        return (obrera, patronal);
    }

    // Retención ISR mensual art. 96 LISR proporcional al periodo y subsidio al empleo acreditable.
    private static (decimal isr, decimal subsidio) CalcularIsrYSubsidio(decimal totalPercepcionesPeriodo, PeriodicidadPago periodicidad, int diasPagados, NominaConfiguracion configuracion)
    {
        if (totalPercepcionesPeriodo <= 0 || diasPagados <= 0)
            return (0m, 0m);

        // Factor para escalar tarifa mensual al periodo efectivamente pagado.
        var diasPeriodoNominal = periodicidad switch
        {
            PeriodicidadPago.Semanal => 7m,
            PeriodicidadPago.Quincenal => 15m,
            _ => 30m
        };
        var factor = Math.Max(0.0001m, diasPeriodoNominal / 30m);

        var tabla = configuracion.ObtenerTablaIsr();
        var tramo = tabla.FirstOrDefault(t => totalPercepcionesPeriodo >= t.LimiteInferior * factor && totalPercepcionesPeriodo <= t.LimiteSuperior * factor);
        if (tramo == null)
            tramo = tabla.Last();

        var limiteInferiorPeriodo = tramo.LimiteInferior * factor;
        var cuotaFijaPeriodo = tramo.CuotaFija * factor;
        var excedente = Math.Max(0m, totalPercepcionesPeriodo - limiteInferiorPeriodo);
        var isrCausado = cuotaFijaPeriodo + (excedente * tramo.TasaExcedente);

        var tablaSub = configuracion.ObtenerTablaSubsidio();
        var subsTramo = tablaSub.FirstOrDefault(t => totalPercepcionesPeriodo >= t.LimiteInferior * factor && totalPercepcionesPeriodo <= t.LimiteSuperior * factor);
        var subsidio = subsTramo == null ? 0m : subsTramo.Subsidio * factor;

        // Aplicar subsidio: si ISR causado <= subsidio, no se retiene ISR y la diferencia se paga como subsidio al empleo.
        if (isrCausado >= subsidio)
        {
            return (Math.Round(isrCausado - subsidio, 2), 0m);
        }
        return (0m, Math.Round(subsidio - isrCausado, 2));
    }

    private static (decimal aguinaldo, decimal primaVacacional) CalcularProvisiones(decimal sueldoDiario, int diasPagados, decimal diasVacacionesAnuales, NominaConfiguracion configuracion)
    {
        if (sueldoDiario <= 0 || diasPagados <= 0)
            return (0m, 0m);

        // Aguinaldo proporcional: (días_aguinaldo * sueldoDiario / 365) * diasPagados
        var aguinaldo = Math.Round((configuracion.DiasAguinaldoMinimo * sueldoDiario / 365m) * diasPagados, 2);
        // Prima vacacional proporcional: (díasVacaciones * 25% * sueldoDiario / 365) * diasPagados
        var primaVac = Math.Round((diasVacacionesAnuales * configuracion.PrimaVacacionalMinima * sueldoDiario / 365m) * diasPagados, 2);
        return (aguinaldo, primaVac);
    }
}
