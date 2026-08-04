using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Infrastructure.Authentication;
using Ordivo.Infrastructure.Persistence;
using Ordivo.Infrastructure.Persistence.Interceptors;
using Ordivo.Infrastructure.Persistence.Repositories;

namespace Ordivo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrdivoDatabase")
            ?? throw new InvalidOperationException("Connection string 'OrdivoDatabase' was not configured.");

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddDbContext<OrdivoDbContext>((provider, options) => options
            .UseNpgsql(connectionString)
            .AddInterceptors(provider.GetRequiredService<AuditableEntityInterceptor>()));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<OrdivoDbContext>());
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPlatformUserRepository, PlatformUserRepository>();
        services.AddScoped<IPlatformTenantRepository, PlatformTenantRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IGenerateToken, JwtTokenGenerator>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddHttpContextAccessor();
        return services;
    }
}
