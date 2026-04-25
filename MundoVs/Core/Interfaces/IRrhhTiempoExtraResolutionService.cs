using MundoVs.Core.Entities;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Interfaces;

public interface IRrhhTiempoExtraResolutionService
{
    Task<int> ObtenerSaldoBancoHorasAsync(CrmDbContext db, Guid empresaId, Guid empleadoId, CancellationToken cancellationToken = default);
    Task<RrhhTiempoExtraConfiguracionSnapshot> ObtenerConfiguracionAsync(CrmDbContext db, Guid empresaId, CancellationToken cancellationToken = default);
    Task<RrhhTiempoExtraEmpleadoContexto> ObtenerContextoEmpleadoAsync(CrmDbContext db, Guid empresaId, Guid empleadoId, CancellationToken cancellationToken = default);
    Task<int> ObtenerTopeBancoHorasAsync(CrmDbContext db, Guid empresaId, CancellationToken cancellationToken = default);
    Task<decimal> ObtenerFactorTiempoExtraAsync(CrmDbContext db, Guid empresaId, CancellationToken cancellationToken = default);
    Task<bool> ObtenerBancoHorasHabilitadoAsync(CrmDbContext db, Guid empresaId, CancellationToken cancellationToken = default);
    Task<decimal> ObtenerFactorAcumulacionBancoHorasAsync(CrmDbContext db, Guid empresaId, CancellationToken cancellationToken = default);
    Task<RrhhTiempoExtraResolutionResult> AplicarResolucionAsync(CrmDbContext db, RrhhTiempoExtraResolutionCommand command, CancellationToken cancellationToken = default);
    Task<int> AplicarPermisoConGoceBancoHorasAsync(CrmDbContext db, RrhhPermisoBancoHorasCommand command, CancellationToken cancellationToken = default);
    Task<int> RemoverPermisoBancoHorasAsync(CrmDbContext db, Guid empresaId, Guid empleadoId, Guid ausenciaId, CancellationToken cancellationToken = default);
}

public sealed class RrhhTiempoExtraConfiguracionSnapshot
{
    public int TopeBancoMinutos { get; init; }
    public decimal FactorTiempoExtra { get; init; }
    public bool BancoHorasHabilitado { get; init; }
    public decimal FactorAcumulacionBancoHoras { get; init; }
}

public sealed class RrhhTiempoExtraEmpleadoContexto
{
    public required RrhhTiempoExtraConfiguracionSnapshot Configuracion { get; init; }
    public int SaldoBancoHorasMinutos { get; init; }
}

public sealed class RrhhTiempoExtraResolutionCommand
{
    public Guid EmpresaId { get; init; }
    public Guid AsistenciaId { get; init; }
    public string Resolucion { get; init; } = string.Empty;
    public int MinutosPago { get; init; }
    public int MinutosBanco { get; init; }
    public int MinutosCubrirBanco { get; init; }
    public string? Observaciones { get; init; }
    public string UsuarioActual { get; init; } = string.Empty;
}

public sealed class RrhhTiempoExtraResolutionResult
{
    public required RrhhAsistencia Asistencia { get; init; }
    public int SaldoBancoActualMinutos { get; init; }
    public int TopeBancoMinutos { get; init; }
    public decimal FactorTiempoExtra { get; init; }
    public bool BancoHorasHabilitado { get; init; }
    public decimal FactorAcumulacionBancoHoras { get; init; }
    public int MinutosPagoAplicados { get; init; }
    public int MinutosBancoAplicados { get; init; }
    public int MinutosCubiertosBancoAplicados { get; init; }
    public string BitacoraDetalle { get; init; } = string.Empty;
}

public sealed class RrhhPermisoBancoHorasCommand
{
    public Guid EmpresaId { get; init; }
    public Guid EmpleadoId { get; init; }
    public Guid AusenciaId { get; init; }
    public DateOnly Fecha { get; init; }
    public decimal HorasPermiso { get; init; }
    public string? Observaciones { get; init; }
    public string UsuarioActual { get; init; } = string.Empty;
}
