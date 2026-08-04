using Ordivo.Application.Abstractions.Persistence;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;
namespace Ordivo.Application.ServiceOrders.ListServiceOrders;
public sealed record ListServiceOrdersQuery : IQuery<IReadOnlyCollection<ServiceOrderDto>>;
public sealed class ListServiceOrdersQueryHandler(IServiceOrderRepository orders) : IQueryHandler<ListServiceOrdersQuery, IReadOnlyCollection<ServiceOrderDto>>
{
    public async Task<Result<IReadOnlyCollection<ServiceOrderDto>>> Handle(ListServiceOrdersQuery query, CancellationToken ct) =>
        Result.Success((await orders.ListAsync(ct)).ToListDto());
}
