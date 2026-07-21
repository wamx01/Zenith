using MundoVs.Core.Entities;
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
    public int MinutosTrabajadosNetos { get; init; }
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
    public int MinutosFaltanteDetectado { get; init; }
    public int MinutosFaltanteNetoPeriodo { get; init; } // faltante no cubierto por permiso con goce
    public int MinutosPermisoConGocePeriodo { get; init; }
    public int MinutosRetardoDetectado { get; init; }
    public int MinutosTrabajadosNetosDetectado { get; init; }

    // Fase 2: extra pagable/bancable tras tapar el faltante neto del periodo.
    public int MinutosExtraAbsorbible { get; init; }
    public int MinutosFaltanteAbsorbidoExtra { get; init; }
    // Fase 3: minutos de extra que taparon el retardo del periodo.
    public int MinutosRetardoAbsorbidoExtra { get; init; }
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