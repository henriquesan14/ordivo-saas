using Ordivo.Domain.Customers;

namespace Ordivo.Application.Customers;

public static class CustomerMappingExtensions
{
    public static CustomerDto ToDto(this Customer customer) => new(
        customer.Id,
        customer.TenantId,
        customer.Name,
        customer.Document,
        customer.Phone,
        customer.Email,
        customer.CreatedAt,
        customer.UpdatedAt,
        customer.CreatedByName,
        customer.UpdatedByName);

    public static IReadOnlyCollection<CustomerDto> ToListDto(this IEnumerable<Customer> customers) =>
        [.. customers.Select(customer => customer.ToDto())];
}
