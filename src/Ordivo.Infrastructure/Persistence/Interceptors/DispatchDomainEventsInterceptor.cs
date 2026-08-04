using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ordivo.SharedKernel.Domain;

namespace Ordivo.Infrastructure.Persistence.Interceptors;

internal sealed class DispatchDomainEventsInterceptor(IDomainEventDispatcher dispatcher) : SaveChangesInterceptor
{
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        DispatchAndClearAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await DispatchAndClearAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchAndClearAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is null) return;

        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot<Guid>>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToArray();

        if (aggregates.Length == 0) return;

        var domainEvents = aggregates
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToArray();

        await dispatcher.DispatchAsync(domainEvents, cancellationToken);

        foreach (var aggregate in aggregates)
            aggregate.ClearDomainEvents();
    }
}
