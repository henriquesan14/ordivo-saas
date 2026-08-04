using Ordivo.SharedKernel.Domain;

namespace Ordivo.Domain.PlatformUsers;

public enum PlatformRole { Support, PlatformAdmin }

public sealed class PlatformUser : AggregateRoot<Guid>
{
    private PlatformUser(Guid id) : base(id) { }

    public static PlatformUser Create(string name, string email, string passwordHash, PlatformRole role)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        return new PlatformUser(Guid.NewGuid())
        {
            Name = name.Trim(),
            Email = NormalizeEmail(email),
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true
        };
    }

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public PlatformRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public void Deactivate() => IsActive = false;
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
