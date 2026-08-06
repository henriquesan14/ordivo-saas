using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordivo.Domain.Commercial;
using Ordivo.Infrastructure.Persistence;

namespace Ordivo.Infrastructure.Payments;

public static class CommercialSeeder
{
    public static async Task SeedDefaultPlanAsync(this IServiceProvider services, IConfiguration configuration, CancellationToken ct = default)
    {
        if (!configuration.GetValue("Commercial:SeedDefaultPlan", true)) return;
        await using var scope = services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<OrdivoDbContext>();
        if (await db.Plans.AnyAsync(ct)) return;
        var section = configuration.GetSection("Commercial:DefaultPlan");
        await db.Plans.AddAsync(Plan.Create(section["Name"] ?? "Pro", section["Code"] ?? "pro", section.GetValue("Price", 99.90m), section["Currency"] ?? "BRL", BillingInterval.Monthly, section.GetValue("TrialDays", 14), section.GetValue("MaxUsers", 10), section.GetValue("MaxCustomers", 500), section.GetValue("MaxServiceOrders", 200)), ct);
        await db.SaveChangesAsync(ct);
    }
}
