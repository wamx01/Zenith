using MundoVs.Core.Entities;
using MundoVs.Core.Interfaces;

namespace MundoVs.Core.Services;

public sealed class NominaLegalPolicyService : INominaLegalPolicyService
{
    public bool IncluyeSueldoBase(EmpleadoEsquemaPago? asignacion)
    {
        var esquema = asignacion?.EsquemaPago;
        return esquema == null || esquema.IncluyeSueldoBase || esquema.Tipo == TipoEsquemaPago.SueldoFijo;
    }

    public decimal ObtenerSueldoReferencia(Empleado empleado, EmpleadoEsquemaPago? asignacion)
        => asignacion?.SueldoBaseOverride ?? asignacion?.EsquemaPago?.SueldoBaseSugerido ?? empleado.SueldoSemanal;

    public decimal CalcularSueldoBasePeriodo(decimal sueldoReferencia, PeriodicidadPago periodicidadPago, int diasPagados, NominaConfiguracion configuracion)
    {
        var diasBase = Math.Max(1, configuracion.ObtenerDiasBase(periodicidadPago));
        var sueldoDiario = sueldoReferencia / diasBase;
        return Math.Round(sueldoDiario * Math.Max(0, diasPagados), 2);
    }

    public decimal CalcularSueldoBasePeriodo(Empleado empleado, EmpleadoEsquemaPago? asignacion, int diasPagados, NominaConfiguracion configuracion)
    {
        if (!IncluyeSueldoBase(asignacion))
        {
            return 0m;
        }

        var sueldoReferencia = ObtenerSueldoReferencia(empleado, asignacion);
        return CalcularSueldoBasePeriodo(sueldoReferencia, empleado.PeriodicidadPago, diasPagados, configuracion);
    }

    public decimal CalcularDiasVacacionesDisponibles(Empleado empleado, DateTime fechaReferencia, decimal vacacionesUsadas, NominaConfiguracion configuracion)
    {
        if (!empleado.FechaContratacion.HasValue)
        {
            return 0m;
        }

        var anios = (int)Math.Floor((fechaReferencia.Date - empleado.FechaContratacion.Value.Date).TotalDays / 365.25);
        var dias = configuracion.ObtenerDiasVacacionesPorAntiguedad(anios);
        return Math.Max(0m, dias - vacacionesUsadas);
    }

    public decimal CalcularComplementoSalarioMinimo(decimal sueldoBasePeriodo, decimal montoDestajo, int diasPagados, NominaConfiguracion configuracion)
    {
        if (diasPagados <= 0)
        {
            return 0m;
        }

        var ingresoBasePeriodo = Math.Max(0m, sueldoBasePeriodo) + Math.Max(0m, montoDestajo);
        var minimoPeriodo = configuracion.ObtenerSalarioMinimo() * diasPagados;
        return Math.Max(0m, Math.Round(minimoPeriodo - ingresoBasePeriodo, 2));
    }

    public decimal CalcularComplementoSalarioMinimo(Empleado empleado, EmpleadoEsquemaPago? asignacion, int diasPagados, decimal montoDestajo, NominaConfiguracion configuracion)
    {
        var sueldoBasePeriodo = CalcularSueldoBasePeriodo(empleado, asignacion, diasPagados, configuracion);
        return CalcularComplementoSalarioMinimo(sueldoBasePeriodo, montoDestajo, diasPagados, configuracion);
    }
}
