using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MundoVs.Infrastructure.Data;

namespace MundoVs.Core.Services;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbContextFactory<CrmDbContext> _dbFactory;

    public DatabaseHealthCheck(IDbContextFactory<CrmDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var canConnect = await db.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Base de datos disponible.")
                : HealthCheckResult.Unhealthy("No fue posible conectar a la base de datos.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error al validar la base de datos.", ex);
        }
    }
}
