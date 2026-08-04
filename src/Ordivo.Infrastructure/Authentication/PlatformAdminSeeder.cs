using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Domain.PlatformUsers;
using Ordivo.Infrastructure.Persistence;

namespace Ordivo.Infrastructure.Authentication;

public static class PlatformAdminSeeder
{
    public static async Task SeedPlatformAdminAsync(this IServiceProvider services, IConfiguration configuration, CancellationToken ct = default)
    {
        var email = configuration["PlatformAdmin:Email"];
        var password = configuration["PlatformAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
        if (password.Length < 12) throw new InvalidOperationException("The platform admin seed password must have at least 12 characters.");

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrdivoDbContext>();
        var normalizedEmail = PlatformUser.NormalizeEmail(email);
        if (await dbContext.PlatformUsers.AnyAsync(user => user.Email == normalizedEmail, ct)) return;

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var name = configuration["PlatformAdmin:Name"] ?? "Platform Admin";
        await dbContext.PlatformUsers.AddAsync(
            PlatformUser.Create(name, normalizedEmail, hasher.Hash(password), PlatformRole.PlatformAdmin), ct);
        await dbContext.SaveChangesAsync(ct);
    }
}
