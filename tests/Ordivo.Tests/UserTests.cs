using Ordivo.Domain.Users;

namespace Ordivo.Tests;

public sealed class UserTests
{
    [Fact]
    public void Create_normalizes_email_and_raises_event()
    {
        var tenantId = Guid.NewGuid();
        var user = User.Create(tenantId, "Rico", " RICO@EXAMPLE.COM ", "hashed-password");

        Assert.Equal("rico@example.com", user.Email);
        Assert.Equal(tenantId, user.TenantId);
        Assert.Equal(UserRole.Owner, user.Role);
        Assert.True(user.IsActive);
        Assert.IsType<UserCreatedDomainEvent>(Assert.Single(user.DomainEvents));
    }

    [Fact]
    public void Deactivate_marks_user_as_inactive()
    {
        var user = User.Create(Guid.NewGuid(), "Rico", "rico@example.com", "hashed-password");

        user.Deactivate();

        Assert.False(user.IsActive);
    }

    [Fact]
    public void Administrative_changes_update_role_and_status()
    {
        var user = User.Create(Guid.NewGuid(), "Rico", "rico@example.com", "hashed-password");

        user.ChangeRole(UserRole.Admin);
        user.Deactivate();
        user.Activate();

        Assert.Equal(UserRole.Admin, user.Role);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void ChangePassword_replaces_password_hash()
    {
        var user = User.Create(Guid.NewGuid(), "Rico", "rico@example.com", "old-hash");

        user.ChangePassword("new-hash");

        Assert.Equal("new-hash", user.PasswordHash);
    }

    [Fact]
    public void VerifyEmail_marks_email_as_verified()
    {
        var now = DateTimeOffset.UtcNow;
        var user = User.Create(Guid.NewGuid(), "Rico", "rico@example.com", "hash");

        user.VerifyEmail(now);

        Assert.True(user.IsEmailVerified);
        Assert.Equal(now, user.EmailVerifiedAt);
    }
}
