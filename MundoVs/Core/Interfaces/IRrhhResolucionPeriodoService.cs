using MundoVs.Core.Entities;
using MundoVs.Core.Services;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Interfaces;

/// <summary>
/// Resolución de tiempo extra A NIVEL PERIODO de nómina.
/// La detección sigue siendo diaria (RrhhAsistencia.MinutosExtra); la LIQUIDACIÓN
/// (pago / banco) se autoriza por periodo, en una sola decisión por empleado.
/// Fase 1: sin netting (faltante/retardo/extra siguen independientes).
/// </summary>
public interface IRrhhResolucionPeriodoService
{
    /// <summary>
    /// Resuelve la ventana del periodo de nómina para el empleado en la fecha de
    /// referencia y obtiene o crea la entidad de resolución. Lanza si el empleado
    /// es Destajo (fuera del flujo de resolución por periodo).
    /// </summary>
    Task<RrhhResolucionTiempoExtraPeriodo> ObtenerOCrearPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaReferencia, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumen de un periodo: detección agregada (extra/faltante/retardo/neto) +
    /// desglose por día + configuración de banco + saldo. Solo lectura: no crea
    /// la entidad. <see cref="EsAplicable"/> es false para Destajo.
    /// </summary>
    Task<RrhhResolucionPeriodoResumen> ObtenerResumenPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaReferencia, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumen de un periodo forzando el rango de fechas exacto (inicio/fin). Útil cuando
    /// la vista ya conoce la ventana mostrada y se quiere evitar que el servicio
    /// recalcule el periodo con reglas de cierre de nómina distintas.
    /// </summary>
    Task<RrhhResolucionPeriodoResumen> ObtenerResumenPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaInicio, DateOnly fechaFin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Autoriza la resolución del periodo: reparte el extra detectado entre pago y
    /// banco. Sin netting (Fase 1). No toca RrhhAsistencia. Idempotente por
    /// ReferenciaTipo del ledger del periodo.
    /// </summary>
    Task<RrhhResolucionPeriodoResult> AplicarResolucionPeriodoAsync(
        CrmDbContext db, RrhhResolucionPeriodoCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reabre un periodo autorizado: revierte el movimiento de banco acumulado y
    /// deja la resolución en Reabierta para que el operador re-apruebe (tras una
    /// corrección de marcación que cambia el extra detectado del periodo).
    /// </summary>
    Task ReabrirPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId, DateOnly fechaReferencia, string usuarioActual, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reabre un periodo autorizado a partir del rango explícito (vista contenedor)
    /// en vez de inferirlo desde una fecha de referencia. Útil cuando la fecha de
    /// referencia cae fuera de la semana visualizada y resolvería otro periodo.
    /// English: Reopens an authorized period from the explicit range (container view)
    /// instead of inferring it from a reference date — used when the reference date
    /// falls outside the displayed week and would otherwise resolve a different period.
    /// </summary>
    Task ReabrirPeriodoAsync(
        CrmDbContext db, Guid empresaId, Guid empleadoId,
        DateOnly? fechaInicioPeriodo, DateOnly? fechaFinPeriodo, DateOnly fechaReferencia,
        string usuarioActual, CancellationToken cancellationToken = default);

    /// <summary>
    /// Migración one-shot: genera <see cref="RrhhResolucionTiempoExtraPeriodo"/>
    /// (Autorizada) a partir de la suma de las columnas diarias heredadas
    /// <c>MinutosExtraAutorizadosPago/Banco</c>, agrupando por (empleado, periodo de nómina).
    /// Idempotente: salta los periodos que ya tienen entidad. No genera movimientos
    /// de banco (esos ya existen desde el path diario histórico).
    /// </summary>
    Task<RrhhResolucionPeriodoBackfillResult> BackfillDesdeAutorizacionDiariaAsync(
        CrmDbContext db, Guid? empresaId = null, string usuarioActual = "backfill", CancellationToken cancellationToken = default);

    /// <summary>
    /// Fase 9 (backfill opcional, no bloquea): siembra <see cref="RrhhResolucionTiempoExtraLinea"/>
    /// en resoluciones <see cref="RrhhResolucionPeriodoEstatus.Autorizada"/> PRE-Fase 8 que no
    /// tienen líneas (caen al path escalar legado). A partir de los escalares persistidos
    /// (MinutosExtraPago/Banco, MinutosExtraDobles/Triples, FactorTiempoExtraAplicado) reconstruye:
    /// una línea de pago dobles @ factor + una línea de pago triples @ factor (reproduciendo el
    /// split legal y el monto exacto del path legado), y una línea de banco @ factor de
    /// acumulación. Recalcula <c>HorasExtraFactoradas</c> y pone <c>FactorTiempoExtraAplicado</c>
    /// en null → la resolución pasa al path por líneas (monto idéntico al legado). Idempotente:
    /// solo procesa periodos SIN líneas. No toca el ledger del banco (los movimientos ya existen).
    /// </summary>
    Task<RrhhResolucionPeriodoBackfillLineasResult> SembrarLineasEnResolucionesAutorizadasAsync(
        CrmDbContext db, Guid? empresaId = null, string usuarioActual = "backfill", CancellationToken cancellationToken = default);

    /// <summary>
    /// Gate (Fase 7): empleados activos de la <paramref name="periodicidad"/> con tiempo extra
    /// detectado en el rango (Sum(MinutosExtra) &gt; 0) y SIN resolución Autorizada del periodo.
    /// Vacío = OK para avanzar. Solo bloquea cuando existe extra no autorizado — un periodo sin
    /// extra no se bloquea; un empleado sin extra no requiere resolución. Read-only.
    /// </summary>
    Task<IReadOnlyList<Guid>> ObtenerEmpleadosConExtraSinAutorizarAsync(
        CrmDbContext db, Guid empresaId, PeriodicidadPago periodicidad,
        DateTime inicio, DateTime fin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resúmenes de resolución por periodo para VARIOS empleados en una sola pasada (sin N+1).
    /// El listado semanal lo usa para mostrar las MISMAS columnas neteadas que el drawer del
    /// detalle (Total Hrs / Hrs Pagadas / Entro a Banco / Usaste Banco / Saldo Banco / Adic. a
    /// Pago / Ded. faltante / Ded. retardo / Ded. salida), sin replicar la lógica de neteo.
    /// <paramref name="calendario"/> es el periodo compartido por todos los empleados del rango.
    /// English: Period resolution summaries for SEVERAL employees in a single pass (no N+1). The
    /// weekly listing uses it to show the SAME netted columns as the detail drawer, without
    /// duplicating the netting logic. calendario is the period shared by all employees in the range.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, RrhhResolucionPeriodoResumen>> ObtenerResumenesPeriodoBatchAsync(
        CrmDbContext db, Guid empresaId, IReadOnlyCollection<Guid> empleadoIds,
        DateOnly fechaInicio, DateOnly fechaFin, PeriodicidadPago periodicidad,
        NominaPeriodoCalendario calendario, CancellationToken cancellationToken = default);
}

/// <summary>Resumen de la migración one-shot desde autorización diaria a periodos.</summary>
public sealed class RrhhResolucionPeriodoBackfillResult
{
    public int EmpleadosProcesados { get; init; }
    public int PeriodosCreados { get; init; }
    public int PeriodosOmitidos { get; init; }
}

/// <summary>Resumen del backfill de líneas Fase 9 (siembra líneas en Autorizada sin líneas).</summary>
public sealed class RrhhResolucionPeriodoBackfillLineasResult
{
    public int PeriodosProcesados { get; init; }
    public int PeriodosOmitidos { get; init; }   // ya tenían líneas
    public int LineasCreadas { get; init; }
}

public sealed class RrhhResolucionPeriodoCommand
{
    public Guid EmpresaId { get; init; }
    public Guid EmpleadoId { get; init; }
    public DateOnly FechaReferencia { get; init; }

