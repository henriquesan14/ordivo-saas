using Ordivo.SharedKernel.Domain;

namespace Ordivo.Domain.ServiceOrders;

public enum ServiceOrderStatus { Open, InProgress, Completed, Cancelled }

public sealed class ServiceOrder : AggregateRoot<Guid>, ITenantEntity
{
    private readonly List<ServiceOrderStatusHistory> _statusHistory = [];
    private readonly List<ServiceOrderComment> _comments = [];
    private readonly List<ServiceOrderAttachment> _attachments = [];
    private ServiceOrder(Guid id) : base(id) { }

    public static ServiceOrder Create(Guid tenantId, long sequence, Guid customerId, string title,
        string description, decimal price, Guid? assignedUserId, DateTimeOffset? scheduledAt,
        string changedByName, DateTimeOffset now)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant is required.", nameof(tenantId));
        if (customerId == Guid.Empty) throw new ArgumentException("Customer is required.", nameof(customerId));
        var order = new ServiceOrder(Guid.NewGuid())
        {
            TenantId = tenantId,
            Number = $"OS-{now.Year}-{sequence:000000}",
            Status = ServiceOrderStatus.Open
        };
        order.Update(customerId, title, description, price, assignedUserId, scheduledAt);
        order._statusHistory.Add(ServiceOrderStatusHistory.Create(order.Id, ServiceOrderStatus.Open, changedByName, now));
        order.Raise(new ServiceOrderCreatedDomainEvent(order.Id, order.CustomerId, now));
        return order;
    }

    public Guid CustomerId { get; private set; }
    public Guid TenantId { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public ServiceOrderStatus Status { get; private set; }
    public Guid? AssignedUserId { get; private set; }
    public DateTimeOffset? ScheduledAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public IReadOnlyCollection<ServiceOrderStatusHistory> StatusHistory => _statusHistory;
    public IReadOnlyCollection<ServiceOrderComment> Comments => _comments;
    public IReadOnlyCollection<ServiceOrderAttachment> Attachments => _attachments;

    public void Update(Guid customerId, string title, string description, decimal price,
        Guid? assignedUserId, DateTimeOffset? scheduledAt)
    {
        if (customerId == Guid.Empty) throw new ArgumentException("Customer is required.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        CustomerId = customerId;
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        AssignedUserId = assignedUserId;
        ScheduledAt = scheduledAt;
    }

    public void ChangeStatus(ServiceOrderStatus status, string changedByName, DateTimeOffset now, string? note = null)
    {
        if (Status is ServiceOrderStatus.Completed or ServiceOrderStatus.Cancelled)
            throw new InvalidOperationException("A closed service order cannot change status.");
        if (Status == status) return;
        Status = status;
        CompletedAt = status == ServiceOrderStatus.Completed ? now : null;
        _statusHistory.Add(ServiceOrderStatusHistory.Create(Id, status, changedByName, now, note));
    }

    public void AddComment(Guid userId, string userName, string text, DateTimeOffset now) =>
        _comments.Add(ServiceOrderComment.Create(Id, userId, userName, text, now));

    public void AddAttachment(Guid userId, string userName, string fileName, string contentType,
        long size, string storageKey, DateTimeOffset now) =>
        _attachments.Add(ServiceOrderAttachment.Create(Id, userId, userName, fileName, contentType, size, storageKey, now));
}

public sealed class ServiceOrderStatusHistory
{
    private ServiceOrderStatusHistory() { }
    public Guid Id { get; private set; }
    public Guid ServiceOrderId { get; private set; }
    public ServiceOrderStatus Status { get; private set; }
    public string ChangedByName { get; private set; } = string.Empty;
    public string? Note { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    internal static ServiceOrderStatusHistory Create(Guid orderId, ServiceOrderStatus status, string by, DateTimeOffset at, string? note = null) =>
        new() { Id = Guid.NewGuid(), ServiceOrderId = orderId, Status = status, ChangedByName = by, ChangedAt = at, Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim() };
}

public sealed class ServiceOrderComment
{
    private ServiceOrderComment() { }
    public Guid Id { get; private set; }
    public Guid ServiceOrderId { get; private set; }
    public Guid UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    internal static ServiceOrderComment Create(Guid orderId, Guid userId, string name, string text, DateTimeOffset at) =>
        new() { Id = Guid.NewGuid(), ServiceOrderId = orderId, UserId = userId, UserName = name, Text = string.IsNullOrWhiteSpace(text) ? throw new ArgumentException("Comment is required.") : text.Trim(), CreatedAt = at };
}

public sealed class ServiceOrderAttachment
{
    private ServiceOrderAttachment() { }
    public Guid Id { get; private set; }
    public Guid ServiceOrderId { get; private set; }
    public Guid UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long Size { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    internal static ServiceOrderAttachment Create(Guid orderId, Guid userId, string name, string fileName, string contentType, long size, string key, DateTimeOffset at)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(key) || size < 0) throw new ArgumentException("Valid attachment metadata is required.");
        return new() { Id = Guid.NewGuid(), ServiceOrderId = orderId, UserId = userId, UserName = name, FileName = fileName.Trim(), ContentType = contentType.Trim(), Size = size, StorageKey = key.Trim(), CreatedAt = at };
    }
}

public sealed record ServiceOrderCreatedDomainEvent(Guid ServiceOrderId, Guid CustomerId, DateTimeOffset OccurredAt) : IDomainEvent;
