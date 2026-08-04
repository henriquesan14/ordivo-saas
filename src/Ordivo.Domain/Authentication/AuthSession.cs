using Ordivo.SharedKernel.Domain;

namespace Ordivo.Domain.Authentication;

public enum AuthSubjectType { TenantUser, PlatformUser }

public sealed class AuthSession : AggregateRoot<Guid>
{
    private AuthSession(Guid id) : base(id) { }

    public static AuthSession Create(
        Guid userId,
        Guid? tenantId,
        AuthSubjectType subjectType,
        string tokenHash,
        DateTimeOffset expiresAt)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User is required.", nameof(userId));
        if (subjectType == AuthSubjectType.TenantUser && (!tenantId.HasValue || tenantId.Value == Guid.Empty))
            throw new ArgumentException("Tenant is required for a tenant user.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ArgumentException("Token hash is required.", nameof(tokenHash));

        return new AuthSession(Guid.NewGuid())
        {
            UserId = userId,
            TenantId = tenantId,
            SubjectType = subjectType,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            Version = Guid.NewGuid()
        };
    }

    public Guid UserId { get; private set; }
    public Guid? TenantId { get; private set; }
    public AuthSubjectType SubjectType { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? ReplacedBySessionId { get; private set; }
    public Guid Version { get; private set; }

    public bool IsActive(DateTimeOffset now) => RevokedAt is null && ExpiresAt > now;

    public void Rotate(Guid replacementSessionId, DateTimeOffset now)
    {
        if (!IsActive(now)) throw new InvalidOperationException("Session is no longer active.");
        RevokedAt = now;
        ReplacedBySessionId = replacementSessionId;
        Version = Guid.NewGuid();
    }

    public void Revoke(DateTimeOffset now)
    {
        if (RevokedAt is not null) return;
        RevokedAt = now;
        Version = Guid.NewGuid();
    }
}
