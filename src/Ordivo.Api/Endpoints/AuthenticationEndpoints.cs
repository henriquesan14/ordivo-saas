using Carter;
using Ordivo.Api.Authentication;
using Ordivo.Application.Authentication;
using Ordivo.Application.Authentication.Login;
using Ordivo.Application.Authentication.Register;
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

        group.MapPost("/logout", (HttpContext context) =>
        {
            context.DeleteAuthCookie();
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
