using Microsoft.Extensions.DependencyInjection;
using Ordivo.Application.Customers;
using Ordivo.Application.Authentication;
using Ordivo.Application.Authentication.Login;
using Ordivo.Application.Authentication.Register;
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
using Ordivo.Application.Platform.Tenants;
using Ordivo.Application.Platform.Tenants.ListTenants;
using Ordivo.SharedKernel.Messaging;
namespace Ordivo.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) => services
        .AddScoped<ICommandHandler<RegisterCommand, AuthDto>, RegisterCommandHandler>()
        .AddScoped<ICommandHandler<LoginCommand, AuthDto>, LoginCommandHandler>()
        .AddScoped<ICommandHandler<PlatformLoginCommand, PlatformAuthDto>, PlatformLoginCommandHandler>()
        .AddScoped<IQueryHandler<ListPlatformTenantsQuery, IReadOnlyCollection<PlatformTenantDto>>, ListPlatformTenantsQueryHandler>()
        .AddScoped<IQueryHandler<GetCurrentTenantQuery, TenantDto>, GetCurrentTenantQueryHandler>()
        .AddScoped<ICommandHandler<UpdateTenantCommand, TenantDto>, UpdateTenantCommandHandler>()
        .AddScoped<ICommandHandler<CreateCustomerCommand, CustomerDto>, CreateCustomerCommandHandler>()
        .AddScoped<IQueryHandler<GetCustomerQuery, CustomerDto>, GetCustomerQueryHandler>()
        .AddScoped<IQueryHandler<ListCustomersQuery, IReadOnlyCollection<CustomerDto>>, ListCustomersQueryHandler>()
        .AddScoped<ICommandHandler<CreateServiceOrderCommand, ServiceOrderDto>, CreateServiceOrderCommandHandler>()
        .AddScoped<IQueryHandler<GetServiceOrderQuery, ServiceOrderDto>, GetServiceOrderQueryHandler>()
        .AddScoped<IQueryHandler<ListServiceOrdersQuery, IReadOnlyCollection<ServiceOrderDto>>, ListServiceOrdersQueryHandler>()
        .AddScoped<ICommandHandler<ChangeServiceOrderStatusCommand, ServiceOrderDto>, ChangeServiceOrderStatusCommandHandler>();
}
