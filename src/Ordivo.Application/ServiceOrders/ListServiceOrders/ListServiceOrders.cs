using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Application.Common;
using Ordivo.Domain.ServiceOrders;
using Ordivo.SharedKernel.Messaging;
using Ordivo.SharedKernel.Results;

namespace Ordivo.Application.ServiceOrders.ListServiceOrders;
public sealed record ListServiceOrdersQuery(string? Search=null, ServiceOrderStatus? Status=null, Guid? CustomerId=null,
    Guid? AssignedUserId=null, DateTimeOffset? ScheduledFrom=null, DateTimeOffset? ScheduledTo=null,
    int Page=1, int PageSize=20, string SortBy="createdAt", bool Descending=true) : IQuery<PagedResult<ServiceOrderDto>>;
public sealed class ListServiceOrdersQueryHandler(IServiceOrderRepository orders) : IQueryHandler<ListServiceOrdersQuery, PagedResult<ServiceOrderDto>>
{
    public async Task<Result<PagedResult<ServiceOrderDto>>> Handle(ListServiceOrdersQuery q, CancellationToken ct)
    {
        var page=Math.Max(1,q.Page); var size=Math.Clamp(q.PageSize,1,100);
        var result=await orders.ListAsync(q.Search,q.Status,q.CustomerId,q.AssignedUserId,q.ScheduledFrom,q.ScheduledTo,page,size,q.SortBy,q.Descending,ct);
        return Result.Success(new PagedResult<ServiceOrderDto>(result.Items.ToListDto(),page,size,result.TotalCount));
    }
}
