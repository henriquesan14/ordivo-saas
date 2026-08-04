using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Domain.Users;

namespace Ordivo.Application.Authentication;

public sealed record AuthDto(
    Guid UserId,
    Guid TenantId,
    string Name,
    string Email,
    UserRole Role,
    string AccessToken,
    DateTimeOffset ExpiresAt);

public static class AuthMappingExtensions
{
    public static AuthDto ToAuthDto(this User user, AccessToken token) =>
        new(user.Id, user.TenantId, user.Name, user.Email, user.Role, token.Token, token.ExpiresAt);
}
