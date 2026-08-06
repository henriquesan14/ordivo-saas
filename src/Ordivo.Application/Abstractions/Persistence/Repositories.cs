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
    Task<(IReadOnlyCollection<Customer> Items, int TotalCount)> ListAsync(
        string? name, string? document, string? email, string? phone,
        bool includeInactive, int page, int pageSize, string sortBy, bool descending,
        CancellationToken ct);
    Task<Customer?> GetAsync(Guid id, CancellationToken ct);
    Task<bool> DocumentExistsAsync(string document, CancellationToken ct, Guid? excludingCustomerId = null);
    Task AddAsync(Customer customer, CancellationToken ct);
}
public interface IServiceOrderRepository
{
    Task<(IReadOnlyCollection<ServiceOrder> Items, int TotalCount)> ListAsync(
        string? search, ServiceOrderStatus? status, Guid? customerId, Guid? assignedUserId,
        DateTimeOffset? scheduledFrom, DateTimeOffset? scheduledTo,
        int page, int pageSize, string sortBy, bool descending, CancellationToken ct);
    Task<ServiceOrder?> GetAsync(Guid id, CancellationToken ct);
    Task AddAsync(ServiceOrder order, CancellationToken ct);
    Task<long> NextSequenceAsync(CancellationToken ct);
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
    Task<IReadOnlyCollection<AuthSession>> ListByFamilyAsync(Guid familyId, CancellationToken ct);
    Task<IReadOnlyCollection<AuthSession>> ListActiveByUserAsync(Guid userId, AuthSubjectType subjectType, CancellationToken ct);
    Task<IReadOnlyCollection<AuthSession>> ListActiveByTenantAsync(Guid tenantId, CancellationToken ct);
}
public interface IIdentityTokenRepository
{
    Task<IdentityToken?> GetByHashAsync(string tokenHash, IdentityTokenType type, CancellationToken ct);
    Task AddAsync(IdentityToken token, CancellationToken ct);
    Task ConsumeActiveAsync(Guid userId, IdentityTokenType type, DateTimeOffset now, CancellationToken ct);
}
public interface IPlatformTenantRepository
{
    Task<IReadOnlyCollection<Tenant>> ListAsync(CancellationToken ct);
    Task<Tenant?> GetAsync(Guid id, CancellationToken ct);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct);
}
