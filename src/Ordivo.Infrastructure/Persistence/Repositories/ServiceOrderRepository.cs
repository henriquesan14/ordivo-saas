using Microsoft.EntityFrameworkCore;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.ServiceOrders;

namespace Ordivo.Infrastructure.Persistence.Repositories;

internal sealed class ServiceOrderRepository(OrdivoDbContext dbContext) : IServiceOrderRepository
{
    public async Task AddAsync(ServiceOrder order, CancellationToken ct) => await dbContext.ServiceOrders.AddAsync(order, ct);
    public Task<ServiceOrder?> GetAsync(Guid id, CancellationToken ct) => dbContext.ServiceOrders.SingleOrDefaultAsync(order => order.Id == id, ct);
    public async Task<IReadOnlyCollection<ServiceOrder>> ListAsync(CancellationToken ct) =>
        await dbContext.ServiceOrders.AsNoTracking().OrderByDescending(order => order.CreatedAt).ToListAsync(ct);
}
