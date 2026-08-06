using Carter;
using Ordivo.Api.Authentication;
using Ordivo.Api.Common;
using Ordivo.Api.Security;
using Ordivo.Application.Platform.Authentication;
using Ordivo.Application.Platform.Authentication.Login;
using Ordivo.Application.Platform.Authentication.Refresh;
using Ordivo.Application.Platform.Tenants;
using Ordivo.Application.Platform.Tenants.ListTenants;
using Ordivo.Application.Platform.Tenants.CreateTenant;
using Ordivo.Application.Platform.Tenants.ManageTenant;
using Ordivo.Application.Platform.Impersonation;
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
            .AllowAnonymous()
            .RequireRateLimiting(SecurityExtensions.AuthenticationRateLimitPolicy);

        app.MapPost("/api/platform/auth/refresh", async (
            ICommandHandler<RefreshPlatformSessionCommand, PlatformAuthDto> handler,
            HttpContext context,
            CancellationToken ct) =>
        {
            var refreshToken = context.GetRefreshToken();
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Results.Unauthorized();
            return (await handler.Handle(new RefreshPlatformSessionCommand(refreshToken), ct)).ToAuthCookieResult(context);
        })
            .WithTags("Platform")
            .AllowAnonymous()
            .RequireRateLimiting(SecurityExtensions.RefreshRateLimitPolicy);

        app.MapGet("/api/platform/tenants", async (
            IQueryHandler<ListPlatformTenantsQuery, IReadOnlyCollection<PlatformTenantDto>> handler,
            CancellationToken ct) => (await handler.Handle(new ListPlatformTenantsQuery(), ct)).ToHttpResult())
            .WithTags("Platform")
            .RequireAuthorization("PlatformAdmin");

        app.MapGet("/api/platform/tenants/{id:guid}", async (
            Guid id,
            IQueryHandler<GetPlatformTenantByIdQuery, PlatformTenantDto> handler,
            CancellationToken ct) => (await handler.Handle(new GetPlatformTenantByIdQuery(id), ct)).ToHttpResult())
            .WithTags("Platform")
            .RequireAuthorization("PlatformAdmin");

        app.MapGet("/api/platform/tenants/by-slug/{slug}", async (
            string slug,
            IQueryHandler<GetPlatformTenantBySlugQuery, PlatformTenantDto> handler,
            CancellationToken ct) => (await handler.Handle(new GetPlatformTenantBySlugQuery(slug), ct)).ToHttpResult())
            .WithTags("Platform")
            .RequireAuthorization("PlatformAdmin");

        app.MapPost("/api/platform/tenants", async (
            CreatePlatformTenantCommand command,
            ICommandHandler<CreatePlatformTenantCommand, CreatePlatformTenantDto> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/platform/tenants/{result.Value.TenantId}", result.Value)
                : result.ToHttpResult();
        })
            .WithTags("Platform")
            .RequireAuthorization("PlatformAdmin");

        app.MapPut("/api/platform/tenants/{id:guid}", async (
            Guid id,
            UpdatePlatformTenantRequest request,
            ICommandHandler<UpdatePlatformTenantCommand, PlatformTenantDto> handler,
            CancellationToken ct) =>
            (await handler.Handle(new UpdatePlatformTenantCommand(id, request.Name), ct)).ToHttpResult())
            .WithTags("Platform")
            .RequireAuthorization("PlatformAdmin");

        app.MapPatch("/api/platform/tenants/{id:guid}/status", async (
            Guid id,
            ChangePlatformTenantStatusRequest request,
            ICommandHandler<ChangePlatformTenantStatusCommand, PlatformTenantDto> handler,
            CancellationToken ct) =>
            (await handler.Handle(new ChangePlatformTenantStatusCommand(id, request.IsActive), ct)).ToHttpResult())
            .WithTags("Platform")
            .RequireAuthorization("PlatformAdmin");

        app.MapPost("/api/platform/impersonations", async (StartImpersonationCommand command, ICommandHandler<StartImpersonationCommand, ImpersonationDto> handler, HttpContext context, CancellationToken ct) =>
            (await handler.Handle(command, ct)).ToImpersonationCookieResult(context))
            .WithTags("Platform Impersonation").RequireAuthorization("PlatformAdmin");

        app.MapPost("/api/impersonation/end", async (ICommandHandler<EndImpersonationCommand, EndImpersonationDto> handler, HttpContext context, CancellationToken ct) =>
            (await handler.Handle(new EndImpersonationCommand(), ct)).ToRestorePlatformCookieResult(context))
            .WithTags("Platform Impersonation").RequireAuthorization("Impersonating");
    }

    private sealed record UpdatePlatformTenantRequest(string Name);
    private sealed record ChangePlatformTenantStatusRequest(bool IsActive);
}
