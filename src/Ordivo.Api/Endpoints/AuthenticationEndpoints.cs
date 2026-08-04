using Ordivo.Api.Common;
using Ordivo.Application.Authentication;
using Ordivo.Application.Authentication.Login;
using Ordivo.Application.Authentication.Register;
using Ordivo.SharedKernel.Messaging;

namespace Ordivo.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication").AllowAnonymous();

        group.MapPost("/register", async (
            RegisterCommand command,
            ICommandHandler<RegisterCommand, AuthDto> handler,
            CancellationToken ct) => (await handler.Handle(command, ct)).ToHttpResult());

        group.MapPost("/login", async (
            LoginCommand command,
            ICommandHandler<LoginCommand, AuthDto> handler,
            CancellationToken ct) => (await handler.Handle(command, ct)).ToHttpResult());

        return app;
    }
}
