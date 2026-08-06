namespace Ordivo.SharedKernel.Domain;
public interface IDomainEvent { DateTimeOffset OccurredAt { get; } }
public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset? UpdatedAt { get; }
    string CreatedByName { get; }
    string? UpdatedByName { get; }
}
public interface ITenantEntity
{
    Guid TenantId { get; }
}
public abstract class AggregateRoot<TId> : Entity<TId>, IAuditableEntity where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];
    protected AggregateRoot(TId id) : base(id) => CreatedAt = DateTimeOffset.UtcNow;
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset? UpdatedAt { get; protected set; }
    public string CreatedByName { get; protected set; } = string.Empty;
    public string? UpdatedByName { get; protected set; }
    public Guid Version { get; protected set; } = Guid.NewGuid();
    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
