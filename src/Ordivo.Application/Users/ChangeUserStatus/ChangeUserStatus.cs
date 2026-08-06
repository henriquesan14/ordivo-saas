using FluentValidation;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Users;
using Ordivo.Domain.Authentication;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Users.ChangeUserStatus;

public sealed record ChangeUserStatusCommand(Guid UserId, bool IsActive) : ICommand<UserDto>;

public sealed class ChangeUserStatusCommandValidator : AbstractValidator<ChangeUserStatusCommand>
{
    public ChangeUserStatusCommandValidator() => RuleFor(command => command.UserId).NotEmpty();
}

public sealed class ChangeUserStatusCommandHandler(
    IUserRepository users,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    IAuthSessionRepository sessions,
    TimeProvider timeProvider) : ICommandHandler<ChangeUserStatusCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(ChangeUserStatusCommand command, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(command.UserId, ct);
        if (user is null || user.TenantId != userContext.TenantId)
            return Result.Failure<UserDto>(Error.NotFound("User not found."));
        if (user.Id == userContext.UserId && !command.IsActive)
            return Result.Failure<UserDto>(Error.Conflict("You cannot deactivate your own user."));
        if (user.Role == UserRole.Owner && userContext.Role != UserRole.Owner.ToString())
            return Result.Failure<UserDto>(Error.Forbidden("Only an Owner can change another Owner's status."));
        if (!command.IsActive && user.Role == UserRole.Owner && await users.CountActiveOwnersAsync(ct) <= 1)
            return Result.Failure<UserDto>(Error.Conflict("The last active Owner cannot be deactivated."));

        if (command.IsActive) user.Activate(); else
        {
            user.Deactivate();
            var now = timeProvider.GetUtcNow();
            foreach (var session in await sessions.ListActiveByUserAsync(user.Id, AuthSubjectType.TenantUser, ct))
                session.Revoke(now);
        }
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(user.ToDto());
    }
}
