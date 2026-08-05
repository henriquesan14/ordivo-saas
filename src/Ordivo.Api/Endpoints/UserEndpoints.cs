using Carter;
using Ordivo.Api.Common;
using Ordivo.Application.Users;
using Ordivo.Application.Users.ChangeMyPassword;
using Ordivo.Application.Users.ChangeUserRole;
using Ordivo.Application.Users.ChangeUserStatus;
using Ordivo.Application.Users.CreateUser;
using Ordivo.Application.Users.GetUser;
using Ordivo.Application.Users.ListUsers;
using Ordivo.Domain.Users;
using Ordivo.SharedKernel.Messaging;

namespace Ordivo.Api.Endpoints;

public sealed class UserEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization("TenantUser");

        group.MapGet("/", async (
            IQueryHandler<ListUsersQuery, IReadOnlyCollection<UserDto>> handler,
            CancellationToken ct) =>
            (await handler.Handle(new ListUsersQuery(), ct)).ToHttpResult());

        group.MapGet("/{id:guid}", async (
            Guid id,
            IQueryHandler<GetUserQuery, UserDto> handler,
            CancellationToken ct) =>
            (await handler.Handle(new GetUserQuery(id), ct)).ToHttpResult());

        group.MapPost("/", async (
            CreateUserCommand command,
            ICommandHandler<CreateUserCommand, UserDto> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.IsSuccess
                ? Results.Created($"/api/users/{result.Value.Id}", result.Value)
                : result.ToHttpResult();
        }).RequireAuthorization("TenantAdmin");

        group.MapPatch("/{id:guid}/role", async (
            Guid id,
            ChangeRoleRequest request,
            ICommandHandler<ChangeUserRoleCommand, UserDto> handler,
            CancellationToken ct) =>
            (await handler.Handle(new ChangeUserRoleCommand(id, request.Role), ct)).ToHttpResult())
            .RequireAuthorization("TenantOwner");

        group.MapPatch("/{id:guid}/status", async (
            Guid id,
            ChangeStatusRequest request,
            ICommandHandler<ChangeUserStatusCommand, UserDto> handler,
            CancellationToken ct) =>
            (await handler.Handle(new ChangeUserStatusCommand(id, request.IsActive), ct)).ToHttpResult())
            .RequireAuthorization("TenantAdmin");

        group.MapPut("/me/password", async (
            ChangeMyPasswordCommand command,
            ICommandHandler<ChangeMyPasswordCommand, bool> handler,
            CancellationToken ct) =>
            (await handler.Handle(command, ct)).ToHttpResult());
    }

    private sealed record ChangeRoleRequest(UserRole Role);
    private sealed record ChangeStatusRequest(bool IsActive);
}
