using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Interfaces;

public interface IRrhhAsistenciaProcessor
{
    Task ProcesarMarcacionesPendientesAsync(CrmDbContext db, Guid empresaId, Guid checadorId, CancellationToken cancellationToken = default);
    Task<int> ReprocesarRangoAsync(CrmDbContext db, Guid empresaId, DateOnly fechaDesde, DateOnly fechaHasta, Guid? empleadoId = null, CancellationToken cancellationToken = default);
}
