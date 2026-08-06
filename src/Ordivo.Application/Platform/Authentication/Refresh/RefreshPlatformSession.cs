using FluentValidation;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Authentication;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Platform.Authentication.Refresh;

public sealed record RefreshPlatformSessionCommand(string RefreshToken) : ICommand<PlatformAuthDto>;

public sealed class RefreshPlatformSessionCommandValidator : AbstractValidator<RefreshPlatformSessionCommand>
{
    public RefreshPlatformSessionCommandValidator() => RuleFor(command => command.RefreshToken).NotEmpty();
}

public sealed class RefreshPlatformSessionCommandHandler(
    IAuthSessionRepository sessions,
    IPlatformUserRepository users,
    IGenerateToken accessTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<RefreshPlatformSessionCommand, PlatformAuthDto>
{
    public async Task<Result<PlatformAuthDto>> Handle(RefreshPlatformSessionCommand command, CancellationToken ct)
    {
        var session = await sessions.GetByTokenHashAsync(refreshTokenGenerator.Hash(command.RefreshToken), ct);
        var now = timeProvider.GetUtcNow();
        if (session is null || session.SubjectType != AuthSubjectType.PlatformUser)
            return Result.Failure<PlatformAuthDto>(new Error("unauthorized", "Invalid or expired refresh token."));
        if (!session.IsActive(now))
        {
            if (session.RevokedAt is not null && session.ReplacedBySessionId is not null)
            {
                foreach (var related in await sessions.ListByFamilyAsync(session.FamilyId, ct)) related.Revoke(now);
                await unitOfWork.SaveChangesAsync(ct);
            }
            return Result.Failure<PlatformAuthDto>(new Error("unauthorized", "Invalid or expired refresh token."));
        }

        var user = await users.GetByIdAsync(session.UserId, ct);
        if (user is null || !user.IsActive)
            return Result.Failure<PlatformAuthDto>(new Error("unauthorized", "User is no longer active."));

        var refreshToken = refreshTokenGenerator.Generate();
        var replacement = AuthSession.CreateReplacement(session, refreshToken.Hash, refreshToken.ExpiresAt);
        session.Rotate(replacement.Id, now);
        await sessions.AddAsync(replacement, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(user.ToAuthDto(accessTokenGenerator.GenerateToken(user), refreshToken));
    }
}
