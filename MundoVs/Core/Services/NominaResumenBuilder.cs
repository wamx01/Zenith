using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;

namespace MundoVs.Core.Services;

public class NominaResumenBuilder(INominaCalculator nominaCalculator, INominaLegalPolicyService nominaLegalPolicy) : INominaResumenBuilder
{
    public NominaResumenResult Build(NominaResumenInput input)
    {
        var detalles = new Dictionary<Guid, NominaDetalleResumen>();
        decimal totalSueldoBase = 0m;
        decimal totalDestajo = 0m;
        decimal totalBono = 0m;
        decimal totalFestivoTrabajado = 0m;
        decimal totalDescansoTrabajado = 0m;
        decimal totalPrimaDominical = 0m;
        decimal totalPrimaVacacional = 0m;
        decimal totalComplementoSalarioMinimo = 0m;
        decimal totalHorasExtraDobles = 0m;
        decimal totalHorasExtraTriples = 0m;
        decimal totalHorasExtraBanco = 0m;
        decimal totalHorasExtra = 0m;
        decimal totalPercepcionesManuales = 0m;
        decimal totalImssObrero = 0m;
        decimal totalIsr = 0m;
        decimal totalImssPatronal = 0m;
        decimal totalObligacionesTerceros = 0m;
        decimal totalAportacionesPatronales = 0m;
        decimal totalProvisiones = 0m;
        decimal totalCostoEmpresa = 0m;
        decimal totalInfonavit = 0m;
        decimal totalDeducciones = 0m;
        decimal totalDescuentoMinutos = 0m;
        decimal totalNeto = 0m;

        foreach (var detalle in input.Detalles)
        {
            var resultado = BuildDetalle(detalle, input.Configuracion, input.FechaReferencia);
            detalles[detalle.Id] = new NominaDetalleResumen
            {
                NominaDetalleId = detalle.Id,
                HorasExtraDoblesPreview = resultado.HorasExtraDobles,
                HorasExtraTriplesPreview = resultado.HorasExtraTriples,
                MontoHorasExtraPreview = resultado.MontoHorasExtra,
                TotalPagarPreview = resultado.TotalPagar
            };

            totalSueldoBase += detalle.SueldoBase;
            totalDestajo += detalle.MontoDestajo;
            totalBono += detalle.MontoBono;
            totalFestivoTrabajado += detalle.MontoFestivoTrabajado;
            totalDescansoTrabajado += detalle.MontoDescansoTrabajado;
            totalPrimaDominical += detalle.MontoPrimaDominical;
            totalPrimaVacacional += detalle.MontoPrimaVacacional;
            totalComplementoSalarioMinimo += detalle.ComplementoSalarioMinimo;
            totalHorasExtraDobles += resultado.HorasExtraDobles;
            totalHorasExtraTriples += resultado.HorasExtraTriples;
            totalHorasExtraBanco += detalle.HorasExtraBanco;
            totalHorasExtra += resultado.MontoHorasExtra;
            totalPercepcionesManuales += detalle.Bonos;
            totalImssObrero += detalle.CuotaImssObrera;
            totalIsr += detalle.RetencionIsr;
            totalImssPatronal += detalle.CuotaImssPatronal;
            totalObligacionesTerceros += detalle.TotalObligacionesTerceros;
            totalAportacionesPatronales += detalle.TotalAportacionesPatronales;
            totalProvisiones += detalle.TotalProvisiones;
            totalCostoEmpresa += detalle.CostoEmpresa;
            totalInfonavit += detalle.MontoInfonavit;
            totalDeducciones += detalle.Deducciones;
            totalDescuentoMinutos += detalle.MontoDescuentoMinutos;
            totalNeto += resultado.TotalPagar;
        }

        return new NominaResumenResult
        {
            Detalles = detalles,
            TotalSueldoBase = totalSueldoBase,
            TotalDestajo = totalDestajo,
            TotalBono = totalBono,
            TotalFestivoTrabajado = totalFestivoTrabajado,
            TotalDescansoTrabajado = totalDescansoTrabajado,
            TotalPrimaDominical = totalPrimaDominical,
            TotalPrimaVacacional = totalPrimaVacacional,
            TotalComplementoSalarioMinimo = totalComplementoSalarioMinimo,
            TotalHorasExtraDobles = totalHorasExtraDobles,
            TotalHorasExtraTriples = totalHorasExtraTriples,
            TotalHorasExtraBanco = totalHorasExtraBanco,
            TotalHorasExtra = totalHorasExtra,
            TotalPercepcionesManuales = totalPercepcionesManuales,
            TotalImssObrero = totalImssObrero,
            TotalIsr = totalIsr,
            TotalImssPatronal = totalImssPatronal,
            TotalObligacionesTerceros = totalObligacionesTerceros,
            TotalAportacionesPatronales = totalAportacionesPatronales,
            TotalProvisiones = totalProvisiones,
            TotalCostoEmpresa = totalCostoEmpresa,
            TotalInfonavit = totalInfonavit,
            TotalDeducciones = totalDeducciones,
            TotalDescuentoMinutos = totalDescuentoMinutos,
            TotalNeto = totalNeto
        };
    }

