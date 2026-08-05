using FluentValidation;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Users;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Users.ChangeUserRole;

public sealed record ChangeUserRoleCommand(Guid UserId, UserRole Role) : ICommand<UserDto>;

public sealed class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Role).IsInEnum();
    }
}

public sealed class ChangeUserRoleCommandHandler(
    IUserRepository users,
    IUserContext userContext,
    IUnitOfWork unitOfWork) : ICommandHandler<ChangeUserRoleCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(ChangeUserRoleCommand command, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(command.UserId, ct);
        if (user is null || user.TenantId != userContext.TenantId)
            return Result.Failure<UserDto>(Error.NotFound("User not found."));

        if (user.Role == UserRole.Owner && command.Role != UserRole.Owner &&
            await users.CountActiveOwnersAsync(ct) <= 1)
            return Result.Failure<UserDto>(Error.Conflict("The last active Owner cannot be demoted."));

        user.ChangeRole(command.Role);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(user.ToDto());
    }
}
