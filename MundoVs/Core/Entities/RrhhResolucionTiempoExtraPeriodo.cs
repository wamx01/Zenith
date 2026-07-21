namespace MundoVs.Core.Entities;

/// <summary>
/// Resolución de tiempo extra de un periodo de nómina (semanal/quincenal/mensual).
///
/// Punto de decisión A NIVEL PERIODO (no diario): el operador autoriza cuántos
/// minutos del extra detectado en el periodo van a pago y cuántos al banco de
/// horas, en una sola decisión por periodo, con desglose por día.
///
/// La DETECCIÓN sigue siendo diaria (RrhhAsistencia.MinutosExtra). Esta entidad
/// guarda la LIQUIDACIÓN del periodo: la suma de lo detectado + lo autorizado.
///
/// Fase 1: sin netting (faltante/retardo/extra siguen independientes). Los neteos
/// se agregan en fases posteriores, uno a la vez.
/// </summary>
public class RrhhResolucionTiempoExtraPeriodo : BaseEntity
{
    public Guid EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    // Identificación del periodo (espeja NominaPeriodoCalendario). La tupla
    // (PeriodicidadPago, AnioPeriodo, NumeroPeriodo) + FechaInicio/Fin identifica
    // de forma única el periodo aunque cambie DiaCorteSemana.
    public PeriodicidadPago PeriodicidadPago { get; set; } = PeriodicidadPago.Semanal;
    public int AnioPeriodo { get; set; }
    public int NumeroPeriodo { get; set; }
    public string PeriodoKey { get; set; } = string.Empty;      // ej. "Semanal-2026-03" (usado en ReferenciaTipo del ledger)
    public string PeriodoEtiqueta { get; set; } = string.Empty; // ej. "Semana 04/01 - 10/01/2026"

    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }

    // Detección agregada del periodo (suma de RrhhAsistencia de los días del
    // periodo). Solo lectura: se recalcula desde las asistencias, no se autoriza.
    public int MinutosExtraDetectado { get; set; }
    public int MinutosFaltanteDetectado { get; set; }
    public int MinutosRetardoDetectado { get; set; }
    public int MinutosTrabajadosNetosDetectado { get; set; }

    // Fase 2 — neteo: faltante del periodo NO cubierto por permiso con goce
    // (faltante "neto"). El extra absorbible tapa esto antes de ser pagable/banco.
    public int MinutosFaltanteNetoDetectado { get; set; }
    // Minutos de extra que se usaron para cubrir el faltante neto del periodo.
    public int MinutosFaltanteAbsorbidoExtra { get; set; }

    // Fase 3 — neteo: retardo del periodo. El sobrante de extra tras tapar el
    // faltante neto cubre el retardo del periodo ANTES de ser pagable/bancable.
    public int MinutosRetardoAbsorbidoExtra { get; set; }

    // Fase 4 — restauración: banco consumido en el periodo (Consumo del ledger,
    // excluyendo cobertura-banco que Fase 2 ya cuenta como faltante neto).
    public int MinutosBancoConsumidoDetectado { get; set; }
    // Minutos de extra sobrante (tras faltante y retardo) que se usaron para
    // REPONER el banco consumido en el periodo. Genera un movimiento positivo
    // al banco (restauracion-banco). Solo lo que sobra tras esto es pagable.
    public int MinutosBancoRestauradoExtra { get; set; }

    // Autorización del operador a nivel periodo. Fase 1: sin netting.
    public int MinutosExtraPago { get; set; }
    public int MinutosExtraBanco { get; set; }
    // Fase 5 — split legal del PAGO: primeros minutos hasta el techo configurable
    // (HorasExtraDoblesPorSemana) = dobles; excedente = triples. El banco NO se
    // reparte en dobles/triples (acumula en crudo). Fuente autoritativa del split.
    public int MinutosExtraDobles { get; set; }
    public int MinutosExtraTriples { get; set; }
    // Fase 8 — minutos a pago con factor que NO es dobles (2) ni triples (3) (ej. x1,
    // x1.5). Las líneas llevan el factor explícito; esta cubeta es derivada para
    // display/reportes. Los dobles/triples siguen siendo Σ pago(Factor==2/3).
    public int MinutosExtraSimples { get; set; }
    // Fase 8 — "horas ponderadas" a pago = Σ pago.Minutos/60 × Factor. El calculador
    // multiplica esto por sueldoHora → monto exacto sin importar cuántos factores
    // distintos haya. 0 (o sin líneas) → el calculador cae al path dobles/triples.
    public decimal HorasExtraFactoradas { get; set; }
    public decimal? FactorTiempoExtraAplicado { get; set; }
    public decimal? FactorAcumulacionBancoHorasAplicado { get; set; }

    public string? Resolucion { get; set; } // "PagarTodo" / "BancoTodo" / "MitadMitad" / custom
    public RrhhResolucionPeriodoEstatus Estatus { get; set; } = RrhhResolucionPeriodoEstatus.Pendiente;
    public string? AutorizadoPor { get; set; }
    public DateTime? FechaAutorizacion { get; set; }
    public string? Observaciones { get; set; }

    // F9 — extra DESCARTADO por el operador: se detectó extra pero NO se autorizó
    // compensación ni pago. En este caso NO aplica el neteo (los absorbidos quedan
    // en 0) → el faltante/retardo del periodo se descuenta COMPLETO en la nómina.
    // Sigue siendo Estatus=Autorizada (el periodo quedó resuelto y desbloquea el
    // gate de prenomina), pero sin pago ni alivio. La compensación NO es automática:
    // requiere autorización explícita (cualquier otro modo); sin ella, se descuenta.
    public bool ExtraDescartado { get; set; }

    // Fase 8 — líneas de resolución (un segmento por factor/destino). Vacío para
    // resoluciones pre-Fase 8 (caen al path escalar legado).
    public List<RrhhResolucionTiempoExtraLinea> Lineas { get; set; } = new();
}

/// <summary>
/// Línea de resolución de tiempo extra de un periodo (Fase 8). Un segmento de
/// minutos del extra absorbible del periodo que se autoriza con un factor
/// específico y un destino (pago o banco). Varias líneas por periodo permiten
/// repartir el extra con distintos factores (ej. 2h x1 + 3h x2).
///
/// El factor es de pago (Destino=Pago) o de acumulación al banco (Destino=Banco).
/// Los escalares derivados (MinutosExtraPago/Banco/Dobles/Triples/Simples y
/// HorasExtraFactoradas) se mantienen en sync con las líneas al autorizar.
/// </summary>
public class RrhhResolucionTiempoExtraLinea : BaseEntity
{
    public Guid EmpresaId { get; set; }      // denormalizado para queries / global filter
    public Guid EmpleadoId { get; set; }    // denormalizado
    public Guid ResolucionPeriodoId { get; set; }
    public RrhhResolucionTiempoExtraPeriodo ResolucionPeriodo { get; set; } = null!;

    public int Orden { get; set; }          // orden estable para display
    public RrhhDestinoTiempoExtraLinea Destino { get; set; } = RrhhDestinoTiempoExtraLinea.Pago;
    public int Minutos { get; set; }         // minutos base del segmento
    public decimal Factor { get; set; } = 1m; // factor de pago (Pago) o de acumulación (Banco)
    public string? Observaciones { get; set; }
}

public enum RrhhDestinoTiempoExtraLinea
{
    Pago = 1,
    Banco = 2
}

public enum RrhhResolucionPeriodoEstatus
{
    Pendiente = 1,
    Autorizada = 2,
    Reabierta = 3
}