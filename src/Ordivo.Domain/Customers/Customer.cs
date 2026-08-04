using Ordivo.SharedKernel.Domain;
namespace Ordivo.Domain.Customers;
public sealed class Customer : AggregateRoot<Guid>, ITenantEntity
{
    private Customer(Guid id) : base(id) { }
    public static Customer Create(Guid tenantId, string name, string document, string phone, string? email)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant is required.", nameof(tenantId));
        var customer = new Customer(Guid.NewGuid());
        customer.TenantId = tenantId;
        customer.Update(name, document, phone, email);
        customer.Raise(new CustomerCreatedDomainEvent(customer.Id, customer.CreatedAt));
        return customer;
    }
    public string Name { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    public string Document { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public void Update(string name, string document, string phone, string? email)
    {
        Name = Required(name, nameof(name), 120); Document = Required(document, nameof(document), 20);
        Phone = Required(phone, nameof(phone), 20); Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }
    private static string Required(string value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{field} is required.", field);
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : throw new ArgumentException($"{field} must have at most {maxLength} characters.", field);
    }
}
public sealed record CustomerCreatedDomainEvent(Guid CustomerId, DateTimeOffset OccurredAt) : IDomainEvent;
