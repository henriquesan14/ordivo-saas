using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Domain.PlatformUsers;

namespace Ordivo.Application.Platform.Authentication;

public sealed record PlatformAuthDto(
    Guid UserId,
    string Name,
    string Email,
    PlatformRole Role,
    string AccessToken,
    DateTimeOffset ExpiresAt);

public static class PlatformAuthMappingExtensions
{
    public static PlatformAuthDto ToAuthDto(this PlatformUser user, AccessToken token) =>
        new(user.Id, user.Name, user.Email, user.Role, token.Token, token.ExpiresAt);
}
