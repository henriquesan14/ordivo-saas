using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.PlatformUsers;
using Ordivo.Domain.Authentication;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Platform.Authentication.Login;

public sealed record PlatformLoginCommand(string Email, string Password) : ICommand<PlatformAuthDto>;

public sealed class PlatformLoginCommandHandler(
    IPlatformUserRepository users,
    IPasswordHasher passwordHasher,
    IGenerateToken tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IAuthSessionRepository sessions,
    IUnitOfWork unitOfWork) : ICommandHandler<PlatformLoginCommand, PlatformAuthDto>
{
    public async Task<Result<PlatformAuthDto>> Handle(PlatformLoginCommand command, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(PlatformUser.NormalizeEmail(command.Email), ct);
        if (user is null || !user.IsActive || !passwordHasher.Verify(user.PasswordHash, command.Password))
            return Result.Failure<PlatformAuthDto>(new Error("unauthorized", "Invalid email or password."));

        var refreshToken = refreshTokenGenerator.Generate();
        await sessions.AddAsync(AuthSession.Create(
            user.Id, null, AuthSubjectType.PlatformUser, refreshToken.Hash, refreshToken.ExpiresAt), ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(user.ToAuthDto(tokenGenerator.GenerateToken(user), refreshToken));
    }
}
