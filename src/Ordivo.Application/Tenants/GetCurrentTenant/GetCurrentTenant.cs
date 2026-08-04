using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.Tenants.GetCurrentTenant;

public sealed record GetCurrentTenantQuery : IQuery<TenantDto>;

public sealed class GetCurrentTenantQueryHandler(ITenantRepository tenants, IUserContext userContext)
    : IQueryHandler<GetCurrentTenantQuery, TenantDto>
{
    public async Task<Result<TenantDto>> Handle(GetCurrentTenantQuery query, CancellationToken ct) =>
        await tenants.GetAsync(userContext.TenantId, ct) is { } tenant
            ? Result.Success(tenant.ToDto())
            : Result.Failure<TenantDto>(Error.NotFound("Tenant not found."));
}
