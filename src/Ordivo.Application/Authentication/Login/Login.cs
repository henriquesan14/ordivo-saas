using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Users;
using Ordivo.Domain.Authentication;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Authentication.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthDto>;

public sealed class LoginCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IGenerateToken tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IAuthSessionRepository sessions,
    IPlatformTenantRepository tenants,
    IUnitOfWork unitOfWork) : ICommandHandler<LoginCommand, AuthDto>
{
    public async Task<Result<AuthDto>> Handle(LoginCommand command, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(User.NormalizeEmail(command.Email), ct);
        if (user is null || !user.IsActive || !passwordHasher.Verify(user.PasswordHash, command.Password))
            return Result.Failure<AuthDto>(new Error("unauthorized", "Invalid email or password."));
        if (!user.IsEmailVerified)
            return Result.Failure<AuthDto>(Error.Forbidden("Email verification is required before login."));
        var tenant = await tenants.GetAsync(user.TenantId, ct);
        if (tenant is null || !tenant.IsActive)
            return Result.Failure<AuthDto>(Error.Forbidden("Tenant is suspended."));

        var refreshToken = refreshTokenGenerator.Generate();
        await sessions.AddAsync(AuthSession.Create(
            user.Id, user.TenantId, AuthSubjectType.TenantUser, refreshToken.Hash, refreshToken.ExpiresAt), ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(user.ToAuthDto(tokenGenerator.GenerateToken(user), refreshToken));
    }
}
