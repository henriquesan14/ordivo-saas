using Ordivo.SharedKernel.Domain;

namespace Ordivo.Domain.Users;

public enum UserRole { Owner, Admin, Member }

public sealed class User : AggregateRoot<Guid>, ITenantEntity
{
    private User(Guid id) : base(id) { }

    public static User Create(Guid tenantId, string name, string email, string passwordHash, UserRole role = UserRole.Owner)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        var user = new User(Guid.NewGuid())
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Email = NormalizeEmail(email),
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true
        };
        user.Raise(new UserCreatedDomainEvent(user.Id, user.Email, user.CreatedAt));
        return user;
    }

    public string Name { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    public void ChangePassword(string passwordHash) =>
        PasswordHash = string.IsNullOrWhiteSpace(passwordHash)
            ? throw new ArgumentException("Password hash is required.", nameof(passwordHash))
            : passwordHash;

    public void ChangeRole(UserRole role) => Role = role;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}

public sealed record UserCreatedDomainEvent(Guid UserId, string Email, DateTimeOffset OccurredAt) : IDomainEvent;
