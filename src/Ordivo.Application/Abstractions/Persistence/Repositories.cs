using Ordivo.Domain.Customers;
using Ordivo.Domain.ServiceOrders;
using Ordivo.Domain.Users;
using Ordivo.Domain.Tenants;
namespace Ordivo.Application.Abstractions.Persistence;
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
public interface ICustomerRepository
{
    Task<IReadOnlyCollection<Customer>> ListAsync(CancellationToken ct);
    Task<Customer?> GetAsync(Guid id, CancellationToken ct);
    Task<bool> DocumentExistsAsync(string document, CancellationToken ct);
    Task AddAsync(Customer customer, CancellationToken ct);
}
public interface IServiceOrderRepository
{
    Task<IReadOnlyCollection<ServiceOrder>> ListAsync(CancellationToken ct);
    Task<ServiceOrder?> GetAsync(Guid id, CancellationToken ct);
    Task AddAsync(ServiceOrder order, CancellationToken ct);
}
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct);
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
}
public interface ITenantRepository
{
    Task<Tenant?> GetAsync(Guid id, CancellationToken ct);
    Task AddAsync(Tenant tenant, CancellationToken ct);
}
