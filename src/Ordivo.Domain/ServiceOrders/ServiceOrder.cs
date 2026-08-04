using Ordivo.SharedKernel.Domain;
namespace Ordivo.Domain.ServiceOrders;
public enum ServiceOrderStatus { Open, InProgress, Completed, Cancelled }
public sealed class ServiceOrder : AggregateRoot<Guid>, ITenantEntity
{
    private ServiceOrder(Guid id) : base(id) { }
    public static ServiceOrder Create(Guid tenantId, Guid customerId, string title, string description, decimal price)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant is required.", nameof(tenantId));
        if (customerId == Guid.Empty) throw new ArgumentException("Customer is required.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        var order = new ServiceOrder(Guid.NewGuid()) { TenantId = tenantId, CustomerId = customerId, Title = title.Trim(), Description = description?.Trim() ?? string.Empty, Price = price, Status = ServiceOrderStatus.Open };
        order.Raise(new ServiceOrderCreatedDomainEvent(order.Id, order.CustomerId, order.CreatedAt));
        return order;
    }
    public Guid CustomerId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public ServiceOrderStatus Status { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public void ChangeStatus(ServiceOrderStatus status)
    {
        if (Status is ServiceOrderStatus.Completed or ServiceOrderStatus.Cancelled) throw new InvalidOperationException("A closed service order cannot change status.");
        Status = status; CompletedAt = status == ServiceOrderStatus.Completed ? DateTimeOffset.UtcNow : null;
    }
}
public sealed record ServiceOrderCreatedDomainEvent(Guid ServiceOrderId, Guid CustomerId, DateTimeOffset OccurredAt) : IDomainEvent;
