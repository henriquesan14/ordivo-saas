using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Platform.Tenants.ListTenants;

public sealed record ListPlatformTenantsQuery : IQuery<IReadOnlyCollection<PlatformTenantDto>>;

public sealed class ListPlatformTenantsQueryHandler(IPlatformTenantRepository tenants)
    : IQueryHandler<ListPlatformTenantsQuery, IReadOnlyCollection<PlatformTenantDto>>
{
    public async Task<Result<IReadOnlyCollection<PlatformTenantDto>>> Handle(ListPlatformTenantsQuery query, CancellationToken ct) =>
        Result.Success((await tenants.ListAsync(ct)).ToPlatformListDto());
}
