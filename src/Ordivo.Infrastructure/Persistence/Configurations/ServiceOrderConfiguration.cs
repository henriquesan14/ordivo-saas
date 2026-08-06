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
        builder.Property(order => order.Number).HasMaxLength(32).IsRequired();
        builder.Property(order => order.Title).HasMaxLength(160).IsRequired();
        builder.Property(order => order.Description).HasMaxLength(4000).IsRequired();
        builder.Property(order => order.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(order => order.AssignedUserId);
        builder.Property(order => order.ScheduledAt);
        builder.Property(order => order.CompletedAt);
        builder.Property(order => order.CreatedAt).IsRequired();
        builder.Property(order => order.UpdatedAt);
        builder.Property(order => order.CreatedByName).HasMaxLength(120).IsRequired();
        builder.Property(order => order.UpdatedByName).HasMaxLength(120);
        builder.HasIndex(order => new { order.CustomerId, order.CreatedAt });
        builder.HasIndex(order => new { order.TenantId, order.CreatedAt });
        builder.HasIndex(order => order.Number).IsUnique();
        builder.HasOne<Ordivo.Domain.Tenants.Tenant>().WithMany().HasForeignKey(order => order.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Customer>().WithMany().HasForeignKey(order => order.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Ordivo.Domain.Users.User>().WithMany().HasForeignKey(order => order.AssignedUserId).OnDelete(DeleteBehavior.Restrict);
        builder.OwnsMany(order => order.StatusHistory, history =>
        {
            history.ToTable("service_order_status_history"); history.HasKey(item => item.Id); history.Property(item => item.Id).ValueGeneratedNever();
            history.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
            history.Property(item => item.ChangedByName).HasMaxLength(120); history.Property(item => item.Note).HasMaxLength(1000);
            history.WithOwner().HasForeignKey(item => item.ServiceOrderId);
        });
        builder.OwnsMany(order => order.Comments, comment =>
        {
            comment.ToTable("service_order_comments"); comment.HasKey(item => item.Id); comment.Property(item => item.Id).ValueGeneratedNever();
            comment.Property(item => item.UserName).HasMaxLength(120); comment.Property(item => item.Text).HasMaxLength(2000);
            comment.WithOwner().HasForeignKey(item => item.ServiceOrderId);
        });
        builder.OwnsMany(order => order.Attachments, attachment =>
        {
            attachment.ToTable("service_order_attachments"); attachment.HasKey(item => item.Id); attachment.Property(item => item.Id).ValueGeneratedNever();
            attachment.Property(item => item.UserName).HasMaxLength(120); attachment.Property(item => item.FileName).HasMaxLength(255);
            attachment.Property(item => item.ContentType).HasMaxLength(150); attachment.Property(item => item.StorageKey).HasMaxLength(1000);
            attachment.WithOwner().HasForeignKey(item => item.ServiceOrderId);
        });
        builder.Ignore(order => order.DomainEvents);
    }
}
