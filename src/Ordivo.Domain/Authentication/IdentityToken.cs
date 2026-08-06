using Ordivo.SharedKernel.Domain;

namespace Ordivo.Domain.Authentication;

public enum IdentityTokenType { EmailVerification, PasswordReset, UserInvitation }

public sealed class IdentityToken : AggregateRoot<Guid>
{
    private IdentityToken(Guid id) : base(id) { }

    public static IdentityToken Create(
        Guid userId,
        Guid tenantId,
        string email,
        IdentityTokenType type,
        string tokenHash,
        DateTimeOffset expiresAt)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User is required.", nameof(userId));
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ArgumentException("Token hash is required.", nameof(tokenHash));
        return new IdentityToken(Guid.NewGuid())
        {
            UserId = userId,
            TenantId = tenantId,
            Email = email.Trim().ToLowerInvariant(),
            Type = type,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt
        };
    }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public IdentityTokenType Type { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? ConsumedAt { get; private set; }
    public bool IsValid(DateTimeOffset now) => ConsumedAt is null && ExpiresAt > now;
    public void Consume(DateTimeOffset now)
    {
        if (!IsValid(now)) throw new InvalidOperationException("Identity token is no longer valid.");
        ConsumedAt = now;
    }
}
