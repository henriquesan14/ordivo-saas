using Ordivo.Api.Common;
using Ordivo.Application.ServiceOrders;
using Ordivo.Application.ServiceOrders.ChangeServiceOrderStatus;
using Ordivo.Application.ServiceOrders.CreateServiceOrder;
using Ordivo.Application.ServiceOrders.GetServiceOrder;
using Ordivo.Application.ServiceOrders.ListServiceOrders;
using Ordivo.Domain.ServiceOrders;
using Ordivo.SharedKernel.Messaging;
namespace Ordivo.Api.Endpoints;
public static class ServiceOrderEndpoints
{
    public static IEndpointRouteBuilder MapServiceOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/service-orders").WithTags("Service orders").RequireAuthorization();
        group.MapPost("/", async (CreateServiceOrderCommand command, ICommandHandler<CreateServiceOrderCommand, ServiceOrderDto> handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.IsSuccess ? Results.Created($"/api/service-orders/{result.Value.Id}", result.Value) : result.ToHttpResult();
        });
        group.MapGet("/{id:guid}", async (Guid id, IQueryHandler<GetServiceOrderQuery, ServiceOrderDto> handler, CancellationToken ct) =>
            (await handler.Handle(new GetServiceOrderQuery(id), ct)).ToHttpResult());
        group.MapGet("/", async (IQueryHandler<ListServiceOrdersQuery, IReadOnlyCollection<ServiceOrderDto>> handler, CancellationToken ct) =>
            (await handler.Handle(new ListServiceOrdersQuery(), ct)).ToHttpResult());
        group.MapPatch("/{id:guid}/status", async (Guid id, ChangeStatusRequest request, ICommandHandler<ChangeServiceOrderStatusCommand, ServiceOrderDto> handler, CancellationToken ct) =>
            (await handler.Handle(new ChangeServiceOrderStatusCommand(id, request.Status), ct)).ToHttpResult());
        return app;
    }
    private sealed record ChangeStatusRequest(ServiceOrderStatus Status);
}