    private NominaCalculationResult BuildDetalle(NominaDetalle detalle, NominaConfiguracion configuracion, DateTime fechaReferencia)
    {
        if (detalle.Empleado == null)
        {
            return new NominaCalculationResult
            {
                SueldoBase = detalle.SueldoBase,
                MontoBono = detalle.MontoBono,
                MontoFestivoTrabajado = detalle.MontoFestivoTrabajado,
                MontoDescansoTrabajado = detalle.MontoDescansoTrabajado,
                MontoPrimaDominical = detalle.MontoPrimaDominical,
                MontoPrimaVacacional = detalle.MontoPrimaVacacional,
                ComplementoSalarioMinimo = detalle.ComplementoSalarioMinimo,
                HorasExtraBase = detalle.HorasExtraBase,
                HorasExtraDobles = detalle.HorasExtraDobles,
                HorasExtraTriples = detalle.HorasExtraTriples,
                HorasExtraBanco = detalle.HorasExtraBanco,
                MontoHorasExtra = detalle.MontoHorasExtra,
                MontoPercepcionesManuales = detalle.Bonos,
                MontoDeducciones = detalle.Deducciones,
                MontoDescuentoMinutos = detalle.MontoDescuentoMinutos,
                CuotaImssObrera = detalle.CuotaImssObrera,
                CuotaImssPatronal = detalle.CuotaImssPatronal,
                MontoInfonavit = detalle.MontoInfonavit,
                TotalPagar = detalle.TotalPagar
            };
        }

        var sueldoReferencia = ObtenerSueldoReferenciaDesdeDetalle(detalle.Empleado, detalle, configuracion);
        var diasBase = Math.Max(1, configuracion.ObtenerDiasBase(detalle.Empleado.PeriodicidadPago));
        var sueldoDiario = sueldoReferencia <= 0 ? 0m : sueldoReferencia / diasBase;
        var factorFestivo = detalle.DiasFestivoTrabajado > 0 && sueldoDiario > 0
            ? Math.Max(0m, Math.Round(detalle.MontoFestivoTrabajado / (detalle.DiasFestivoTrabajado * sueldoDiario), 4))
            : 2m;
        var cicloVacacional = nominaLegalPolicy.ObtenerCicloVacacional(detalle.Empleado, fechaReferencia, configuracion);

        return nominaCalculator.Calculate(new NominaCalculationInput
        {
            Empleado = detalle.Empleado,
            Configuracion = configuracion,
            SueldoReferencia = sueldoReferencia,
            SueldoBaseOverride = detalle.SueldoBase,
            DiasPagados = detalle.DiasPagados,
            DiasVacaciones = detalle.DiasVacaciones,
            DiasDescansoTrabajado = detalle.DiasDescansoTrabajado,
            DiasDomingoTrabajado = detalle.DiasDomingoTrabajado,
            DiasFestivoTrabajado = detalle.DiasFestivoTrabajado,
            MinutosDescuento = detalle.MinutosRetardo + detalle.MinutosSalidaAnticipada + detalle.MinutosDescuentoManual,
            HorasExtraBase = detalle.HorasExtraBase,
            HorasExtraDobles = detalle.HorasExtraDobles,
            HorasExtraTriples = detalle.HorasExtraTriples,
            HorasExtraBanco = detalle.HorasExtraBanco,
            AplicaImss = detalle.AplicaImss,
            AplicaIsr = detalle.Empleado.AplicaIsr,
            HorasExtra = detalle.HorasExtra,
            MontoDestajo = detalle.MontoDestajo,
            MontoBono = detalle.MontoBono,
            MontoPercepcionesManuales = detalle.Bonos,
            MontoDeducciones = detalle.Deducciones,
            ComplementoSalarioMinimo = detalle.ComplementoSalarioMinimo,
            FactorFestivo = factorFestivo,
            AniosServicio = CalcularAniosServicio(detalle.Empleado, fechaReferencia),
            DiasVacacionesAnualesOverride = cicloVacacional.DiasVacacionesEquivalentes
        });
    }

    private static decimal ObtenerSueldoReferenciaDesdeDetalle(Empleado empleado, NominaDetalle detalle, NominaConfiguracion configuracion)
    {
        if (detalle.DiasPagados > 0 && detalle.SueldoBase > 0)
        {
            var diasBase = Math.Max(1, configuracion.ObtenerDiasBase(empleado.PeriodicidadPago));
            return Math.Round((detalle.SueldoBase / detalle.DiasPagados) * diasBase, 2);
        }

        return empleado.SueldoSemanal;
    }

    private static int CalcularAniosServicio(Empleado empleado, DateTime fechaReferencia)
    {
        if (!empleado.FechaContratacion.HasValue)
            return 0;

        return Math.Max(0, (int)Math.Floor((fechaReferencia.Date - empleado.FechaContratacion.Value.Date).TotalDays / 365.25));
    }
}
