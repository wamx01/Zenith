using MundoVs.Core.Entities;

namespace MundoVs.Core.Services;

/// <summary>
/// Resultado del sourcing de tiempo extra y deducciones para un <see cref="NominaDetalle"/>.
/// Origen "periodo" = tomado de la resolución por periodo autorizada; "incidencia" = snapshot
/// de prenómina (sumas diarias), equivalente al comportamiento previo al cutover.
/// </summary>
public sealed class NominaOvertimeSourcing
{
    public decimal HorasExtra { get; init; }
    public decimal HorasExtraBase { get; init; }
    public decimal HorasExtraBanco { get; init; }
    public decimal HorasExtraDobles { get; init; }
    public decimal HorasExtraTriples { get; init; }
    public decimal FactorPagoTiempoExtra { get; init; }
    /// <summary>
    /// Fase 8 — "horas ponderadas" a pago = Σ pago.Minutos/60 × Factor (de las líneas de la
    /// resolución). El calculador multiplica esto por sueldoHora para el monto exacto cuando hay
    /// varios factores. 0 → el calculador cae al path dobles/triples×factor (legado).
    /// </summary>
    public decimal HorasExtraFactoradas { get; init; }

    public int MinutosFaltanteDescontable { get; init; }
    public int MinutosRetardo { get; init; }
    public int MinutosSalidaAnticipada { get; init; }
    public int MinutosDescuentoManual { get; init; }
    public int MinutosPerdonadosManual { get; init; }

    /// <summary>"periodo" (resolución autorizada) o "incidencia" (fallback de prenómina).</summary>
    public string Origen { get; init; } = "incidencia";
}

/// <summary>
/// Valores diarios derivados que alimentan el sourcing. Agnósticos al origen (incidencia de
/// prenómina <see cref="PrenominaDetalle"/> o resumen interno del snapshot) — ambos consumidores
/// construyen este struct y llaman a <see cref="NominaTiempoExtraSourcing.Source"/> para que la
/// lógica del "consumo del periodo" tenga un solo dueño testeado.
/// </summary>
public readonly record struct NominaOvertimeSourcingInput
{
    public decimal HorasExtra { get; init; }
    public decimal HorasExtraBase { get; init; }
    public decimal HorasBancoAcumuladas { get; init; }
    public int MinutosRetardo { get; init; }
    public int MinutosSalidaAnticipada { get; init; }
    public int MinutosPerdonadosManual { get; init; }
    public int MinutosFaltanteDescontable { get; init; }
    public int MinutosDescuentoManual { get; init; }
}

/// <summary>
/// Decide de dónde tomar el tiempo extra (pago/banco/dobles/triples/factor) y las deducciones de
/// sueldo asociadas, para un (empleado, periodo).
///
/// Fase 5.5 — cutover nómina → resolución por periodo: cuando existe una
/// <see cref="RrhhResolucionTiempoExtraPeriodo"/> <see cref="RrhhResolucionPeriodoEstatus.Autorizada"/>
/// para el periodo, se consume como fuente autoritativa (el snapshot de prenómina suma minutos
/// diarios ahora dormidos). Si no existe (o está Pendiente/Reabierta), cae al comportamiento
/// histórico: incidencia de prenómina + derivación local de dobles/triples.
///
/// Fase 7 — la prenómina (snapshot) reutiliza este mismo helper para que su display cuadre con la
/// nómina (mismo origen, misma fórmula → sin divergencia).
///
/// Coherencia de la deducción (F5.5b): los minutos que la resolución absorbió en el extra
/// (faltante neto, retardo) se ALIVIAN de la deducción de sueldo — el extra "tapa" ese tiempo,
/// el empleado queda parejo, no se le doble-cobra. La salida anticipada y el descuento manual
/// no forman parte del neteo del periodo y se toman íntegros del input.
/// </summary>
public static class NominaTiempoExtraSourcing
{
    /// <summary>Mapper de conveniencia desde una incidencia de prenómina persistida.</summary>
    public static NominaOvertimeSourcingInput InputFrom(PrenominaDetalle incidencia) => new()
    {
        HorasExtra = incidencia.HorasExtra,
        HorasExtraBase = incidencia.HorasExtraBase,
        HorasBancoAcumuladas = incidencia.HorasBancoAcumuladas,
        MinutosRetardo = incidencia.MinutosRetardo,
        MinutosSalidaAnticipada = incidencia.MinutosSalidaAnticipada,
        MinutosPerdonadosManual = incidencia.MinutosPerdonadosManual,
        MinutosFaltanteDescontable = incidencia.MinutosFaltanteDescontable,
        MinutosDescuentoManual = incidencia.MinutosDescuentoManual
    };

