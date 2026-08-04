using Microsoft.EntityFrameworkCore;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Tenants;

namespace Ordivo.Infrastructure.Persistence.Repositories;

internal sealed class TenantRepository(OrdivoDbContext dbContext) : ITenantRepository
{
    public async Task<Tenant?> GetAsync(Guid id, CancellationToken ct) =>
        await dbContext.Tenants.SingleOrDefaultAsync(tenant => tenant.Id == id, ct);
    public async Task AddAsync(Tenant tenant, CancellationToken ct) => await dbContext.Tenants.AddAsync(tenant, ct);
}
