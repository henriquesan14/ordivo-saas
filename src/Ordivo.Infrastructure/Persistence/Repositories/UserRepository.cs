using Microsoft.EntityFrameworkCore;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Users;

namespace Ordivo.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(OrdivoDbContext dbContext) : IUserRepository
{
    public async Task AddAsync(User user, CancellationToken ct) => await dbContext.Users.AddAsync(user, ct);
    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct) =>
        dbContext.Users.IgnoreQueryFilters().SingleOrDefaultAsync(user => user.Email == normalizedEmail, ct);
    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct) =>
        dbContext.Users.IgnoreQueryFilters().AnyAsync(user => user.Email == normalizedEmail, ct);
}
