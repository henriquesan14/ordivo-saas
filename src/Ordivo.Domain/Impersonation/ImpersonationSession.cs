using Ordivo.SharedKernel.Domain;

namespace Ordivo.Domain.Impersonation;

public sealed class ImpersonationSession : AggregateRoot<Guid>
{
    private ImpersonationSession(Guid id) : base(id) { }
    public Guid PlatformUserId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid TargetUserId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public bool IsActive(DateTimeOffset now) => EndedAt is null && ExpiresAt > now;
    public static ImpersonationSession Start(Guid platformUserId, Guid tenantId, Guid targetUserId, string reason, DateTimeOffset now, TimeSpan duration) =>
        new(Guid.NewGuid()) { PlatformUserId = platformUserId, TenantId = tenantId, TargetUserId = targetUserId, Reason = reason.Trim(), StartedAt = now, ExpiresAt = now.Add(duration) };
    public void End(DateTimeOffset now) { if (EndedAt is null) EndedAt = now; }
}
