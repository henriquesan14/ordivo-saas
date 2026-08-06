using Microsoft.EntityFrameworkCore;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.ServiceOrders;

namespace Ordivo.Infrastructure.Persistence.Repositories;

internal sealed class ServiceOrderRepository(OrdivoDbContext dbContext) : IServiceOrderRepository
{
    public async Task AddAsync(ServiceOrder order, CancellationToken ct) => await dbContext.ServiceOrders.AddAsync(order, ct);
    public Task<ServiceOrder?> GetAsync(Guid id, CancellationToken ct) => dbContext.ServiceOrders
        .Include(order => order.StatusHistory).Include(order => order.Comments).Include(order => order.Attachments)
        .SingleOrDefaultAsync(order => order.Id == id, ct);

    public async Task<long> NextSequenceAsync(CancellationToken ct) =>
        await dbContext.Database.SqlQueryRaw<long>("SELECT nextval('service_order_number_sequence') AS \"Value\"").SingleAsync(ct);

    public async Task<(IReadOnlyCollection<ServiceOrder> Items, int TotalCount)> ListAsync(
        string? search, ServiceOrderStatus? status, Guid? customerId, Guid? assignedUserId,
        DateTimeOffset? scheduledFrom, DateTimeOffset? scheduledTo,
        int page, int pageSize, string sortBy, bool descending, CancellationToken ct)
    {
        var query = dbContext.ServiceOrders.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = $"%{search.Trim()}%";
            query = query.Where(order => EF.Functions.ILike(order.Number, value) || EF.Functions.ILike(order.Title, value) || EF.Functions.ILike(order.Description, value));
        }
        if (status.HasValue) query = query.Where(order => order.Status == status.Value);
        if (customerId.HasValue) query = query.Where(order => order.CustomerId == customerId.Value);
        if (assignedUserId.HasValue) query = query.Where(order => order.AssignedUserId == assignedUserId.Value);
        if (scheduledFrom.HasValue) query = query.Where(order => order.ScheduledAt >= scheduledFrom.Value);
        if (scheduledTo.HasValue) query = query.Where(order => order.ScheduledAt <= scheduledTo.Value);
        var total = await query.CountAsync(ct);
        query = (sortBy.Trim().ToLowerInvariant(), descending) switch
        {
            ("number", false) => query.OrderBy(order => order.Number), ("number", true) => query.OrderByDescending(order => order.Number),
            ("title", false) => query.OrderBy(order => order.Title), ("title", true) => query.OrderByDescending(order => order.Title),
            ("status", false) => query.OrderBy(order => order.Status), ("status", true) => query.OrderByDescending(order => order.Status),
            ("scheduledat", false) => query.OrderBy(order => order.ScheduledAt), ("scheduledat", true) => query.OrderByDescending(order => order.ScheduledAt),
            ("price", false) => query.OrderBy(order => order.Price), ("price", true) => query.OrderByDescending(order => order.Price),
            ("createdat", false) => query.OrderBy(order => order.CreatedAt), _ => query.OrderByDescending(order => order.CreatedAt)
        };
        var items = await query.Include(order => order.StatusHistory).Include(order => order.Comments).Include(order => order.Attachments)
            .AsSplitQuery().Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
