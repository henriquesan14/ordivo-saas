using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordivo.Domain.Customers;
using Ordivo.Domain.ServiceOrders;

namespace Ordivo.Infrastructure.Persistence.Configurations;

internal sealed class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.ToTable("service_orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedNever();
        builder.Property(order => order.TenantId).IsRequired();
        builder.Property(order => order.Title).HasMaxLength(160).IsRequired();
        builder.Property(order => order.Description).HasMaxLength(4000).IsRequired();
        builder.Property(order => order.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(order => order.CreatedAt).IsRequired();
        builder.Property(order => order.UpdatedAt);
        builder.Property(order => order.CreatedByName).HasMaxLength(120).IsRequired();
        builder.Property(order => order.UpdatedByName).HasMaxLength(120);
        builder.HasIndex(order => new { order.CustomerId, order.CreatedAt });
        builder.HasIndex(order => new { order.TenantId, order.CreatedAt });
        builder.HasOne<Ordivo.Domain.Tenants.Tenant>().WithMany().HasForeignKey(order => order.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Customer>().WithMany().HasForeignKey(order => order.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(order => order.DomainEvents);
    }
}
