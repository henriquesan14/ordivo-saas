using FluentValidation;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Authentication;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Authentication.Refresh;

public sealed record RefreshSessionCommand(string RefreshToken) : ICommand<AuthDto>;

public sealed class RefreshSessionCommandValidator : AbstractValidator<RefreshSessionCommand>
{
    public RefreshSessionCommandValidator() => RuleFor(command => command.RefreshToken).NotEmpty();
}

public sealed class RefreshSessionCommandHandler(
    IAuthSessionRepository sessions,
    IUserRepository users,
    IGenerateToken accessTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<RefreshSessionCommand, AuthDto>
{
    public async Task<Result<AuthDto>> Handle(RefreshSessionCommand command, CancellationToken ct)
    {
        var session = await sessions.GetByTokenHashAsync(refreshTokenGenerator.Hash(command.RefreshToken), ct);
        var now = timeProvider.GetUtcNow();
        if (session is null || session.SubjectType != AuthSubjectType.TenantUser || !session.IsActive(now))
            return Result.Failure<AuthDto>(new Error("unauthorized", "Invalid or expired refresh token."));

        var user = await users.GetByIdAsync(session.UserId, ct);
        if (user is null || !user.IsActive || user.TenantId != session.TenantId)
            return Result.Failure<AuthDto>(new Error("unauthorized", "User is no longer active."));

        var refreshToken = refreshTokenGenerator.Generate();
        var replacement = AuthSession.Create(
            user.Id, user.TenantId, AuthSubjectType.TenantUser, refreshToken.Hash, refreshToken.ExpiresAt);
        session.Rotate(replacement.Id, now);
        await sessions.AddAsync(replacement, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(user.ToAuthDto(accessTokenGenerator.GenerateToken(user), refreshToken));
    }
}
