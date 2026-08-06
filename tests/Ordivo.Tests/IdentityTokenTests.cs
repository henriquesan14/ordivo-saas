using Ordivo.Domain.Authentication;

namespace Ordivo.Tests;

public sealed class IdentityTokenTests
{
    [Fact]
    public void Consume_makes_token_invalid()
    {
        var now = DateTimeOffset.UtcNow;
        var token = IdentityToken.Create(Guid.NewGuid(), Guid.NewGuid(), "user@example.com",
            IdentityTokenType.PasswordReset, "hash", now.AddHours(1));

        token.Consume(now);

        Assert.Equal(now, token.ConsumedAt);
        Assert.False(token.IsValid(now));
    }

    [Fact]
    public void Expired_token_is_invalid()
    {
        var now = DateTimeOffset.UtcNow;
        var token = IdentityToken.Create(Guid.NewGuid(), Guid.NewGuid(), "user@example.com",
            IdentityTokenType.EmailVerification, "hash", now.AddMinutes(-1));

        Assert.False(token.IsValid(now));
    }
}
