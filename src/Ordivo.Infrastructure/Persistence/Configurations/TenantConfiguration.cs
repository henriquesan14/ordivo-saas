using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordivo.Domain.Tenants;

namespace Ordivo.Infrastructure.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(tenant => tenant.Id);
        builder.Property(tenant => tenant.Id).ValueGeneratedNever();
        builder.Property(tenant => tenant.Name).HasMaxLength(160).IsRequired();
        builder.Property(tenant => tenant.IsActive).IsRequired();
        builder.Property(tenant => tenant.CreatedAt).IsRequired();
        builder.Property(tenant => tenant.UpdatedAt);
        builder.Property(tenant => tenant.CreatedByName).HasMaxLength(120).IsRequired();
        builder.Property(tenant => tenant.UpdatedByName).HasMaxLength(120);
        builder.Ignore(tenant => tenant.DomainEvents);
    }
}
