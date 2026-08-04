using Carter;
using Ordivo.Api.Authentication;
using Ordivo.Api.Common;
using Ordivo.Application.Platform.Authentication;
using Ordivo.Application.Platform.Authentication.Login;
using Ordivo.Application.Platform.Tenants;
using Ordivo.Application.Platform.Tenants.ListTenants;
using Ordivo.SharedKernel.Messaging;

namespace Ordivo.Api.Endpoints;

public sealed class PlatformEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/platform/auth/login", async (
            PlatformLoginCommand command,
            ICommandHandler<PlatformLoginCommand, PlatformAuthDto> handler,
            HttpContext context,
            CancellationToken ct) => (await handler.Handle(command, ct)).ToAuthCookieResult(context))
            .WithTags("Platform")
            .AllowAnonymous();

        app.MapGet("/api/platform/tenants", async (
            IQueryHandler<ListPlatformTenantsQuery, IReadOnlyCollection<PlatformTenantDto>> handler,
            CancellationToken ct) => (await handler.Handle(new ListPlatformTenantsQuery(), ct)).ToHttpResult())
            .WithTags("Platform")
            .RequireAuthorization("PlatformAdmin");
    }
}
