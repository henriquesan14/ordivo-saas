using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Infrastructure.Authentication;
using Ordivo.Infrastructure.DomainEvents;
using Ordivo.Infrastructure.Persistence;
using Ordivo.Infrastructure.Persistence.Interceptors;
using Ordivo.Infrastructure.Persistence.Repositories;
using Ordivo.SharedKernel.Domain;
using Ordivo.Infrastructure.Health;
using Ordivo.Infrastructure.BackgroundJobs;

namespace Ordivo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrdivoDatabase")
            ?? throw new InvalidOperationException("Connection string 'OrdivoDatabase' was not configured.");

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddScoped<OutboxInterceptor>();
        services.AddDbContext<OrdivoDbContext>((provider, options) => options
            .UseNpgsql(connectionString)
            .AddInterceptors(
                provider.GetRequiredService<AuditableEntityInterceptor>(),
                provider.GetRequiredService<OutboxInterceptor>(),
                provider.GetRequiredService<DispatchDomainEventsInterceptor>()));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<OrdivoDbContext>());
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPlatformUserRepository, PlatformUserRepository>();
        services.AddScoped<IPlatformTenantRepository, PlatformTenantRepository>();
        services.AddScoped<IAuthSessionRepository, AuthSessionRepository>();
        services.AddScoped<IIdentityTokenRepository, IdentityTokenRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IGenerateToken, JwtTokenGenerator>();
        services.AddOptions<RefreshTokenOptions>()
            .Bind(configuration.GetSection(RefreshTokenOptions.SectionName))
            .Validate(options => options.ExpirationDays is >= 1 and <= 365, "Refresh token expiration must be between 1 and 365 days.")
            .ValidateOnStart();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IIdentityTokenGenerator, IdentityTokenGenerator>();
        services.AddOptions<EmailOptions>().Bind(configuration.GetSection(EmailOptions.SectionName));
        services.AddScoped<IIdentityEmailSender, IdentityEmailSender>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddHttpContextAccessor();
        services.AddHealthChecks().AddCheck<PostgresHealthCheck>("postgres", tags: ["ready"]);
        services.AddHostedService<OutboxWorker>();
        services.AddHostedService<SessionCleanupWorker>();
        return services;
    }
}
