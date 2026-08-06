using Microsoft.EntityFrameworkCore;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Customers;
using Ordivo.Domain.ServiceOrders;
using Ordivo.Domain.Users;
using Ordivo.Domain.Tenants;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Domain.PlatformUsers;
using Ordivo.SharedKernel.Domain;
using Ordivo.Domain.Authentication;
using Ordivo.Domain.Commercial;
using Ordivo.Domain.Impersonation;

namespace Ordivo.Infrastructure.Persistence;

public sealed class OrdivoDbContext(DbContextOptions<OrdivoDbContext> options, IUserContext userContext) : DbContext(options), IUnitOfWork
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<PlatformUser> PlatformUsers => Set<PlatformUser>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();
    public DbSet<IdentityToken> IdentityTokens => Set<IdentityToken>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<BillingInvoice> BillingInvoices => Set<BillingInvoice>();
    public DbSet<PaymentWebhookEvent> PaymentWebhookEvents => Set<PaymentWebhookEvent>();
    public DbSet<ImpersonationSession> ImpersonationSessions => Set<ImpersonationSession>();
    public Guid CurrentTenantId => userContext.IsAuthenticated ? userContext.TenantId : Guid.Empty;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdivoDbContext).Assembly);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(type =>
                     type.ClrType.BaseType is not null && IsAggregate(type.ClrType)))
            modelBuilder.Entity(entityType.ClrType).Property<Guid>("Version").IsConcurrencyToken();
        modelBuilder.Entity<Customer>().HasQueryFilter(customer => customer.TenantId == CurrentTenantId);
        modelBuilder.Entity<ServiceOrder>().HasQueryFilter(order => order.TenantId == CurrentTenantId);
        modelBuilder.Entity<User>().HasQueryFilter(user => user.TenantId == CurrentTenantId);
        modelBuilder.Entity<Tenant>().HasQueryFilter(tenant => tenant.Id == CurrentTenantId);
    }

    private static bool IsAggregate(Type type) =>
        type != typeof(object) && (type.BaseType?.IsGenericType == true && type.BaseType.GetGenericTypeDefinition() == typeof(AggregateRoot<>) || IsAggregate(type.BaseType!));

}
