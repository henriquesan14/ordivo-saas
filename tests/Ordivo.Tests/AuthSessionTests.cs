using Ordivo.Domain.Authentication;

namespace Ordivo.Tests;

public sealed class AuthSessionTests
{
    [Fact]
    public void Rotate_revokes_current_session_and_points_to_replacement()
    {
        var now = DateTimeOffset.UtcNow;
        var session = AuthSession.Create(Guid.NewGuid(), Guid.NewGuid(), AuthSubjectType.TenantUser, "hash", now.AddDays(1));
        var replacementId = Guid.NewGuid();

        session.Rotate(replacementId, now);

        Assert.Equal(now, session.RevokedAt);
        Assert.Equal(replacementId, session.ReplacedBySessionId);
        Assert.False(session.IsActive(now));
    }

    [Fact]
    public void Expired_session_is_not_active()
    {
        var now = DateTimeOffset.UtcNow;
        var session = AuthSession.Create(Guid.NewGuid(), null, AuthSubjectType.PlatformUser, "hash", now.AddMinutes(-1));

        Assert.False(session.IsActive(now));
    }

    [Fact]
    public void Tenant_session_requires_tenant()
    {
        Assert.Throws<ArgumentException>(() => AuthSession.Create(
            Guid.NewGuid(), null, AuthSubjectType.TenantUser, "hash", DateTimeOffset.UtcNow.AddDays(1)));
    }

    [Fact]
    public void Replacement_preserves_session_family()
    {
        var current = AuthSession.Create(Guid.NewGuid(), Guid.NewGuid(), AuthSubjectType.TenantUser,
            "first-hash", DateTimeOffset.UtcNow.AddDays(1));

        var replacement = AuthSession.CreateReplacement(current, "second-hash", DateTimeOffset.UtcNow.AddDays(1));

        Assert.Equal(current.FamilyId, replacement.FamilyId);
        Assert.NotEqual(current.Id, replacement.Id);
    }
}
