using Ordivo.Domain.Tenants;

namespace Ordivo.Application.Platform.Tenants;

public sealed record PlatformTenantDto(Guid Id, string Name, bool IsActive, DateTimeOffset CreatedAt);

public static class PlatformTenantMappingExtensions
{
    public static PlatformTenantDto ToPlatformDto(this Tenant tenant) =>
        new(tenant.Id, tenant.Name, tenant.IsActive, tenant.CreatedAt);

    public static IReadOnlyCollection<PlatformTenantDto> ToPlatformListDto(this IEnumerable<Tenant> tenants) =>
        [.. tenants.Select(tenant => tenant.ToPlatformDto())];
}
