using Ordivo.Domain.Customers;
using Ordivo.Domain.ServiceOrders;
using Ordivo.Domain.Users;
using Ordivo.Domain.Tenants;
using Ordivo.Domain.PlatformUsers;
using Ordivo.Domain.Authentication;
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
    Task<IReadOnlyCollection<User>> ListAsync(CancellationToken ct);
    Task<int> CountActiveOwnersAsync(CancellationToken ct);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct);
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
}
public interface ITenantRepository
{
    Task<Tenant?> GetAsync(Guid id, CancellationToken ct);
    Task AddAsync(Tenant tenant, CancellationToken ct);
}
public interface IPlatformUserRepository
{
    Task<PlatformUser?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<PlatformUser?> GetByEmailAsync(string normalizedEmail, CancellationToken ct);
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct);
    Task AddAsync(PlatformUser user, CancellationToken ct);
}
public interface IAuthSessionRepository
{
    Task<AuthSession?> GetByTokenHashAsync(string tokenHash, CancellationToken ct);
    Task<AuthSession?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyCollection<AuthSession>> ListByUserAsync(Guid userId, AuthSubjectType subjectType, CancellationToken ct);
    Task AddAsync(AuthSession session, CancellationToken ct);
}
public interface IPlatformTenantRepository
{
    Task<IReadOnlyCollection<Tenant>> ListAsync(CancellationToken ct);
    Task<Tenant?> GetAsync(Guid id, CancellationToken ct);
}
