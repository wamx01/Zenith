using Microsoft.EntityFrameworkCore;
using MundoVs.Core.Models;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Interfaces;

/// <summary>
/// Construye el reporte compilado de tiempo extra por empleado y fecha para una empresa,
/// consultando las asistencias ya procesadas por el motor RRHH.
/// </summary>
public interface IRrhhTiempoExtraReporteService
{
    Task<RrhhTiempoExtraReporteResponse> GenerarAsync(
        IDbContextFactory<MundoVs.Infrastructure.Data.CrmDbContext> dbFactory,
        RrhhTiempoExtraReporteRequest request,
        CancellationToken cancellationToken = default);
}