    /// Rango explícito del periodo que se está autorizando (vista contenedor, p.ej.
    /// Mié 05/08 .. Mar 11/08). Cuando ambas vienen seteadas, la resolución se calcula
    /// sobre ESTE rango — igual que el preview que usa ObtenerResumenPeriodoAsync(rango).
    /// Si NO vienen, se resuelve desde FechaReferencia (comportamiento legado). Esto evita
    /// el bug donde FechaReferencia cae en otra semana (p.ej. "hoy") y el apply autoriza un
    /// periodo distinto al que el operador veía en pantalla.
    /// English: Explicit range of the period being authorized (container view, e.g. Wed
    /// 05/08 .. Tue 11/08). When both are set, the resolution is computed over THIS range —
    /// same as the preview's ObtenerResumenPeriodoAsync(range). If NOT set, it resolves from
    /// FechaReferencia (legacy behavior). This avoids the bug where FechaReferencia falls in
    /// a different week (e.g. "today") and the apply authorizes a period other than the one
    /// the operator sees on screen.
    public DateOnly? FechaInicioPeriodo { get; init; }
    public DateOnly? FechaFinPeriodo { get; init; }
    public string Resolucion { get; init; } = string.Empty;
    public decimal? FactorTiempoExtraOverride { get; init; }
    public int MinutosBasePago { get; init; }
    public int MinutosBaseBanco { get; init; }
    public string? Observaciones { get; init; }
    public string UsuarioActual { get; init; } = string.Empty;

