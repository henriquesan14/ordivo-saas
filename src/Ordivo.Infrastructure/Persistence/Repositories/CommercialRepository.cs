using Microsoft.EntityFrameworkCore;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Commercial;

namespace Ordivo.Infrastructure.Persistence.Repositories;

internal sealed class CommercialRepository(OrdivoDbContext db) : ICommercialRepository
{
    public async Task<IReadOnlyCollection<Plan>> ListPlansAsync(bool activeOnly, CancellationToken ct) =>
        await db.Plans.AsNoTracking().Where(x => !activeOnly || x.IsActive).OrderBy(x => x.Price).ToListAsync(ct);
    public Task<Plan?> GetPlanAsync(Guid id, CancellationToken ct) => db.Plans.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> PlanCodeExistsAsync(string code, Guid? excludingId, CancellationToken ct) => db.Plans.AnyAsync(x => x.Code == code.Trim().ToLower() && (!excludingId.HasValue || x.Id != excludingId), ct);
    public Task AddPlanAsync(Plan plan, CancellationToken ct) => db.Plans.AddAsync(plan, ct).AsTask();
    public async Task<Subscription?> GetSubscriptionAsync(Guid tenantId, bool tracked, CancellationToken ct)
    {
        var query = db.Subscriptions.IgnoreQueryFilters().Where(x => x.TenantId == tenantId);
        return await (tracked ? query : query.AsNoTracking()).SingleOrDefaultAsync(ct);
    }
    public Task AddSubscriptionAsync(Subscription subscription, CancellationToken ct) => db.Subscriptions.AddAsync(subscription, ct).AsTask();
    public Task<int> CountUsersAsync(Guid tenantId, CancellationToken ct) => db.Users.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantId && x.IsActive, ct);
    public Task<int> CountCustomersAsync(Guid tenantId, CancellationToken ct) => db.Customers.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantId && x.IsActive, ct);
    public Task<int> CountServiceOrdersAsync(Guid tenantId, DateTimeOffset periodStart, CancellationToken ct) => db.ServiceOrders.IgnoreQueryFilters().CountAsync(x => x.TenantId == tenantId && x.CreatedAt >= periodStart, ct);
    public Task<bool> WebhookExistsAsync(string gateway, string eventId, CancellationToken ct) => db.PaymentWebhookEvents.AnyAsync(x => x.Gateway == gateway && x.ExternalEventId == eventId, ct);
    public Task AddWebhookAsync(PaymentWebhookEvent webhook, CancellationToken ct) => db.PaymentWebhookEvents.AddAsync(webhook, ct).AsTask();
    public async Task<IReadOnlyCollection<BillingInvoice>> ListInvoicesAsync(Guid tenantId, CancellationToken ct) => await db.BillingInvoices.AsNoTracking().Where(x => x.TenantId == tenantId).OrderByDescending(x => x.DueAt).ToListAsync(ct);
    public Task<BillingInvoice?> GetInvoiceByGatewayIdAsync(string gatewayInvoiceId, CancellationToken ct) => db.BillingInvoices.SingleOrDefaultAsync(x => x.GatewayInvoiceId == gatewayInvoiceId, ct);
    public Task AddInvoiceAsync(BillingInvoice invoice, CancellationToken ct) => db.BillingInvoices.AddAsync(invoice, ct).AsTask();
}
