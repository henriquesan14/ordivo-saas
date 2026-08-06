using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ordivo.SharedKernel.Domain;

namespace Ordivo.Infrastructure.Persistence.Interceptors;
internal sealed class OutboxInterceptor(TimeProvider time) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData data, InterceptionResult<int> result){Capture(data.Context);return result;}
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data,InterceptionResult<int> result,CancellationToken ct=default){Capture(data.Context);return ValueTask.FromResult(result);}
    private void Capture(DbContext? context)
    {
        if(context is null)return;
        var aggregates=context.ChangeTracker.Entries<AggregateRoot<Guid>>().Select(x=>x.Entity).Where(x=>x.DomainEvents.Count>0).ToArray();
        foreach(var domainEvent in aggregates.SelectMany(x=>x.DomainEvents)) context.Set<OutboxMessage>().Add(new(){Type=domainEvent.GetType().AssemblyQualifiedName!,Payload=JsonSerializer.Serialize(domainEvent,domainEvent.GetType()),OccurredAt=time.GetUtcNow()});
    }
}