    /// <summary>
    /// F9 — DESCARTAR el extra detectado: el operador acepta la detección (el
    /// periodo queda resuelto y desbloquea el gate de prenómina) pero NO autoriza
    /// compensación ni pago. La compensación NO es automática: requiere autorización
    /// explícita (cualquier otro modo); sin ella, el faltante/retardo del periodo se
    /// descuenta COMPLETO. Incompatible con <see cref="MinutosBasePago"/>,
    /// <see cref="MinutosBaseBanco"/> y <see cref="Lineas"/> (deben ir en 0/vacío).
    /// </summary>
    public bool DescartarExtra { get; init; }

    /// <summary>
    /// Fase 8 — líneas de resolución (un segmento por factor/destino). Si la lista
    /// trae elementos, la autorización se hace por líneas (cada una con sus minutos,
    /// factor y destino Pago/Banco). Si está vacía, se usa el path legado de bucket
    /// único (MinutosBasePago/MinutosBaseBanco + FactorTiempoExtraOverride + split
    /// legal dobles/triples).
    /// </summary>
    public IReadOnlyList<RrhhResolucionPeriodoLineaCommand> Lineas { get; init; } = Array.Empty<RrhhResolucionPeriodoLineaCommand>();

    /// <summary>
    /// Permisos por diferencia neta generados al cierre del periodo cuando
    /// |retardoDetectado − extraDetectado| > 0. El operador reparte la diferencia
    /// entre hasta 3 categorías (Banco / ConGoceSinBanco / SinGoce). Cada categoría
    /// con Minutos > 0 crea una fila sintética en RrhhAusencia (Tipo=PermisoPorDiferenciaPeriodo);
    /// la categoría Banco además consume saldo del banco de horas. Vacío o suma=0
    /// significa "sin permisos por diferencia" (idempotente: re-aplicar con la lista
    /// vacía REVIERTE las sintéticas previas del periodo, igual que reabrir).
    /// </summary>
    public IReadOnlyList<PermisoDiferenciaInput>? PermisosPorDiferencia { get; init; }
}

/// <summary>
/// Fase 8 — un segmento de la resolución de tiempo extra del periodo. El factor es
/// de pago (Destino=Pago) o de acumulación al banco (Destino=Banco).
/// </summary>
public sealed class RrhhResolucionPeriodoLineaCommand
{
    public RrhhDestinoTiempoExtraLinea Destino { get; init; } = RrhhDestinoTiempoExtraLinea.Pago;
    public int Minutos { get; init; }
    public decimal Factor { get; init; } = 1m;
    public string? Observaciones { get; init; }
}

public sealed class RrhhResolucionPeriodoDia
{
    public DateOnly Fecha { get; init; }
    public int MinutosExtra { get; init; }
    public int MinutosFaltante { get; init; }
    public int MinutosFaltanteNeto { get; init; } // faltante no cubierto por permiso con goce
    public int MinutosPermisoConGoce { get; init; }
    public int MinutosRetardo { get; init; }
    // Salida anticipada detectada del día (bruto; el neteo es a nivel periodo).
    // English: Detected early-leave for the day (raw; netting is at period level).
    public int MinutosSalidaAnticipada { get; init; }
    public int MinutosTrabajadosNetos { get; init; }
    // Jornada neta programada del día (la meta del turno; 0 si no hay turno). Base-calc, no bruto.
    // English: Scheduled net jornada for the day (the shift meta; 0 if no shift). Base-calc, not raw.
    public int MinutosJornadaProgramada { get; init; }
    // Base pagada del día = ObtenerMinutosBasePagada = min(netoEfectivo − extraDetectado,
    // jornadaNetaProgramada). Es EXACTAMENTE el "Normal" por día del listado AsistenciasSemanal:
    // el día con shortfall aporta su tiempo trabajado (descuenta el faltante de las horas), el
    // día con extra aporta la jornada (el extra va aparte). Base-calc, no marcaciones en bruto.
    // English: Paid base for the day = ObtenerMinutosBasePagada = min(netEffective − detectedExtra,
    // scheduledNetJornada). This is EXACTLY the per-day "Normal" of the AsistenciasSemanal listing:
    // a day with a shortfall contributes its worked time (deducts the shortfall from the hours),
    // a day with extra contributes the jornada (extra goes separately). Base-calc, not raw marks.
    public int MinutosBasePagada { get; init; }
}

public sealed class RrhhResolucionPeriodoResumen
{
    public bool EsAplicable { get; init; }
    public RrhhResolucionTiempoExtraPeriodo? Periodo { get; init; }
    public PeriodicidadPago PeriodicidadPago { get; init; }
    public int AnioPeriodo { get; init; }
    public int NumeroPeriodo { get; init; }
    public string PeriodoKey { get; init; } = string.Empty;
    public string PeriodoEtiqueta { get; init; } = string.Empty;
    public DateOnly FechaInicio { get; init; }
    public DateOnly FechaFin { get; init; }

