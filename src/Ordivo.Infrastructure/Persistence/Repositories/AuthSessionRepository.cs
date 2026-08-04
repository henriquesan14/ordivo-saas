using Microsoft.EntityFrameworkCore;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Authentication;

namespace Ordivo.Infrastructure.Persistence.Repositories;

internal sealed class AuthSessionRepository(OrdivoDbContext dbContext) : IAuthSessionRepository
{
    public Task<AuthSession?> GetByTokenHashAsync(string tokenHash, CancellationToken ct) =>
        dbContext.AuthSessions.SingleOrDefaultAsync(session => session.TokenHash == tokenHash, ct);

    public Task<AuthSession?> GetByIdAsync(Guid id, CancellationToken ct) =>
        dbContext.AuthSessions.SingleOrDefaultAsync(session => session.Id == id, ct);

    public async Task<IReadOnlyCollection<AuthSession>> ListByUserAsync(
        Guid userId,
        AuthSubjectType subjectType,
        CancellationToken ct) =>
        await dbContext.AuthSessions.AsNoTracking()
            .Where(session => session.UserId == userId && session.SubjectType == subjectType)
            .OrderByDescending(session => session.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(AuthSession session, CancellationToken ct) =>
        await dbContext.AuthSessions.AddAsync(session, ct);
}
