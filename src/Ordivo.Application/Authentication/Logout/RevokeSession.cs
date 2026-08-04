using FluentValidation;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Authentication.Logout;

public sealed record RevokeSessionCommand(string RefreshToken) : ICommand<bool>;

public sealed class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionCommandValidator() => RuleFor(command => command.RefreshToken).NotEmpty();
}

public sealed class RevokeSessionCommandHandler(
    IAuthSessionRepository sessions,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<RevokeSessionCommand, bool>
{
    public async Task<Result<bool>> Handle(RevokeSessionCommand command, CancellationToken ct)
    {
        var session = await sessions.GetByTokenHashAsync(refreshTokenGenerator.Hash(command.RefreshToken), ct);
        if (session is null) return Result.Success(false);
        session.Revoke(timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}
