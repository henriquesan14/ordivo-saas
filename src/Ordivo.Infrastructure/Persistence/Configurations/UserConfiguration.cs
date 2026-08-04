using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordivo.Domain.Users;

namespace Ordivo.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.TenantId).IsRequired();
        builder.Property(user => user.Name).HasMaxLength(120).IsRequired();
        builder.Property(user => user.Email).HasMaxLength(254).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(user => user.Role).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(user => user.IsActive).IsRequired();
        builder.Property(user => user.CreatedAt).IsRequired();
        builder.Property(user => user.UpdatedAt);
        builder.Property(user => user.CreatedByName).HasMaxLength(120).IsRequired();
        builder.Property(user => user.UpdatedByName).HasMaxLength(120);
        builder.HasIndex(user => user.Email).IsUnique();
        builder.HasIndex(user => user.TenantId);
        builder.HasOne<Ordivo.Domain.Tenants.Tenant>().WithMany().HasForeignKey(user => user.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(user => user.DomainEvents);
    }
}