    public int MinutosExtraDetectado { get; init; }
    // Extra crudo bajo umbral (no pagadero) que entró al pool del neteo para tapar deducciones
    // de otros días. NO se paga; sólo se muestra para que la aritmética del neteo cuadre.
    // English: Below-threshold raw extra (non-payable) that entered the neteo pool to cover
    // other days' deductions. NOT paid; only shown so the neteo arithmetic reconciles.
    public int MinutosExtraBajoUmbralNoPagadero { get; init; }
    public int MinutosFaltanteDetectado { get; init; }
    public int MinutosFaltanteNetoPeriodo { get; init; } // faltante no cubierto por permiso con goce
    public int MinutosPermisoConGocePeriodo { get; init; }
    public int MinutosRetardoDetectado { get; init; }
    // Salida anticipada detectada del periodo (bruto). Entra al neteo tras retardo.
    // English: Detected early-leave for the period (raw). Enters net after late.
    public int MinutosSalidaAnticipadaDetectado { get; init; }
    public int MinutosTrabajadosNetosDetectado { get; init; }

    // Fase 2: extra pagable/bancable tras tapar el faltante neto del periodo.
    public int MinutosExtraAbsorbible { get; init; }
    public int MinutosFaltanteAbsorbidoExtra { get; init; }
    // Fase 3: minutos de extra que taparon el retardo del periodo.
    public int MinutosRetardoAbsorbidoExtra { get; init; }
    // Fase 3b: minutos de extra que taparon la salida anticipada del periodo (tras
    // faltante y retardo, antes de restaurar banco / ser pagable).
    // English: Phase 3b: extra minutes that covered the period's early-leave (after
    // shortage and late, before restoring bank / becoming payable).
    public int MinutosSalidaAnticipadaAbsorbidoExtra { get; init; }
    // Fase 4: banco consumido en el periodo (Consumo del ledger, excluye
    // cobertura-banco) y minutos de extra que se usan para reponerlo.
    public int MinutosBancoConsumidoPeriodo { get; init; }
    public int MinutosBancoRestauradoExtra { get; init; }
    // Fase 5: split legal del PAGO (dobles hasta el techo configurable, triples el
    // excedente). Persistido en la entidad al autorizar; 0 si el periodo está pendiente.
    public int MinutosExtraDobles { get; init; }
    public int MinutosExtraTriples { get; init; }

