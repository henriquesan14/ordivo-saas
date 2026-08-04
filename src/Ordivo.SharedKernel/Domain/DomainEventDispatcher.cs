namespace Ordivo.SharedKernel.Domain;

public interface IDomainEventHandler
{
    Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

public interface IDomainEventHandler<in TDomainEvent> : IDomainEventHandler
    where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default);

    Task IDomainEventHandler.Handle(IDomainEvent domainEvent, CancellationToken cancellationToken) =>
        Handle((TDomainEvent)domainEvent, cancellationToken);
}

public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default);
}
