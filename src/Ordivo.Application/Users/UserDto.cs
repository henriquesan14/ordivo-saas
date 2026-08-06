using Ordivo.Domain.Users;

namespace Ordivo.Application.Users;

public sealed record UserDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Email,
    UserRole Role,
    bool IsActive,
    bool IsEmailVerified,
    DateTimeOffset? EmailVerifiedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user) =>
        new(user.Id, user.TenantId, user.Name, user.Email, user.Role, user.IsActive,
            user.IsEmailVerified, user.EmailVerifiedAt, user.CreatedAt, user.UpdatedAt);

    public static IReadOnlyCollection<UserDto> ToListDto(this IEnumerable<User> users) =>
        [.. users.Select(user => user.ToDto())];
}
