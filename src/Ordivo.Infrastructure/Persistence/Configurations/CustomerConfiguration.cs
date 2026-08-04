using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordivo.Domain.Customers;

namespace Ordivo.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Id).ValueGeneratedNever();
        builder.Property(customer => customer.TenantId).IsRequired();
        builder.Property(customer => customer.Name).HasMaxLength(120).IsRequired();
        builder.Property(customer => customer.Document).HasMaxLength(20).IsRequired();
        builder.Property(customer => customer.Phone).HasMaxLength(20).IsRequired();
        builder.Property(customer => customer.Email).HasMaxLength(254);
        builder.Property(customer => customer.CreatedAt).IsRequired();
        builder.Property(customer => customer.UpdatedAt);
        builder.Property(customer => customer.CreatedByName).HasMaxLength(120).IsRequired();
        builder.Property(customer => customer.UpdatedByName).HasMaxLength(120);
        builder.HasIndex(customer => new { customer.TenantId, customer.Document }).IsUnique();
        builder.HasOne<Ordivo.Domain.Tenants.Tenant>().WithMany().HasForeignKey(customer => customer.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(customer => customer.DomainEvents);
    }
}
