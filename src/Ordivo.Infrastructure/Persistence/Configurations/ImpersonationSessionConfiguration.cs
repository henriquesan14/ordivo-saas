using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordivo.Domain.Impersonation;

namespace Ordivo.Infrastructure.Persistence.Configurations;
internal sealed class ImpersonationSessionConfiguration : IEntityTypeConfiguration<ImpersonationSession>
{
    public void Configure(EntityTypeBuilder<ImpersonationSession> b)
    {
        b.ToTable("impersonation_sessions"); b.HasKey(x=>x.Id); b.Property(x=>x.Id).ValueGeneratedNever();
        b.Property(x=>x.Reason).HasMaxLength(500).IsRequired(); b.HasIndex(x=>new{x.PlatformUserId,x.EndedAt}); b.HasIndex(x=>x.ExpiresAt);
        b.HasOne<Ordivo.Domain.PlatformUsers.PlatformUser>().WithMany().HasForeignKey(x=>x.PlatformUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Ordivo.Domain.Tenants.Tenant>().WithMany().HasForeignKey(x=>x.TenantId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Ordivo.Domain.Users.User>().WithMany().HasForeignKey(x=>x.TargetUserId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x=>x.Version).IsConcurrencyToken(); b.Ignore(x=>x.DomainEvents);
    }
}
