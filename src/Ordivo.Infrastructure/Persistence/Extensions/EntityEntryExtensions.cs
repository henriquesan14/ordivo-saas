using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ordivo.Infrastructure.Persistence.Extensions;

internal static class EntityEntryExtensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(reference =>
            reference.TargetEntry is not null &&
            reference.TargetEntry.Metadata.IsOwned() &&
            reference.TargetEntry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
}
