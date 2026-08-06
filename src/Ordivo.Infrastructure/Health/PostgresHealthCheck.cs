using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Ordivo.Infrastructure.Persistence;

namespace Ordivo.Infrastructure.Health;

internal sealed class PostgresHealthCheck(IServiceScopeFactory scopeFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<OrdivoDbContext>();
            return await db.Database.CanConnectAsync(ct)
                ? HealthCheckResult.Healthy("PostgreSQL is reachable.")
                : HealthCheckResult.Unhealthy("PostgreSQL is unreachable.");
        }
        catch (Exception ex) { return HealthCheckResult.Unhealthy("PostgreSQL health check failed.", ex); }
    }
}
