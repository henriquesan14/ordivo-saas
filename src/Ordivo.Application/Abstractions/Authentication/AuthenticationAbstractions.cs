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
    AccessToken GenerateImpersonationToken(User user, Guid platformUserId, string platformUserName, Guid sessionId, DateTimeOffset expiresAt);
}

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

public interface IRefreshTokenGenerator
{
    RefreshToken Generate();
    string Hash(string token);
}

public sealed record RefreshToken(string Token, string Hash, DateTimeOffset ExpiresAt);

public interface IIdentityTokenGenerator
{
    GeneratedIdentityToken Generate(TimeSpan lifetime);
    string Hash(string token);
}

public sealed record GeneratedIdentityToken(string Token, string Hash, DateTimeOffset ExpiresAt);

public interface IIdentityEmailSender
{
    Task SendEmailVerificationAsync(string email, string name, string token, CancellationToken ct);
    Task SendPasswordResetAsync(string email, string name, string token, CancellationToken ct);
    Task SendUserInvitationAsync(string email, string name, string token, CancellationToken ct);
}

public interface IUserContext
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    Guid TenantId { get; }
    string? Name { get; }
    string? Email { get; }
    string? Role { get; }
    string? PlatformRole { get; }
    bool IsImpersonating { get; }
    Guid? ImpersonatorUserId { get; }
    Guid? ImpersonationSessionId { get; }
    string? ImpersonatorName { get; }
}
