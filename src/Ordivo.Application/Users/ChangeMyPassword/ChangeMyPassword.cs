using FluentValidation;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Users.ChangeMyPassword;

public sealed record ChangeMyPasswordCommand(string CurrentPassword, string NewPassword) : ICommand<bool>;

public sealed class ChangeMyPasswordCommandValidator : AbstractValidator<ChangeMyPasswordCommand>
{
    public ChangeMyPasswordCommandValidator()
    {
        RuleFor(command => command.CurrentPassword).NotEmpty().MaximumLength(128);
        RuleFor(command => command.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128)
            .NotEqual(command => command.CurrentPassword).WithMessage("The new password must be different from the current password.");
    }
}

public sealed class ChangeMyPasswordCommandHandler(
    IUserRepository users,
    IUserContext userContext,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : ICommandHandler<ChangeMyPasswordCommand, bool>
{
    public async Task<Result<bool>> Handle(ChangeMyPasswordCommand command, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(userContext.UserId, ct);
        if (user is null || user.TenantId != userContext.TenantId ||
            !passwordHasher.Verify(user.PasswordHash, command.CurrentPassword))
            return Result.Failure<bool>(new Error("unauthorized", "Current password is invalid."));

        user.ChangePassword(passwordHasher.Hash(command.NewPassword));
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}
