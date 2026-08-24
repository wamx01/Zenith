namespace MundoVs.Core.Entities;

/// <summary>
/// Esquema de jornada del empleado con vigencia. Define la forma de la jornada
/// (fija con horario programado, o por horas sin horario fijo) para un rango de
/// fechas. Ortogonal a <see cref="TipoNomina"/> y a los esquemas de pago
/// (<see cref="EmpleadoEsquemaPago"/>): éstos describen cómo/qué se paga; el
/// esquema de jornada describe contra qué referencia se evalúa el tiempo.
/// </summary>
public class EmpleadoEsquemaJornada : BaseEntity
{
    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    /// <summary>
    /// Forma de la jornada. Fija = hay horario programado contra el que se calculan
    /// retardo, salida anticipada, faltante y extra automático. PorHoras = horario
    /// variable, no hay referencia: se paga el tiempo trabajado, sin faltante/retardo,
    /// extra sólo manual, y en día festivo el tiempo trabajado va al factor festivo.
    /// </summary>
    public TipoJornada TipoJornada { get; set; } = TipoJornada.Fija;

    /// <summary>
    /// Pago fijo por labor/tarea (sólo aplica cuando TipoJornada == PorHoras). Cuando es
    /// true, el día se paga como Fija (sueldoDiario × día) en vez de por minutos trabajados
    /// — el empleado cobra lo mismo por la labor sin importar cuánto tardó (ej. limpieza:
    /// va, limpia y se va; dure 1h o 5h cobra el sueldo del día). Mantiene lo "sin horario"
    /// de PorHoras: sin turno, sin meta semanal, sin retardo/faltante/salida anticipada.
    /// English: Fixed per-task pay (only when TipoJornada == PorHoras). When true, the day
    /// is paid as Fija (daily salary × day) instead of by worked minutes — the employee
    /// earns the same for the task regardless of duration (e.g. cleaning). Keeps the
    /// "no schedule" behavior of PorHoras: no shift, no weekly meta, no late/shortage/early-leave.
    /// </summary>
    public bool PagoFijoPorLabor { get; set; }

    /// <summary>Inicio de vigencia del esquema (inclusive).</summary>
    public DateTime VigenteDesde { get; set; }

    /// <summary>Fin de vigencia (inclusive). Null = vigente indefinidamente.</summary>
    public DateTime? VigenteHasta { get; set; }
}

public enum TipoJornada
{
    Fija = 1,
    PorHoras = 2
}