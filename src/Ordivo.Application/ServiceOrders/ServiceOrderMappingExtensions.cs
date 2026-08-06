using Ordivo.Domain.ServiceOrders;

namespace Ordivo.Application.ServiceOrders;

public static class ServiceOrderMappingExtensions
{
    public static ServiceOrderDto ToDto(this ServiceOrder order) => new(
        order.Id, order.TenantId, order.CustomerId, order.Number, order.Title, order.Description,
        order.Price, order.Status, order.AssignedUserId, order.ScheduledAt, order.CompletedAt,
        order.CreatedAt, order.UpdatedAt, order.CreatedByName, order.UpdatedByName,
        [.. order.StatusHistory.OrderBy(item => item.ChangedAt).Select(item => new ServiceOrderStatusHistoryDto(item.Id, item.Status, item.ChangedByName, item.Note, item.ChangedAt))],
        [.. order.Comments.OrderBy(item => item.CreatedAt).Select(item => new ServiceOrderCommentDto(item.Id, item.UserId, item.UserName, item.Text, item.CreatedAt))],
        [.. order.Attachments.OrderBy(item => item.CreatedAt).Select(item => new ServiceOrderAttachmentDto(item.Id, item.UserId, item.UserName, item.FileName, item.ContentType, item.Size, item.StorageKey, item.CreatedAt))]);

    public static IReadOnlyCollection<ServiceOrderDto> ToListDto(this IEnumerable<ServiceOrder> orders) =>
        [.. orders.Select(order => order.ToDto())];
}
