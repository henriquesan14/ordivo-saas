namespace Ordivo.Application.Customers;

public sealed record CustomerDto(
    Guid Id,
    Guid TenantId,
    string Name,
    string Document,
    string Phone,
    string? Email,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string CreatedByName,
    string? UpdatedByName);
