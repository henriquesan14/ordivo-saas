using Carter;
using Ordivo.Api.Authentication;
using Ordivo.Api.Common;
using Ordivo.Application.Authentication;
using Ordivo.Application.Authentication.Login;
using Ordivo.Application.Authentication.Register;
using Ordivo.Application.Authentication.Refresh;
using Ordivo.Application.Authentication.Logout;
using Ordivo.Application.Authentication.Sessions;
using Ordivo.SharedKernel.Messaging;

namespace Ordivo.Api.Endpoints;

public sealed class AuthenticationEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/register", async (
            RegisterCommand command,
            ICommandHandler<RegisterCommand, AuthDto> handler,
            HttpContext context,
            CancellationToken ct) => (await handler.Handle(command, ct)).ToAuthCookieResult(context))
            .AllowAnonymous();

        group.MapPost("/login", async (
            LoginCommand command,
            ICommandHandler<LoginCommand, AuthDto> handler,
            HttpContext context,
            CancellationToken ct) => (await handler.Handle(command, ct)).ToAuthCookieResult(context))
            .AllowAnonymous();

        group.MapPost("/refresh", async (
            ICommandHandler<RefreshSessionCommand, AuthDto> handler,
            HttpContext context,
            CancellationToken ct) =>
        {
            var refreshToken = context.GetRefreshToken();
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Results.Unauthorized();
            return (await handler.Handle(new RefreshSessionCommand(refreshToken), ct)).ToAuthCookieResult(context);
        }).AllowAnonymous();

        group.MapPost("/logout", async (
            ICommandHandler<RevokeSessionCommand, bool> handler,
            HttpContext context,
            CancellationToken ct) =>
        {
            var refreshToken = context.GetRefreshToken();
            if (!string.IsNullOrWhiteSpace(refreshToken))
                await handler.Handle(new RevokeSessionCommand(refreshToken), ct);
            context.DeleteAuthCookie();
            return Results.NoContent();
        }).AllowAnonymous();

        group.MapGet("/sessions", async (
            IQueryHandler<ListAuthSessionsQuery, IReadOnlyCollection<AuthSessionDto>> handler,
            CancellationToken ct) =>
            (await handler.Handle(new ListAuthSessionsQuery(), ct)).ToHttpResult())
            .RequireAuthorization();

        group.MapDelete("/sessions/{id:guid}", async (
            Guid id,
            ICommandHandler<RevokeSessionByIdCommand, bool> handler,
            CancellationToken ct) =>
            (await handler.Handle(new RevokeSessionByIdCommand(id), ct)).ToHttpResult())
            .RequireAuthorization();
    }
}
