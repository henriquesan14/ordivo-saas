using FluentValidation;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Users;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Users.CreateUser;

public sealed record CreateUserCommand(string Name, string Email, string Password, UserRole Role) : ICommand<UserDto>;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(120);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(command => command.Role).IsInEnum();
    }
}

public sealed class CreateUserCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IUserContext userContext,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateUserCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(CreateUserCommand command, CancellationToken ct)
    {
        if (command.Role == UserRole.Owner && userContext.Role != UserRole.Owner.ToString())
            return Result.Failure<UserDto>(Error.Forbidden("Only an Owner can create another Owner."));

        var email = User.NormalizeEmail(command.Email);
        if (await users.EmailExistsAsync(email, ct))
            return Result.Failure<UserDto>(Error.Conflict("A user with this email already exists."));

        var user = User.Create(userContext.TenantId, command.Name, email, passwordHasher.Hash(command.Password), command.Role);
        await users.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(user.ToDto());
    }
}
