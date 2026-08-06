using FluentValidation;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Authentication;
using Ordivo.Domain.Users;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Users.InviteUser;

public sealed record InviteUserCommand(string Name, string Email, UserRole Role) : ICommand<UserDto>;
public sealed class InviteUserCommandValidator : AbstractValidator<InviteUserCommand>
{
    public InviteUserCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(120);
        RuleFor(command => command.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(command => command.Role).IsInEnum();
    }
}

public sealed class InviteUserCommandHandler(
    IUserRepository users,
    IUserContext userContext,
    IPasswordHasher passwordHasher,
    IIdentityTokenGenerator tokenGenerator,
    IIdentityTokenRepository tokens,
    IIdentityEmailSender emailSender,
    IUnitOfWork unitOfWork) : ICommandHandler<InviteUserCommand, UserDto>
{
    public async Task<Result<UserDto>> Handle(InviteUserCommand command, CancellationToken ct)
    {
        if (command.Role == UserRole.Owner && userContext.Role != UserRole.Owner.ToString())
            return Result.Failure<UserDto>(Error.Forbidden("Only an Owner can invite another Owner."));
        var email = User.NormalizeEmail(command.Email);
        if (await users.EmailExistsAsync(email, ct))
            return Result.Failure<UserDto>(Error.Conflict("A user with this email already exists."));
        var user = User.Create(userContext.TenantId, command.Name, email,
            passwordHasher.Hash(Guid.NewGuid().ToString("N")), command.Role);
        user.Deactivate();
        var generated = tokenGenerator.Generate(TimeSpan.FromDays(7));
        await users.AddAsync(user, ct);
        await tokens.AddAsync(IdentityToken.Create(user.Id, user.TenantId, user.Email,
            IdentityTokenType.UserInvitation, generated.Hash, generated.ExpiresAt), ct);
        await unitOfWork.SaveChangesAsync(ct);
        await emailSender.SendUserInvitationAsync(user.Email, user.Name, generated.Token, ct);
        return Result.Success(user.ToDto());
    }
}
