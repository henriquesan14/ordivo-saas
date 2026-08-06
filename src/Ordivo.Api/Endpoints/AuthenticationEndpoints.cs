using Carter;
using Microsoft.AspNetCore.Antiforgery;
using Ordivo.Api.Authentication;
using Ordivo.Api.Common;
using Ordivo.Api.Security;
using Ordivo.Application.Authentication;
using Ordivo.Application.Authentication.Login;
using Ordivo.Application.Authentication.Register;
using Ordivo.Application.Authentication.Refresh;
using Ordivo.Application.Authentication.Logout;
using Ordivo.Application.Authentication.Sessions;
using Ordivo.Application.Authentication.Identity;
using Ordivo.SharedKernel.Messaging;

namespace Ordivo.Api.Endpoints;

public sealed class AuthenticationEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapGet("/csrf", (IAntiforgery antiforgery, HttpContext context) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            context.Response.Headers.CacheControl = "no-store";
            return Results.Ok(new { token = tokens.RequestToken, headerName = tokens.HeaderName });
        }).AllowAnonymous();

        group.MapPost("/register", async (
            RegisterCommand command,
            ICommandHandler<RegisterCommand, RegistrationDto> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.IsSuccess ? Results.Accepted(value: result.Value) : result.ToHttpResult();
        })
            .AllowAnonymous()
            .RequireRateLimiting(SecurityExtensions.AuthenticationRateLimitPolicy);

        group.MapPost("/login", async (
            LoginCommand command,
            ICommandHandler<LoginCommand, AuthDto> handler,
            HttpContext context,
            CancellationToken ct) => (await handler.Handle(command, ct)).ToAuthCookieResult(context))
            .AllowAnonymous()
            .RequireRateLimiting(SecurityExtensions.AuthenticationRateLimitPolicy);

        group.MapPost("/refresh", async (
            ICommandHandler<RefreshSessionCommand, AuthDto> handler,
            HttpContext context,
            CancellationToken ct) =>
        {
            var refreshToken = context.GetRefreshToken();
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Results.Unauthorized();
            return (await handler.Handle(new RefreshSessionCommand(refreshToken), ct)).ToAuthCookieResult(context);
        }).AllowAnonymous()
            .RequireRateLimiting(SecurityExtensions.RefreshRateLimitPolicy);

        group.MapPost("/verify-email", async (
            VerifyEmailCommand command,
            ICommandHandler<VerifyEmailCommand, bool> handler,
            CancellationToken ct) => (await handler.Handle(command, ct)).ToHttpResult())
            .AllowAnonymous()
            .RequireRateLimiting(SecurityExtensions.IdentityRateLimitPolicy);

        group.MapPost("/resend-verification", async (
            ResendVerificationCommand command,
            ICommandHandler<ResendVerificationCommand, bool> handler,
            CancellationToken ct) => (await handler.Handle(command, ct)).ToHttpResult())
            .AllowAnonymous()
            .RequireRateLimiting(SecurityExtensions.IdentityRateLimitPolicy);

        group.MapPost("/forgot-password", async (
            ForgotPasswordCommand command,
            ICommandHandler<ForgotPasswordCommand, bool> handler,
            CancellationToken ct) => (await handler.Handle(command, ct)).ToHttpResult())
            .AllowAnonymous()
            .RequireRateLimiting(SecurityExtensions.IdentityRateLimitPolicy);

        group.MapPost("/reset-password", async (
            ResetPasswordCommand command,
            ICommandHandler<ResetPasswordCommand, bool> handler,
            CancellationToken ct) => (await handler.Handle(command, ct)).ToHttpResult())
            .AllowAnonymous()
            .RequireRateLimiting(SecurityExtensions.IdentityRateLimitPolicy);

        group.MapPost("/invitations/accept", async (
            AcceptInvitationCommand command,
            ICommandHandler<AcceptInvitationCommand, bool> handler,
            CancellationToken ct) => (await handler.Handle(command, ct)).ToHttpResult())
            .AllowAnonymous()
            .RequireRateLimiting(SecurityExtensions.IdentityRateLimitPolicy);

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
        }).AllowAnonymous()
            .RequireRateLimiting(SecurityExtensions.RefreshRateLimitPolicy);

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
