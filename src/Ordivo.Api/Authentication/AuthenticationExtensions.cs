using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ordivo.Infrastructure.Authentication;

namespace Ordivo.Api.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetRequiredSection(JwtOptions.SectionName);
        var jwt = section.Get<JwtOptions>() ?? throw new InvalidOperationException("JWT configuration is missing.");

        if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience) || jwt.Key.Length < 32)
            throw new InvalidOperationException("JWT issuer, audience and a key of at least 32 characters are required.");

        services.AddOptions<JwtOptions>().Bind(section).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<AuthCookieOptions>()
            .Bind(configuration.GetSection(AuthCookieOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Name) && !string.IsNullOrWhiteSpace(options.RefreshName),
                "Access and refresh cookie names are required.")
            .ValidateOnStart();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var cookie = context.HttpContext.RequestServices
                            .GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthCookieOptions>>().Value;
                        context.Token = context.Request.Cookies[cookie.Name];
                        return Task.CompletedTask;
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });
        services.AddAuthorization(options =>
        {
            options.AddPolicy("TenantUser", policy => policy.RequireClaim("tenant_id"));
            options.AddPolicy("PlatformAdmin", policy =>
                policy.RequireClaim("platform_role", "PlatformAdmin"));
        });
        return services;
    }
}
