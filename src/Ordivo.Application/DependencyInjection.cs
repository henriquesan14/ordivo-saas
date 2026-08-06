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
using Ordivo.Application.Authentication.Identity;
using Ordivo.Application.Customers.CreateCustomer;
using Ordivo.Application.Customers.GetCustomer;
using Ordivo.Application.Customers.ListCustomers;
using Ordivo.Application.Customers.UpdateCustomer;
using Ordivo.Application.Customers.ChangeCustomerStatus;
using Ordivo.Application.Common;
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
using Ordivo.Application.Platform.Tenants.ManageTenant;
using Ordivo.Application.Users;
using Ordivo.Application.Users.CreateUser;
using Ordivo.Application.Users.ListUsers;
using Ordivo.Application.Users.GetUser;
using Ordivo.Application.Users.ChangeUserRole;
using Ordivo.Application.Users.ChangeUserStatus;
using Ordivo.Application.Users.ChangeMyPassword;
using Ordivo.Application.Users.InviteUser;
using Ordivo.SharedKernel.Messaging;
namespace Ordivo.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>(ServiceLifetime.Scoped);

        services.AddCommandHandler<RegisterCommand, RegistrationDto, RegisterCommandHandler>();
        services.AddCommandHandler<VerifyEmailCommand, bool, VerifyEmailCommandHandler>();
        services.AddCommandHandler<ResendVerificationCommand, bool, ResendVerificationCommandHandler>();
        services.AddCommandHandler<ForgotPasswordCommand, bool, ForgotPasswordCommandHandler>();
        services.AddCommandHandler<ResetPasswordCommand, bool, ResetPasswordCommandHandler>();
        services.AddCommandHandler<AcceptInvitationCommand, bool, AcceptInvitationCommandHandler>();
        services.AddCommandHandler<LoginCommand, AuthDto, LoginCommandHandler>();
        services.AddCommandHandler<RefreshSessionCommand, AuthDto, RefreshSessionCommandHandler>();
        services.AddCommandHandler<RevokeSessionCommand, bool, RevokeSessionCommandHandler>();
        services.AddCommandHandler<RevokeSessionByIdCommand, bool, RevokeSessionByIdCommandHandler>();
        services.AddCommandHandler<PlatformLoginCommand, PlatformAuthDto, PlatformLoginCommandHandler>();
        services.AddCommandHandler<RefreshPlatformSessionCommand, PlatformAuthDto, RefreshPlatformSessionCommandHandler>();
        services.AddCommandHandler<CreatePlatformTenantCommand, CreatePlatformTenantDto, CreatePlatformTenantCommandHandler>();
        services.AddCommandHandler<UpdatePlatformTenantCommand, PlatformTenantDto, UpdatePlatformTenantCommandHandler>();
        services.AddCommandHandler<ChangePlatformTenantStatusCommand, PlatformTenantDto, ChangePlatformTenantStatusCommandHandler>();
        services.AddCommandHandler<UpdateTenantCommand, TenantDto, UpdateTenantCommandHandler>();
        services.AddCommandHandler<CreateCustomerCommand, CustomerDto, CreateCustomerCommandHandler>();
        services.AddCommandHandler<UpdateCustomerCommand, CustomerDto, UpdateCustomerCommandHandler>();
        services.AddCommandHandler<ChangeCustomerStatusCommand, CustomerDto, ChangeCustomerStatusCommandHandler>();
        services.AddCommandHandler<CreateServiceOrderCommand, ServiceOrderDto, CreateServiceOrderCommandHandler>();
        services.AddCommandHandler<ChangeServiceOrderStatusCommand, ServiceOrderDto, ChangeServiceOrderStatusCommandHandler>();
        services.AddCommandHandler<CreateUserCommand, UserDto, CreateUserCommandHandler>();
        services.AddCommandHandler<ChangeUserRoleCommand, UserDto, ChangeUserRoleCommandHandler>();
        services.AddCommandHandler<ChangeUserStatusCommand, UserDto, ChangeUserStatusCommandHandler>();
        services.AddCommandHandler<ChangeMyPasswordCommand, bool, ChangeMyPasswordCommandHandler>();
        services.AddCommandHandler<InviteUserCommand, UserDto, InviteUserCommandHandler>();

        services.AddScoped<IQueryHandler<ListPlatformTenantsQuery, IReadOnlyCollection<PlatformTenantDto>>, ListPlatformTenantsQueryHandler>();
        services.AddScoped<IQueryHandler<GetPlatformTenantByIdQuery, PlatformTenantDto>, GetPlatformTenantByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetPlatformTenantBySlugQuery, PlatformTenantDto>, GetPlatformTenantBySlugQueryHandler>();
        services.AddScoped<IQueryHandler<ListAuthSessionsQuery, IReadOnlyCollection<AuthSessionDto>>, ListAuthSessionsQueryHandler>();
        services.AddScoped<IQueryHandler<GetCurrentTenantQuery, TenantDto>, GetCurrentTenantQueryHandler>();
        services.AddScoped<IQueryHandler<GetCustomerQuery, CustomerDto>, GetCustomerQueryHandler>();
        services.AddScoped<IQueryHandler<ListCustomersQuery, PagedResult<CustomerDto>>, ListCustomersQueryHandler>();
        services.AddScoped<IQueryHandler<GetServiceOrderQuery, ServiceOrderDto>, GetServiceOrderQueryHandler>();
        services.AddScoped<IQueryHandler<ListServiceOrdersQuery, IReadOnlyCollection<ServiceOrderDto>>, ListServiceOrdersQueryHandler>();
        services.AddScoped<IQueryHandler<ListUsersQuery, IReadOnlyCollection<UserDto>>, ListUsersQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserQuery, UserDto>, GetUserQueryHandler>();

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
