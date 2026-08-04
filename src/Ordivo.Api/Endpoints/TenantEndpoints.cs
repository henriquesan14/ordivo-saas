using Carter;
using Ordivo.Api.Common;
using Ordivo.Application.Tenants;
using Ordivo.Application.Tenants.GetCurrentTenant;
using Ordivo.Application.Tenants.UpdateTenant;
using Ordivo.SharedKernel.Messaging;

namespace Ordivo.Api.Endpoints;

public sealed class TenantEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenant").WithTags("Tenant").RequireAuthorization("TenantUser");

        group.MapGet("/", async (
            IQueryHandler<GetCurrentTenantQuery, TenantDto> handler,
            CancellationToken ct) => (await handler.Handle(new GetCurrentTenantQuery(), ct)).ToHttpResult());

        group.MapPut("/", async (
            UpdateTenantCommand command,
            ICommandHandler<UpdateTenantCommand, TenantDto> handler,
            CancellationToken ct) => (await handler.Handle(command, ct)).ToHttpResult())
            .RequireAuthorization(policy => policy.RequireRole("Owner", "Admin"));
    }
}
