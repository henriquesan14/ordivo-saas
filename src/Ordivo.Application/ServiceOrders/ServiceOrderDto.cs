using Ordivo.Domain.ServiceOrders;

namespace Ordivo.Application.ServiceOrders;

public sealed record ServiceOrderDto(
    Guid Id,
    Guid TenantId,
    Guid CustomerId,
    string Title,
    string Description,
    decimal Price,
    ServiceOrderStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string CreatedByName,
    string? UpdatedByName,
    DateTimeOffset? CompletedAt);
