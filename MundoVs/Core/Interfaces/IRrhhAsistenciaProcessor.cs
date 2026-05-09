using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Interfaces;

public interface IRrhhAsistenciaProcessor
{
    Task ProcesarMarcacionesPendientesAsync(CrmDbContext db, Guid empresaId, Guid checadorId, CancellationToken cancellationToken = default);
    Task<int> ReprocesarRangoAsync(CrmDbContext db, Guid empresaId, DateOnly fechaDesde, DateOnly fechaHasta, Guid? empleadoId = null, CancellationToken cancellationToken = default);
    Task<int> ReprocesarRangoAsync(CrmDbContext db, Guid empresaId, DateOnly fechaDesde, DateOnly fechaHasta, Guid? empleadoId, IProgress<RrhhAsistenciaReprocesoProgreso>? progress, CancellationToken cancellationToken = default);
}

public sealed record RrhhAsistenciaReprocesoProgreso(int Procesados, int Total, Guid EmpleadoId, DateOnly Fecha);
