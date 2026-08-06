using Microsoft.EntityFrameworkCore;
using Ordivo.Application.Abstractions.Persistence;
using Ordivo.Domain.Impersonation;
using Ordivo.Domain.Users;

namespace Ordivo.Infrastructure.Persistence.Repositories;
internal sealed class ImpersonationRepository(OrdivoDbContext db) : IImpersonationRepository
{
    public Task<User?> GetTargetUserAsync(Guid tenantId, Guid? userId, CancellationToken ct) => db.Users.IgnoreQueryFilters()
        .Where(x=>x.TenantId==tenantId && x.IsActive && x.EmailVerifiedAt!=null && (!userId.HasValue || x.Id==userId))
        .OrderBy(x=>x.Role==UserRole.Owner?0:x.Role==UserRole.Admin?1:2).FirstOrDefaultAsync(ct);
    public Task<ImpersonationSession?> GetSessionAsync(Guid id, CancellationToken ct) => db.ImpersonationSessions.SingleOrDefaultAsync(x=>x.Id==id,ct);
    public Task AddAsync(ImpersonationSession session,CancellationToken ct)=>db.ImpersonationSessions.AddAsync(session,ct).AsTask();
}
