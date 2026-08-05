using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Users.GetUser;

public sealed record GetUserQuery(Guid Id) : IQuery<UserDto>;

public sealed class GetUserQueryHandler(IUserRepository users, IUserContext userContext)
    : IQueryHandler<GetUserQuery, UserDto>
{
    public async Task<Result<UserDto>> Handle(GetUserQuery query, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(query.Id, ct);
        return user is null || user.TenantId != userContext.TenantId
            ? Result.Failure<UserDto>(Error.NotFound("User not found."))
            : Result.Success(user.ToDto());
    }
}
