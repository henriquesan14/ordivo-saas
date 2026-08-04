using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Users;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Authentication.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthDto>;

public sealed class LoginCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IGenerateToken tokenGenerator) : ICommandHandler<LoginCommand, AuthDto>
{
    public async Task<Result<AuthDto>> Handle(LoginCommand command, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(User.NormalizeEmail(command.Email), ct);
        if (user is null || !user.IsActive || !passwordHasher.Verify(user.PasswordHash, command.Password))
            return Result.Failure<AuthDto>(new Error("unauthorized", "Invalid email or password."));

        return Result.Success(user.ToAuthDto(tokenGenerator.GenerateToken(user)));
    }
}
