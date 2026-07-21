using MundoVs.Core.Entities;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Interfaces;

public interface IRrhhPrenominaSnapshotService
{
    Task<IReadOnlyList<RrhhPrenominaSnapshotItem>> ConstruirSnapshotPeriodoAsync(CrmDbContext db, DateTime inicio, DateTime fin, NominaConfiguracion configuracion, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, EmpleadoEsquemaPago>> ObtenerEsquemasPagoPeriodoAsync(CrmDbContext db, DateTime inicio, DateTime fin, IReadOnlyCollection<Guid> empleadoIds, CancellationToken cancellationToken = default);
}

public sealed class RrhhPrenominaSnapshotItem
{
    public required Empleado Empleado { get; init; }
    public EmpleadoEsquemaPago? AsignacionEsquema { get; init; }
    public int DiasTrabajados { get; init; }
    public int DiasPagados { get; init; }
    public int DiasVacaciones { get; init; }
    public int DiasFaltaJustificada { get; init; }
    public int DiasFaltaInjustificada { get; init; }
    public int DiasIncapacidad { get; init; }
    public int DiasDescansoTrabajado { get; init; }
    public int DiasConMarcacion { get; init; }
    public int DiasDomingoTrabajado { get; init; }
    public int DiasFestivoTrabajado { get; init; }
    // F4b: desglose PorHoras (pago por minutos). DiasFestivoTrabajadoFija = festivos
    // trabajados no-PorHoras (los PorHoras festivos van al base por minutos × factor).
    public int DiasPorHorasTrabajados { get; init; }
    public int MinutosPorHorasNetos { get; init; }
    public int MinutosPorHorasFestivoNetos { get; init; }
    public int DiasFestivoTrabajadoFija { get; init; }
    public decimal HorasTrabajadasNetas { get; init; }
    public decimal HorasExtraBase { get; init; }
    public decimal HorasExtra { get; init; }
    public decimal HorasBancoAcumuladas { get; init; }
    public decimal HorasBancoConsumidas { get; init; }
    public decimal HorasBancoSaldoActual { get; init; }
    public decimal HorasDescansoTomado { get; init; }
    public decimal HorasDescansoPagado { get; init; }
    public decimal HorasDescansoNoPagado { get; init; }
    public int MinutosRetardo { get; init; }
    public int MinutosSalidaAnticipada { get; init; }
    public int MinutosRetardoOriginales { get; init; }
    public int MinutosSalidaAnticipadaOriginales { get; init; }
    public int MinutosPerdonadosManual { get; init; }
    public int MinutosFaltanteDescontable { get; init; }
    public int MinutosDescuentoManual { get; init; }
    public decimal FactorPagoTiempoExtra { get; init; }
    public decimal MontoDestajoInformativo { get; init; }
    public decimal DiasVacacionesDisponibles { get; init; }
    public decimal DiasVacacionesRestantes { get; init; }
    public decimal ComplementoSalarioMinimoSugerido { get; init; }
    public bool AplicaImss { get; init; }
    public string? Notas { get; init; }
}
