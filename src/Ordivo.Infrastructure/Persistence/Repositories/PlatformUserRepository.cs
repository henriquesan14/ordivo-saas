using Microsoft.EntityFrameworkCore;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.PlatformUsers;

namespace Ordivo.Infrastructure.Persistence.Repositories;

internal sealed class PlatformUserRepository(OrdivoDbContext dbContext) : IPlatformUserRepository
{
    public async Task AddAsync(PlatformUser user, CancellationToken ct) => await dbContext.PlatformUsers.AddAsync(user, ct);
    public Task<PlatformUser?> GetByEmailAsync(string normalizedEmail, CancellationToken ct) =>
        dbContext.PlatformUsers.SingleOrDefaultAsync(user => user.Email == normalizedEmail, ct);
    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct) =>
        dbContext.PlatformUsers.AnyAsync(user => user.Email == normalizedEmail, ct);
}
