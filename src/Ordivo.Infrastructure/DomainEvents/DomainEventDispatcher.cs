using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Ordivo.SharedKernel.Domain;

namespace Ordivo.Infrastructure.DomainEvents;

internal sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlersType = typeof(IEnumerable<>).MakeGenericType(handlerType);
            var handlers = (IEnumerable)serviceProvider.GetRequiredService(handlersType);

            foreach (IDomainEventHandler handler in handlers)
                await handler.Handle(domainEvent, cancellationToken);
        }
    }
}