    public int SaldoBancoHorasMinutos { get; init; }
    public int TopeBancoMinutos { get; init; }
    public decimal FactorTiempoExtra { get; init; }
    public bool BancoHorasHabilitado { get; init; }
    public decimal FactorAcumulacionBancoHoras { get; init; }

    // Meta semanal (Fija sin turno): cuando EsMetaSemanal, los totales del periodo se
    // calculan contra una meta de HorasBase del periodo (default 48h) en vez de la suma
    // per-día. El extra es lo trabajado sobre la meta; el déficit (FaltanteNetoPeriodo)
    // es lo que falta bajo la meta y descuenta sueldo. La UI muestra meta + trabajado
    // para que el operador entienda el origen del extra/déficit.
    // English: Weekly meta (Fija with no shift): when EsMetaSemanal, the period totals are
    // computed against a HorasBase meta (default 48h) instead of the per-day sum. Extra is
    // worked time over the meta; the deficit (FaltanteNetoPeriodo) is the shortfall under
    // the meta and docks salary. The UI shows meta + worked so the operator understands the
    // origin of the extra/deficit.
    public bool EsMetaSemanal { get; init; }
    // ¿El periodo entero es PorHoras (sin jornada programada)? Para PorHoras la base pagada
    // es el tiempo trabajado (pago por hora), no jornada − deducciones (no hay jornada ni
    // faltante/retardo/salida). Mutuamente excluyente con EsMetaSemanal.
    // English: Is the whole period PorHoras (no scheduled shift)? For PorHoras the paid base
    // is worked time (hourly pay), not jornada − deductions (no jornada/shortage/late/leave).
    // Mutually exclusive with EsMetaSemanal.
    public bool EsPorHoras { get; init; }
    public int MinutosMetaSemanal { get; init; }
    public int MinutosTrabajadosMetaSemanal { get; init; }

    // Jornada programada del periodo: suma per-día de MinutosJornadaNetaProgramada para
    // Fija-con-turno (= la meta del turno, p.ej. 2880); para Fija-sin-turno (EsMetaSemanal)
    // vale 0 y se usa MinutosMetaSemanal. Es la base salarial bruta, ANTES de deducciones.
    // English: Scheduled jornada for the period: per-day sum of MinutosJornadaNetaProgramada
    // for Fija-with-shift (= the shift meta, e.g. 2880); for Fija-with-no-shift (EsMetaSemanal)
    // it's 0 and MinutosMetaSemanal is used instead. This is the gross salary base, BEFORE
    // deductions.
    public int MinutosJornadaProgramadaPeriodo { get; init; }

    // Base pagada del periodo CALCULADA CON LA MISMA FÓRMULA QUE EL LISTADO de
    // AsistenciasSemanal ("Normal" = Σ ObtenerMinutosBasePagada por día). El listado es la
    // base canónica con la que el operador decide extra vs descuento, así que el detalle
    // (modal + drawer) debe mostrar exactamente eso: por día min(netoEfectivo − extraDetectado,
    // jornadaNetaProgramada) sumado. Así: (a) coincide con el "Normal" del listado, (b) el día
    // con shortfall (trabajó menos que su jornada) aporta su tiempo trabajado (descuenta el
    // faltante de las horas), (c) el tope por día corrige el bug de redondeo del umbral per-día
    // del extra (Francisco 48.00, no 48.03) SIN ignorar lo trabajado como hacía la jornada
    // programada cruda. Para PorHoras (sin jornada) es neto − extra aprobado (pago por hora).
    // English: Period paid base COMPUTED WITH THE SAME FORMULA AS THE AsistenciasSemanal
    // listing ("Normal" = Σ ObtenerMinutosBasePagada per day). The listing is the canonical
    // base the operator uses to decide extra vs discount, so the detail (modal + drawer) must
    // show exactly that: per day min(netEffective − detectedExtra, scheduledNetJornada) summed.
    // This: (a) matches the listing "Normal", (b) a day with a shortfall (worked less than its
    // shift) contributes its worked time (deducts the shortfall from the hours), (c) the per-day
    // cap fixes the per-day extra-threshold rounding bug (Francisco 48.00, not 48.03) WITHOUT
    // ignoring worked time the way raw scheduled jornada did. For PorHoras (no shift) it is
    // neto − approved extra (hourly pay).
    public int MinutosBasePagadaCalculado { get; init; }