    public static NominaOvertimeSourcing Source(
        NominaOvertimeSourcingInput input,
        RrhhResolucionTiempoExtraPeriodo? resolucion,
        NominaConfiguracion configuracion,
        decimal factorPersistido)
    {
        if (resolucion is { Estatus: RrhhResolucionPeriodoEstatus.Autorizada })
        {
            // Fase 8 — si la resolución se autorizó por líneas, HorasExtraFactoradas trae las
            // horas ponderadas (Σ pago.Minutos/60 × Factor) y el calculador las multiplica por
            // sueldoHora. FactorPagoTiempoExtra=0 señala al calculador que use factoradas.
            var porLineas = resolucion.HorasExtraFactoradas > 0m;
            // El banco del periodo es informativo (el ledger ya se escribió al autorizar en
            // AplicarResolucionPeriodoAsync; ni la nómina ni el snapshot recrean movimientos).
            return new NominaOvertimeSourcing
            {
                HorasExtra = resolucion.MinutosExtraPago / 60m,
                HorasExtraBase = resolucion.MinutosExtraDetectado / 60m,
                HorasExtraBanco = resolucion.MinutosExtraBanco / 60m,
                HorasExtraDobles = resolucion.MinutosExtraDobles / 60m,
                HorasExtraTriples = resolucion.MinutosExtraTriples / 60m,
                HorasExtraFactoradas = porLineas ? resolucion.HorasExtraFactoradas : 0m,
                FactorPagoTiempoExtra = porLineas ? 0m : (resolucion.FactorTiempoExtraAplicado ?? 0m),
                // Alivio de deducción: solo se descuenta lo NO absorbido por el extra.
                MinutosFaltanteDescontable = Math.Max(0, resolucion.MinutosFaltanteNetoDetectado - resolucion.MinutosFaltanteAbsorbidoExtra),
                MinutosRetardo = Math.Max(0, resolucion.MinutosRetardoDetectado - resolucion.MinutosRetardoAbsorbidoExtra),
                // Salida anticipada, descuento manual y perdon no participan en el neteo del periodo.
                MinutosSalidaAnticipada = input.MinutosSalidaAnticipada,
                MinutosDescuentoManual = input.MinutosDescuentoManual,
                MinutosPerdonadosManual = input.MinutosPerdonadosManual,
                Origen = "periodo"
            };
        }

        // Fallback histórico: incidencia de prenómina + derivación local de dobles/triples.
        var horasExtra = input.HorasExtra;
        var horasExtraBase = input.HorasExtraBase;
        var horasBase = Math.Max(0m, horasExtraBase > 0 ? horasExtraBase : horasExtra);
        var horasPagables = Math.Max(0m, horasExtra);
        var horasLegales = Math.Min(horasBase, horasPagables);
        var horasDoblesTope = Math.Max(0m, configuracion.HorasExtraDoblesPorSemana);
        var horasDobles = Math.Min(horasDoblesTope, horasLegales);
        var horasTriples = Math.Max(0m, horasLegales - horasDobles);

        return new NominaOvertimeSourcing
        {
            HorasExtra = horasExtra,
            HorasExtraBase = horasExtraBase,
            HorasExtraBanco = input.HorasBancoAcumuladas,
            HorasExtraDobles = horasDobles,
            HorasExtraTriples = horasTriples,
            // Preserva el factor que ya tenía el detalle (comportamiento previo al cutover).
            FactorPagoTiempoExtra = factorPersistido,
            MinutosFaltanteDescontable = input.MinutosFaltanteDescontable,
            MinutosRetardo = input.MinutosRetardo,
            MinutosSalidaAnticipada = input.MinutosSalidaAnticipada,
            MinutosDescuentoManual = input.MinutosDescuentoManual,
            MinutosPerdonadosManual = input.MinutosPerdonadosManual,
            Origen = "incidencia"
        };
    }
}