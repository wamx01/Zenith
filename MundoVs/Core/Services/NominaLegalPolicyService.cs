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

    public VacacionCicloResultado ObtenerCicloVacacional(Empleado empleado, DateTime fechaReferencia, NominaConfiguracion configuracion)
    {
        if (!empleado.FechaContratacion.HasValue)
        {
            return new VacacionCicloResultado
            {
                InicioCiclo = fechaReferencia.Date,
                FinCiclo = fechaReferencia.Date,
                AnioVacacionalReconocido = 0,
                TieneDerechoVacaciones = false,
                DiasVacacionesEquivalentes = 0m
            };
        }

        var fechaContratacion = empleado.FechaContratacion.Value.Date;
        if (configuracion.TipoCicloVacacional == TipoCicloVacacionalRrhh.CorteFijoAnual)
        {
            return ObtenerCicloPorCorteFijo(fechaContratacion, fechaReferencia.Date, configuracion);
        }

        return ObtenerCicloPorAniversario(fechaContratacion, fechaReferencia.Date, configuracion);
    }

    public decimal CalcularDiasVacacionesDisponibles(Empleado empleado, DateTime fechaReferencia, decimal vacacionesUsadas, NominaConfiguracion configuracion)
    {
        var ciclo = ObtenerCicloVacacional(empleado, fechaReferencia, configuracion);
        if (!ciclo.TieneDerechoVacaciones || ciclo.AnioVacacionalReconocido <= 0)
        {
            return 0m;
        }

        return Math.Max(0m, ciclo.DiasVacacionesEquivalentes - vacacionesUsadas);
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

    private static VacacionCicloResultado ObtenerCicloPorAniversario(DateTime fechaContratacion, DateTime fechaReferencia, NominaConfiguracion configuracion)
    {
        var primerAniversario = fechaContratacion.AddYears(1);
        if (fechaReferencia < primerAniversario)
        {
            return new VacacionCicloResultado
            {
                InicioCiclo = fechaContratacion,
                FinCiclo = primerAniversario.AddDays(-1),
                AnioVacacionalReconocido = 0,
                TieneDerechoVacaciones = false,
                DiasVacacionesEquivalentes = 0m
            };
        }

        var aniosReconocidos = primerAniversario.Year - fechaContratacion.Year - (primerAniversario.Month < fechaContratacion.Month || (primerAniversario.Month == fechaContratacion.Month && primerAniversario.Day < fechaContratacion.Day) ? 1 : 0);
        var inicioCiclo = primerAniversario;
        while (inicioCiclo.AddYears(1) <= fechaReferencia)
        {
            inicioCiclo = inicioCiclo.AddYears(1);
            aniosReconocidos++;
        }

        return new VacacionCicloResultado
        {
            InicioCiclo = inicioCiclo,
            FinCiclo = inicioCiclo.AddYears(1).AddDays(-1),
            AnioVacacionalReconocido = Math.Max(1, aniosReconocidos),
            TieneDerechoVacaciones = true,
            DiasVacacionesEquivalentes = configuracion.ObtenerDiasVacacionesPorAntiguedad(Math.Max(1, aniosReconocidos))
        };
    }

    private static VacacionCicloResultado ObtenerCicloPorCorteFijo(DateTime fechaContratacion, DateTime fechaReferencia, NominaConfiguracion configuracion)
    {
        var inicioCicloActual = configuracion.ObtenerFechaInicioCicloVacacional(fechaReferencia.Year);
        if (fechaReferencia < inicioCicloActual)
            inicioCicloActual = inicioCicloActual.AddYears(-1);

        var primerCicloPosteriorAlta = configuracion.ObtenerFechaInicioCicloVacacional(fechaContratacion.Year);
        if (primerCicloPosteriorAlta <= fechaContratacion)
            primerCicloPosteriorAlta = primerCicloPosteriorAlta.AddYears(1);

        var primerCicloElegible = primerCicloPosteriorAlta;
        var mesesMinimos = Math.Max(0, configuracion.MesesMinimosPrimerAnioVacacional);
        while (CalcularMesesCumplidos(fechaContratacion, primerCicloElegible) < mesesMinimos)
            primerCicloElegible = primerCicloElegible.AddYears(1);

        if (inicioCicloActual < primerCicloElegible)
        {
            return new VacacionCicloResultado
            {
                InicioCiclo = inicioCicloActual,
                FinCiclo = inicioCicloActual.AddYears(1).AddDays(-1),
                AnioVacacionalReconocido = 0,
                TieneDerechoVacaciones = false,
                DiasVacacionesEquivalentes = 0m
            };
        }

        var anioReconocido = 1;
        var cursor = primerCicloElegible;
        while (cursor.AddYears(1) <= inicioCicloActual)
        {
            cursor = cursor.AddYears(1);
            anioReconocido++;
        }

        decimal diasVacaciones = configuracion.ObtenerDiasVacacionesPorAntiguedad(anioReconocido);
        if (anioReconocido == 1 && configuracion.PoliticaPrimerCicloVacacional == PoliticaPrimerCicloVacacionalRrhh.Proporcional)
        {
            var diasTrabajadosEnPrimerCiclo = Math.Max(0, (cursor.Date - fechaContratacion.Date).Days);
            var diasCiclo = Math.Max(1, (cursor.AddYears(1).Date - cursor.Date).Days);
            diasVacaciones = Math.Round(diasVacaciones * ((decimal)diasTrabajadosEnPrimerCiclo / diasCiclo), 2);
        }

        return new VacacionCicloResultado
        {
            InicioCiclo = inicioCicloActual,
            FinCiclo = inicioCicloActual.AddYears(1).AddDays(-1),
            AnioVacacionalReconocido = anioReconocido,
            TieneDerechoVacaciones = true,
            DiasVacacionesEquivalentes = diasVacaciones
        };
    }

    private static int CalcularMesesCumplidos(DateTime inicio, DateTime fin)
    {
        var meses = (fin.Year - inicio.Year) * 12 + fin.Month - inicio.Month;
        if (fin.Day < inicio.Day)
            meses--;
        return Math.Max(0, meses);
    }
}