    // "Hrs Pagadas" (display): la base pagada como salario, NETEADA por el extra del periodo.
    // = jornada − deduccionesNetas, donde deduccionesNetas es lo que el extra NO tapó
    // (faltante + retardo + salida, cada uno descontado lo absorbido). El extra de un día
    // tapa el faltante/retardo/salida de otro → la base refleja esa compensación cruzada.
    // Para EsMetaSemanal, MinutosJornadaProgramadaPeriodo ya vale la meta (2880) → la misma
    // fórmula cubre ambos casos (BajoMeta: 2880 − déficit = base coherente, no 2880 fijo).
    // Para PorHoras (EsPorHoras) NO hay jornada ni deducciones → se usa la base por hora
    // trabajada (MinutosBasePagadaCalculado = Σ neto − extra aprobado), igual que el listado.
    // Es display: la nómina paga sueldoDiario×diasPagados y resta las MISMAS deduccionesNetas
    // (minutos→$), así que no hay doble-conteo (base salarial y deducciones son independientes).
    // English: "Paid Hours" (display): the salary base, NETTED by the period's extra. = jornada
    // − netDeductions, where netDeductions is what extra did NOT cover (shortage + late + early-
    // leave, each minus its absorbed). One day's extra covers another day's shortage/late/leave
    // → the base reflects that cross-day compensation. For EsMetaSemanal, jornada already equals
    // the meta (2880) → the same formula covers both (BajoMeta: 2880 − deficit = coherent base,
    // not a fixed 2880). For PorHoras (EsPorHoras) there is no jornada/deductions → use the
    // hourly worked base (MinutosBasePagadaCalculado = Σ net − approved extra), same as listing.
    // Display-only: payroll pays sueldoDiario×diasPagados and subtracts the SAME netDeductions
    // (minutes→$), so no double-count (salary base and deductions independent).
    public int MinutosBasePagadaPeriodo =>
        EsPorHoras ? MinutosBasePagadaCalculado
        : Math.Max(0, MinutosJornadaProgramadaPeriodo - DeduccionesNetasPeriodo);

    // Deducciones que realmente descuenta la nómina tras el neteo del extra del periodo.
    // = Σ max(0, detectado − absorbido) para faltante neto, retardo y salida anticipada.
    // Espejo exacto de lo que el sourcing "periodo" resta (NominaTiempoExtraSourcing.Source).
    // English: Deductions payroll actually docks after the period's extra nets them.
    // = Σ max(0, detected − absorbed) for net shortage, late and early-leave. Exact mirror
    // of what the "periodo" sourcing subtracts (NominaTiempoExtraSourcing.Source).
    internal int DeduccionesNetasPeriodo =>
        Math.Max(0, MinutosFaltanteNetoPeriodo - MinutosFaltanteAbsorbidoExtra)
        + Math.Max(0, MinutosRetardoDetectado - MinutosRetardoAbsorbidoExtra)
        + Math.Max(0, MinutosSalidaAnticipadaDetectado - MinutosSalidaAnticipadaAbsorbidoExtra);

    public IReadOnlyList<RrhhResolucionPeriodoDia> Dias { get; init; } = Array.Empty<RrhhResolucionPeriodoDia>();
}

public sealed class RrhhResolucionPeriodoResult
{
    public required RrhhResolucionTiempoExtraPeriodo Periodo { get; init; }
    public int SaldoBancoActualMinutos { get; init; }
    public int TopeBancoMinutos { get; init; }
    public decimal FactorTiempoExtra { get; init; }
    public bool BancoHorasHabilitado { get; init; }
    public decimal FactorAcumulacionBancoHoras { get; init; }
    public int MinutosBasePagoAplicados { get; init; }
    public int MinutosBaseBancoAplicados { get; init; }
    public int MinutosPagoAplicados { get; init; }
    public int MinutosBancoAplicados { get; init; }
    public string BitacoraDetalle { get; init; } = string.Empty;
}