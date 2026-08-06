using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.RateLimiting;

namespace Ordivo.Api.Security;

public static class SecurityExtensions
{
    public const string CorsPolicy = "Frontend";
    public const string AuthenticationRateLimitPolicy = "Authentication";
    public const string RefreshRateLimitPolicy = "Refresh";
    public const string IdentityRateLimitPolicy = "Identity";

    public static IServiceCollection AddApiSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
        {
            if (allowedOrigins.Length > 0)
                policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }));

        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "ordivo.csrf";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
        });

        var globalPermitLimit = configuration.GetValue("RateLimiting:GlobalPermitLimit", 120);
        var authPermitLimit = configuration.GetValue("RateLimiting:AuthenticationPermitLimit", 5);
        var refreshPermitLimit = configuration.GetValue("RateLimiting:RefreshPermitLimit", 20);
        var identityPermitLimit = configuration.GetValue("RateLimiting:IdentityPermitLimit", 10);
        var windowSeconds = configuration.GetValue("RateLimiting:WindowSeconds", 60);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                CreatePartition(context, "global", globalPermitLimit, windowSeconds));
            options.AddPolicy(AuthenticationRateLimitPolicy, context =>
                CreatePartition(context, "authentication", authPermitLimit, windowSeconds));
            options.AddPolicy(RefreshRateLimitPolicy, context =>
                CreatePartition(context, "refresh", refreshPermitLimit, windowSeconds));
            options.AddPolicy(IdentityRateLimitPolicy, context =>
                CreatePartition(context, "identity", identityPermitLimit, windowSeconds));
            options.OnRejected = async (context, cancellationToken) =>
            {
                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var value)
                    ? Math.Ceiling(value.TotalSeconds)
                    : windowSeconds;
                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString(CultureInfo.InvariantCulture);
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    type = "https://httpstatuses.com/429",
                    title = "Too many requests",
                    status = StatusCodes.Status429TooManyRequests,
                    detail = "The request limit was exceeded. Try again later.",
                    instance = context.HttpContext.Request.Path.Value,
                    traceId = context.HttpContext.TraceIdentifier
                }, cancellationToken);
            };
        });

        return services;
    }

    public static IApplicationBuilder UseApiCsrfProtection(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/api") && IsUnsafeMethod(context.Request.Method))
            {
                var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
                await antiforgery.ValidateRequestAsync(context);
            }

            await next(context);
        });

    private static RateLimitPartition<string> CreatePartition(
        HttpContext context,
        string policy,
        int permitLimit,
        int windowSeconds)
    {
        var subject = context.User.FindFirstValue("sub");
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"{policy}:{subject ?? remoteIp}";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromSeconds(windowSeconds),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    }

    private static bool IsUnsafeMethod(string method) =>
        !HttpMethods.IsGet(method) &&
        !HttpMethods.IsHead(method) &&
        !HttpMethods.IsOptions(method) &&
        !HttpMethods.IsTrace(method);
}
