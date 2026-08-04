using FluentValidation;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Authentication;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Authentication.Sessions;

public sealed record AuthSessionDto(
    Guid Id,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt,
    Guid? ReplacedBySessionId);

public sealed record ListAuthSessionsQuery : IQuery<IReadOnlyCollection<AuthSessionDto>>;

public sealed class ListAuthSessionsQueryHandler(
    IAuthSessionRepository sessions,
    IUserContext userContext)
    : IQueryHandler<ListAuthSessionsQuery, IReadOnlyCollection<AuthSessionDto>>
{
    public async Task<Result<IReadOnlyCollection<AuthSessionDto>>> Handle(
        ListAuthSessionsQuery query,
        CancellationToken ct)
    {
        var subjectType = string.IsNullOrWhiteSpace(userContext.PlatformRole)
            ? AuthSubjectType.TenantUser
            : AuthSubjectType.PlatformUser;
        var items = await sessions.ListByUserAsync(userContext.UserId, subjectType, ct);
        return Result.Success<IReadOnlyCollection<AuthSessionDto>>([.. items.Select(session => new AuthSessionDto(
            session.Id, session.CreatedAt, session.ExpiresAt, session.RevokedAt, session.ReplacedBySessionId))]);
    }
}

public sealed record RevokeSessionByIdCommand(Guid SessionId) : ICommand<bool>;

public sealed class RevokeSessionByIdCommandValidator : AbstractValidator<RevokeSessionByIdCommand>
{
    public RevokeSessionByIdCommandValidator() => RuleFor(command => command.SessionId).NotEmpty();
}

public sealed class RevokeSessionByIdCommandHandler(
    IAuthSessionRepository sessions,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<RevokeSessionByIdCommand, bool>
{
    public async Task<Result<bool>> Handle(RevokeSessionByIdCommand command, CancellationToken ct)
    {
        var session = await sessions.GetByIdAsync(command.SessionId, ct);
        var expectedType = string.IsNullOrWhiteSpace(userContext.PlatformRole)
            ? AuthSubjectType.TenantUser
            : AuthSubjectType.PlatformUser;
        if (session is null || session.UserId != userContext.UserId || session.SubjectType != expectedType)
            return Result.Failure<bool>(Error.NotFound("Session not found."));

        session.Revoke(timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(true);
    }
}
