namespace Ordivo.Application.Tenants;

public sealed record TenantDto(
    Guid Id,
    string Name,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string CreatedByName,
    string? UpdatedByName);
