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