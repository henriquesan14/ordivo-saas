using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Ordivo.Application.Behaviors;
using Ordivo.Application.Customers;
using Ordivo.Application.Authentication;
using Ordivo.Application.Authentication.Login;
using Ordivo.Application.Authentication.Register;
using Ordivo.Application.Authentication.Refresh;
using Ordivo.Application.Authentication.Logout;
using Ordivo.Application.Authentication.Sessions;
using Ordivo.Application.Customers.CreateCustomer;
using Ordivo.Application.Customers.GetCustomer;
using Ordivo.Application.Customers.ListCustomers;
using Ordivo.Application.ServiceOrders.ChangeServiceOrderStatus;
using Ordivo.Application.ServiceOrders;
using Ordivo.Application.ServiceOrders.CreateServiceOrder;
using Ordivo.Application.ServiceOrders.GetServiceOrder;
using Ordivo.Application.ServiceOrders.ListServiceOrders;
using Ordivo.Application.Tenants;
using Ordivo.Application.Tenants.GetCurrentTenant;
using Ordivo.Application.Tenants.UpdateTenant;
using Ordivo.Application.Platform.Authentication;
using Ordivo.Application.Platform.Authentication.Login;
using Ordivo.Application.Platform.Authentication.Refresh;
using Ordivo.Application.Platform.Tenants;
using Ordivo.Application.Platform.Tenants.ListTenants;
using Ordivo.Application.Platform.Tenants.CreateTenant;
using Ordivo.SharedKernel.Messaging;
namespace Ordivo.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>(ServiceLifetime.Scoped);

        services.AddCommandHandler<RegisterCommand, AuthDto, RegisterCommandHandler>();
        services.AddCommandHandler<LoginCommand, AuthDto, LoginCommandHandler>();
        services.AddCommandHandler<RefreshSessionCommand, AuthDto, RefreshSessionCommandHandler>();
        services.AddCommandHandler<RevokeSessionCommand, bool, RevokeSessionCommandHandler>();
        services.AddCommandHandler<RevokeSessionByIdCommand, bool, RevokeSessionByIdCommandHandler>();
        services.AddCommandHandler<PlatformLoginCommand, PlatformAuthDto, PlatformLoginCommandHandler>();
        services.AddCommandHandler<RefreshPlatformSessionCommand, PlatformAuthDto, RefreshPlatformSessionCommandHandler>();
        services.AddCommandHandler<CreatePlatformTenantCommand, CreatePlatformTenantDto, CreatePlatformTenantCommandHandler>();
        services.AddCommandHandler<UpdateTenantCommand, TenantDto, UpdateTenantCommandHandler>();
        services.AddCommandHandler<CreateCustomerCommand, CustomerDto, CreateCustomerCommandHandler>();
        services.AddCommandHandler<CreateServiceOrderCommand, ServiceOrderDto, CreateServiceOrderCommandHandler>();
        services.AddCommandHandler<ChangeServiceOrderStatusCommand, ServiceOrderDto, ChangeServiceOrderStatusCommandHandler>();

        services.AddScoped<IQueryHandler<ListPlatformTenantsQuery, IReadOnlyCollection<PlatformTenantDto>>, ListPlatformTenantsQueryHandler>();
        services.AddScoped<IQueryHandler<ListAuthSessionsQuery, IReadOnlyCollection<AuthSessionDto>>, ListAuthSessionsQueryHandler>();
        services.AddScoped<IQueryHandler<GetCurrentTenantQuery, TenantDto>, GetCurrentTenantQueryHandler>();
        services.AddScoped<IQueryHandler<GetCustomerQuery, CustomerDto>, GetCustomerQueryHandler>();
        services.AddScoped<IQueryHandler<ListCustomersQuery, IReadOnlyCollection<CustomerDto>>, ListCustomersQueryHandler>();
        services.AddScoped<IQueryHandler<GetServiceOrderQuery, ServiceOrderDto>, GetServiceOrderQueryHandler>();
        services.AddScoped<IQueryHandler<ListServiceOrdersQuery, IReadOnlyCollection<ServiceOrderDto>>, ListServiceOrdersQueryHandler>();

        return services;
    }

    private static IServiceCollection AddCommandHandler<TCommand, TResponse, THandler>(this IServiceCollection services)
        where TCommand : ICommand<TResponse>
        where THandler : class, ICommandHandler<TCommand, TResponse>
    {
        services.AddScoped<THandler>();
        services.AddScoped<ICommandHandler<TCommand, TResponse>>(provider =>
            new ValidationCommandHandlerDecorator<TCommand, TResponse>(
                provider.GetRequiredService<THandler>(),
                provider.GetServices<IValidator<TCommand>>()));
        return services;
    }
}
