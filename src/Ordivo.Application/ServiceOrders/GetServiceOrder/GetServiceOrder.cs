using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;
namespace Ordivo.Application.ServiceOrders.GetServiceOrder;
public sealed record GetServiceOrderQuery(Guid ServiceOrderId) : IQuery<ServiceOrderDto>;
public sealed class GetServiceOrderQueryHandler(IServiceOrderRepository orders) : IQueryHandler<GetServiceOrderQuery, ServiceOrderDto>
{
    public async Task<Result<ServiceOrderDto>> Handle(GetServiceOrderQuery query, CancellationToken ct) =>
        await orders.GetAsync(query.ServiceOrderId, ct) is { } order ? Result.Success(order.ToDto()) : Result.Failure<ServiceOrderDto>(Error.NotFound("Service order not found."));
}
