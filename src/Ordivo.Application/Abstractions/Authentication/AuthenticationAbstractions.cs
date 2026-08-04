using Ordivo.Domain.Users;
using Ordivo.Domain.PlatformUsers;

namespace Ordivo.Application.Abstractions.Authentication;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string passwordHash, string password);
}

public interface IGenerateToken
{
    AccessToken GenerateToken(User user);
    AccessToken GenerateToken(PlatformUser user);
}

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

public interface IRefreshTokenGenerator
{
    RefreshToken Generate();
    string Hash(string token);
}

public sealed record RefreshToken(string Token, string Hash, DateTimeOffset ExpiresAt);

public interface IUserContext
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    Guid TenantId { get; }
    string? Name { get; }
    string? Email { get; }
    string? Role { get; }
    string? PlatformRole { get; }
}
