using Microsoft.EntityFrameworkCore;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Authentication;

namespace Ordivo.Infrastructure.Persistence.Repositories;

internal sealed class IdentityTokenRepository(OrdivoDbContext dbContext) : IIdentityTokenRepository
{
    public Task<IdentityToken?> GetByHashAsync(string tokenHash, IdentityTokenType type, CancellationToken ct) =>
        dbContext.IdentityTokens.SingleOrDefaultAsync(token => token.TokenHash == tokenHash && token.Type == type, ct);

    public async Task AddAsync(IdentityToken token, CancellationToken ct) =>
        await dbContext.IdentityTokens.AddAsync(token, ct);

    public async Task ConsumeActiveAsync(Guid userId, IdentityTokenType type, DateTimeOffset now, CancellationToken ct)
    {
        var activeTokens = await dbContext.IdentityTokens
            .Where(token => token.UserId == userId && token.Type == type && token.ConsumedAt == null && token.ExpiresAt > now)
            .ToListAsync(ct);
        foreach (var token in activeTokens) token.Consume(now);
    }
}
