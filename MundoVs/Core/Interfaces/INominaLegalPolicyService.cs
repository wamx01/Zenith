using MundoVs.Core.Entities;

namespace MundoVs.Core.Interfaces;

public interface INominaLegalPolicyService
{
    bool IncluyeSueldoBase(EmpleadoEsquemaPago? asignacion);
    decimal ObtenerSueldoReferencia(Empleado empleado, EmpleadoEsquemaPago? asignacion);
    decimal CalcularSueldoBasePeriodo(decimal sueldoReferencia, PeriodicidadPago periodicidadPago, int diasPagados, NominaConfiguracion configuracion);
    decimal CalcularSueldoBasePeriodo(Empleado empleado, EmpleadoEsquemaPago? asignacion, int diasPagados, NominaConfiguracion configuracion);
    VacacionCicloResultado ObtenerCicloVacacional(Empleado empleado, DateTime fechaReferencia, NominaConfiguracion configuracion);
    decimal CalcularDiasVacacionesDisponibles(Empleado empleado, DateTime fechaReferencia, decimal vacacionesUsadas, NominaConfiguracion configuracion);
    decimal CalcularComplementoSalarioMinimo(decimal sueldoBasePeriodo, decimal montoDestajo, int diasPagados, NominaConfiguracion configuracion);
    decimal CalcularComplementoSalarioMinimo(Empleado empleado, EmpleadoEsquemaPago? asignacion, int diasPagados, decimal montoDestajo, NominaConfiguracion configuracion);
}

public sealed class VacacionCicloResultado
{
    public DateTime InicioCiclo { get; init; }
    public DateTime FinCiclo { get; init; }
    public int AnioVacacionalReconocido { get; init; }
    public bool TieneDerechoVacaciones { get; init; }
    public decimal DiasVacacionesEquivalentes { get; init; }
}
