using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Ordivo.SharedKernel.Domain;
using Ordivo.Application.Abstractions.Authentication;
using Ordivo.Infrastructure.Persistence.Extensions;

namespace Ordivo.Infrastructure.Persistence.Interceptors;

internal sealed class AuditableEntityInterceptor(TimeProvider timeProvider, IUserContext userContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null) return;
        var now = timeProvider.GetUtcNow();
        var userName = userContext.IsAuthenticated && !string.IsNullOrWhiteSpace(userContext.Name)
            ? userContext.Name
            : "System";

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(IAuditableEntity.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(IAuditableEntity.UpdatedAt)).CurrentValue = null;
                entry.Property(nameof(IAuditableEntity.CreatedByName)).CurrentValue = userName;
                entry.Property(nameof(IAuditableEntity.UpdatedByName)).CurrentValue = null;
                entry.Property("Version").CurrentValue = Guid.NewGuid();
            }
            else if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                entry.Property(nameof(IAuditableEntity.CreatedByName)).IsModified = false;
                entry.Property(nameof(IAuditableEntity.UpdatedAt)).CurrentValue = now;
                entry.Property(nameof(IAuditableEntity.UpdatedByName)).CurrentValue = userName;
                entry.Property("Version").CurrentValue = Guid.NewGuid();
            }
        }
    }
}
