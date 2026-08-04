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

namespace Ordivo.Infrastructure.Persistence;

public sealed class OrdivoDbContext(DbContextOptions<OrdivoDbContext> options, IUserContext userContext) : DbContext(options), IUnitOfWork
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<PlatformUser> PlatformUsers => Set<PlatformUser>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();
    public Guid CurrentTenantId => userContext.IsAuthenticated ? userContext.TenantId : Guid.Empty;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdivoDbContext).Assembly);
        modelBuilder.Entity<Customer>().HasQueryFilter(customer => customer.TenantId == CurrentTenantId);
        modelBuilder.Entity<ServiceOrder>().HasQueryFilter(order => order.TenantId == CurrentTenantId);
        modelBuilder.Entity<User>().HasQueryFilter(user => user.TenantId == CurrentTenantId);
        modelBuilder.Entity<Tenant>().HasQueryFilter(tenant => tenant.Id == CurrentTenantId);
    }

}
