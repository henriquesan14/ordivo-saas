using Ordivo.Domain.Tenants;

namespace Ordivo.Application.Tenants;

public static class TenantMappingExtensions
{
    public static TenantDto ToDto(this Tenant tenant) => new(
        tenant.Id,
        tenant.Name,
        tenant.IsActive,
        tenant.CreatedAt,
        tenant.UpdatedAt,
        tenant.CreatedByName,
        tenant.UpdatedByName);

    public static IReadOnlyCollection<TenantDto> ToListDto(this IEnumerable<Tenant> tenants) =>
        [.. tenants.Select(tenant => tenant.ToDto())];
}
