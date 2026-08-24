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
/// Coherencia de la deducción (F5.5b / fuente única 2026-08-22): el alivio de las deducciones
/// (faltante/retardo/salida) NO lo recalcula el sourcing — viene del snapshot de nómina, que lo
/// consume del batch canónico (<c>ObtenerResumenesPeriodoBatchAsync</c> → <c>CalcularNeteoNetoVsNeto</c>,
/// el MISMO neteo que pinta Asistencia Semanal). Así un faltante/retardo neteado a 0 en Asistencia
/// Semanal ya NO reaparece en nómina. La resolución Autorizada del periodo sólo aporta el extra
/// autorizado a PAGAR (pago/dobles/triples/factor/banco); el alivio de deducciones es el VIVO del
/// periodo, no el congelado al autorizar — que puede estar stale o zeroado si el operador
/// descartó el extra (<c>DescartarExtra</c> anula el PAGO, no el neteo de deducciones). El
/// descuento manual y los perdonados manuales se toman íntegros del input.
/// English: Deduction coherence (F5.5b / single source 2026-08-22): the deduction relief
/// (shortage/late/early-leave) is NOT recomputed by sourcing — it comes from the payroll snapshot,
/// which consumes it from the canonical batch (... → CalcularNeteoNetoVsNeto, the SAME neteo
/// Asistencia Semanal paints). So a shortage/late netted to 0 in Asistencia Semanal no longer
/// reappears in nómina. The period's Authorized resolution only contributes the extra authorized
/// to PAY (pay/doubles/triples/factor/bank); the deduction relief is the LIVE one for the period,
/// not the one frozen at authorization — which can be stale or zeroed if the operator discarded
/// the extra (DescartarExtra annuls the PAYMENT, not the deduction neteo). Manual discount and
/// manual forgiven minutes are taken intact from the input.
/// </summary>
public static class NominaTiempoExtraSourcing
{
    /// <summary>
    /// Mapper de conveniencia desde un <see cref="NominaDetalle"/> (fusión prenómina→nómina):
    /// la asistencia congelada vive en la propia nómina, así que el sourcing la lee de aquí.
    /// English: Convenience mapper from a <see cref="NominaDetalle"/> (prenómina→nómina fusion):
    /// the frozen attendance lives on the nómina itself, so sourcing reads it from here.
    /// </summary>
    public static NominaOvertimeSourcingInput InputFrom(NominaDetalle detalle) => new()
    {
        HorasExtra = detalle.HorasExtra,
        HorasExtraBase = detalle.HorasExtraBase,
        HorasBancoAcumuladas = detalle.HorasBancoAcumuladas,
        MinutosRetardo = detalle.MinutosRetardo,
        MinutosSalidaAnticipada = detalle.MinutosSalidaAnticipada,
        MinutosPerdonadosManual = detalle.MinutosPerdonadosManual,
        MinutosFaltanteDescontable = detalle.MinutosFaltanteDescontable,
        MinutosDescuentoManual = detalle.MinutosDescuentoManual
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
                // DEDUCCIONES (faltante/retardo/salida): vienen del input (snapshot), ya neteadas
                // por el batch canónico (RrhhResolucionPeriodoService.ObtenerResumenesPeriodoBatchAsync
                // → CalcularNeteoNetoVsNeto = el MISMO neteo que pinta Asistencia Semanal). El sourcing
                // NO recalcula el neteo aunque haya resolución Autorizada: el resumen autoritativo
                // del extra a PAGAR (pago/dobles/triples/factor) sí viene de la resolución, pero el
                // alivio de deducciones es el VIVO del periodo — no el congelado al autorizar, que
                // puede estar stale o zeroado si el operador descartó el extra (DescartarExtra anula
                // el PAGO, no el neteo de deducciones). Así nómina = Asistencia Semanal (cero drift).
                // English: DEDUCTIONS (shortage/late/early-leave) come from the input (snapshot),
                // already netted by the canonical batch (... → CalcularNeteoNetoVsNeto = the SAME
                // neteo Asistencia Semanal paints). Sourcing does NOT recompute neteo even with an
                // Autorizada resolution: the authoritative extra-to-PAY summary (pay/doubles/triples
                // /factor) does come from the resolution, but the deduction relief is the LIVE one
                // for the period — not the one frozen at authorization, which can be stale or zeroed
                // if the operator discarded the extra (DescartarExtra annuls the PAYMENT, not the
                // deduction neteo). So nómina = Asistencia Semanal (zero drift).
                MinutosFaltanteDescontable = input.MinutosFaltanteDescontable,
                MinutosRetardo = input.MinutosRetardo,
                MinutosSalidaAnticipada = input.MinutosSalidaAnticipada,
                MinutosDescuentoManual = input.MinutosDescuentoManual,
                MinutosPerdonadosManual = input.MinutosPerdonadosManual,
                Origen = "periodo"
            };
        }

        // Fallback histórico: incidencia (NominaDetalle congelado) + derivación local de
        // dobles/triples. Las deducciones (faltante/retardo/salida) YA vienen neteadas desde el
        // snapshot (que consume el neteo canónico de Asistencia Semanal vía
        // RrhhResolucionPeriodoService.ObtenerResumenesPeriodoBatchAsync), así que aquí sólo se
        // pasan intactas — el sourcing NO recalcula el neteo. Ambos paths (periodo e incidencia)
        // toman las deducciones del input; el "periodo" sólo cambia el extra a pagar.
        // English: Historical fallback: incidencia (frozen NominaDetalle) + local dobles/triples
        // derivation. The deductions (shortage/late/early-leave) ALREADY come netted from the
        // snapshot (which consumes Asistencia Semanal's canonical neteo via
        // RrhhResolucionPeriodoService.ObtenerResumenesPeriodoBatchAsync), so they pass through
        // intact here — sourcing does NOT recompute the neteo. Both paths (periodo and incidencia)
        // take deductions from the input; "periodo" only swaps in the extra to pay.
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