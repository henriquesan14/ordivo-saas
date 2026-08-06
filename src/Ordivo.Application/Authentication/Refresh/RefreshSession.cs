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
    IPlatformTenantRepository tenants,
    IGenerateToken accessTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<RefreshSessionCommand, AuthDto>
{
    public async Task<Result<AuthDto>> Handle(RefreshSessionCommand command, CancellationToken ct)
    {
        var session = await sessions.GetByTokenHashAsync(refreshTokenGenerator.Hash(command.RefreshToken), ct);
        var now = timeProvider.GetUtcNow();
        if (session is null || session.SubjectType != AuthSubjectType.TenantUser)
            return Result.Failure<AuthDto>(new Error("unauthorized", "Invalid or expired refresh token."));
        if (!session.IsActive(now))
        {
            if (session.RevokedAt is not null && session.ReplacedBySessionId is not null)
            {
                foreach (var related in await sessions.ListByFamilyAsync(session.FamilyId, ct)) related.Revoke(now);
                await unitOfWork.SaveChangesAsync(ct);
            }
            return Result.Failure<AuthDto>(new Error("unauthorized", "Invalid or expired refresh token."));
        }

        var user = await users.GetByIdAsync(session.UserId, ct);
        if (user is null || !user.IsActive || user.TenantId != session.TenantId)
            return Result.Failure<AuthDto>(new Error("unauthorized", "User is no longer active."));
        var tenant = await tenants.GetAsync(user.TenantId, ct);
        if (tenant is null || !tenant.IsActive)
        {
            session.Revoke(now);
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<AuthDto>(Error.Forbidden("Tenant is suspended."));
        }

        var refreshToken = refreshTokenGenerator.Generate();
        var replacement = AuthSession.CreateReplacement(session, refreshToken.Hash, refreshToken.ExpiresAt);
        session.Rotate(replacement.Id, now);
        await sessions.AddAsync(replacement, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(user.ToAuthDto(accessTokenGenerator.GenerateToken(user), refreshToken));
    }
}
