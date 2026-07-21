using MundoVs.Core.Entities;

namespace MundoVs.Core.Interfaces;

/// <summary>
/// Calcula el sueldo base de un periodo combinando la parte Fija (días pagados Fija × sueldo diario)
/// y la parte PorHoras (minutos trabajados × sueldo hora, con factor festivo en festivos).
/// Extraído de Nominas.razor para poder testear la composición mixta sin la página de 2500 líneas.
/// El divisor Fija/PorHoras de días y el festivo efectivo (que respeta correcciones manuales del
/// total de DiasFestivoTrabajado en la prenómina) los deriva la página; este servicio sólo calcula
/// el monto a partir de los agregados.
/// </summary>
public interface INominaSueldoBasePolicy
{
    SueldoBaseResultado Calcular(SueldoBaseInput input);
}

/// <summary>
/// Entrada del cálculo de sueldo base. DiasPagados es el total del periodo (mantiene IMSS/ISR/provisiones);
/// la parte Fija se deriva como DiasPagados - DiasPorHorasTrabajados dentro del servicio.
/// </summary>
public sealed class SueldoBaseInput
{
    public required PeriodicidadPago PeriodicidadPago { get; init; }
    /// <summary>Si el esquema de pago incluye sueldo base fijo (gate de la parte Fija).</summary>
    public bool IncluyeSueldoBase { get; init; }
    /// <summary>Sueldo de referencia (sueldoBaseOverride ?? sugerido ?? sueldoSemanal).</summary>
    public decimal SueldoReferencia { get; init; }
    /// <summary>Días pagados totales del periodo (no se reduce; alimenta IMSS/ISR/provisiones).</summary>
    public int DiasPagados { get; init; }
    /// <summary>Días trabajados bajo esquema PorHoras (se excluyen de la base Fija).</summary>
    public int DiasPorHorasTrabajados { get; init; }
    /// <summary>Minutos netos trabajados PorHoras en días NO festivos.</summary>
    public int MinutosPorHorasNetos { get; init; }
    /// <summary>Minutos netos trabajados PorHoras en días festivos.</summary>
    public int MinutosPorHorasFestivoNetos { get; init; }
    /// <summary>Factor festivo aplicado (promedio del periodo).</summary>
    public decimal FactorFestivo { get; init; }
    public required NominaConfiguracion Configuracion { get; init; }
}

/// <summary>Resultado: sueldo base total (Fija + PorHoras).</summary>
public sealed class SueldoBaseResultado
{
    public decimal SueldoBase { get; init; }
}