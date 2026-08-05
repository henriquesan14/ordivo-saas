using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Users.ListUsers;

public sealed record ListUsersQuery : IQuery<IReadOnlyCollection<UserDto>>;

public sealed class ListUsersQueryHandler(IUserRepository users)
    : IQueryHandler<ListUsersQuery, IReadOnlyCollection<UserDto>>
{
    public async Task<Result<IReadOnlyCollection<UserDto>>> Handle(ListUsersQuery query, CancellationToken ct) =>
        Result.Success((await users.ListAsync(ct)).ToListDto());
}
