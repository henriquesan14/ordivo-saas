using Ordivo.Domain.ServiceOrders;

namespace Ordivo.Application.ServiceOrders;

public static class ServiceOrderMappingExtensions
{
    public static ServiceOrderDto ToDto(this ServiceOrder order) => new(
        order.Id,
        order.TenantId,
        order.CustomerId,
        order.Title,
        order.Description,
        order.Price,
        order.Status,
        order.CreatedAt,
        order.UpdatedAt,
        order.CreatedByName,
        order.UpdatedByName,
        order.CompletedAt);

    public static IReadOnlyCollection<ServiceOrderDto> ToListDto(this IEnumerable<ServiceOrder> orders) =>
        [.. orders.Select(order => order.ToDto())];
}
