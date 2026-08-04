using Microsoft.EntityFrameworkCore;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Tenants;

namespace Ordivo.Infrastructure.Persistence.Repositories;

internal sealed class PlatformTenantRepository(OrdivoDbContext dbContext) : IPlatformTenantRepository
{
    public async Task<IReadOnlyCollection<Tenant>> ListAsync(CancellationToken ct) =>
        await dbContext.Tenants.IgnoreQueryFilters().AsNoTracking().OrderBy(tenant => tenant.Name).ToListAsync(ct);

    public Task<Tenant?> GetAsync(Guid id, CancellationToken ct) =>
        dbContext.Tenants.IgnoreQueryFilters().SingleOrDefaultAsync(tenant => tenant.Id == id, ct);
}
