using Ordivo.Domain.ServiceOrders;

namespace Ordivo.Application.ServiceOrders;

public sealed record ServiceOrderStatusHistoryDto(Guid Id, ServiceOrderStatus Status, string ChangedByName, string? Note, DateTimeOffset ChangedAt);
public sealed record ServiceOrderCommentDto(Guid Id, Guid UserId, string UserName, string Text, DateTimeOffset CreatedAt);
public sealed record ServiceOrderAttachmentDto(Guid Id, Guid UserId, string UserName, string FileName, string ContentType, long Size, string StorageKey, DateTimeOffset CreatedAt);

public sealed record ServiceOrderDto(
    Guid Id, Guid TenantId, Guid CustomerId, string Number, string Title, string Description,
    decimal Price, ServiceOrderStatus Status, Guid? AssignedUserId, DateTimeOffset? ScheduledAt,
    DateTimeOffset? CompletedAt, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt,
    string CreatedByName, string? UpdatedByName,
    IReadOnlyCollection<ServiceOrderStatusHistoryDto> StatusHistory,
    IReadOnlyCollection<ServiceOrderCommentDto> Comments,
    IReadOnlyCollection<ServiceOrderAttachmentDto> Attachments);
