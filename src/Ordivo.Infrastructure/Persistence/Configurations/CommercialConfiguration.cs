using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordivo.Domain.Commercial;

namespace Ordivo.Infrastructure.Persistence.Configurations;

internal sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> b)
    {
        b.ToTable("plans"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Name).HasMaxLength(120).IsRequired(); b.Property(x => x.Code).HasMaxLength(60).IsRequired(); b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Price).HasPrecision(18, 2); b.Property(x => x.Currency).HasMaxLength(3).IsRequired(); b.Property(x => x.Interval).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Version).IsConcurrencyToken(); b.Ignore(x => x.DomainEvents);
    }
}
internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> b)
    {
        b.ToTable("subscriptions"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedNever(); b.HasIndex(x => x.TenantId).IsUnique();
        b.HasOne<Plan>().WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Ordivo.Domain.Tenants.Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30); b.Property(x => x.GatewayCustomerId).HasMaxLength(200); b.Property(x => x.GatewaySubscriptionId).HasMaxLength(200); b.HasIndex(x => x.GatewaySubscriptionId).IsUnique();
        b.Property(x => x.PlanName).HasMaxLength(120).IsRequired(); b.Property(x => x.PlanCode).HasMaxLength(60).IsRequired();
        b.Property(x => x.ContractPrice).HasPrecision(18, 2); b.Property(x => x.ContractCurrency).HasMaxLength(3).IsRequired(); b.Property(x => x.ContractInterval).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Version).IsConcurrencyToken(); b.Ignore(x => x.DomainEvents);
    }
}
internal sealed class BillingInvoiceConfiguration : IEntityTypeConfiguration<BillingInvoice>
{
    public void Configure(EntityTypeBuilder<BillingInvoice> b)
    {
        b.ToTable("billing_invoices"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedNever();
        b.HasIndex(x => x.GatewayInvoiceId).IsUnique(); b.Property(x => x.GatewayInvoiceId).HasMaxLength(200).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2); b.Property(x => x.Currency).HasMaxLength(3); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        b.HasOne<Subscription>().WithMany().HasForeignKey(x => x.SubscriptionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Ordivo.Domain.Tenants.Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        b.Property(x => x.Version).IsConcurrencyToken(); b.Ignore(x => x.DomainEvents);
    }
}
internal sealed class PaymentWebhookEventConfiguration : IEntityTypeConfiguration<PaymentWebhookEvent>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookEvent> b)
    {
        b.ToTable("payment_webhook_events"); b.HasKey(x => x.Id); b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Gateway).HasMaxLength(60); b.Property(x => x.ExternalEventId).HasMaxLength(200); b.Property(x => x.EventType).HasMaxLength(100); b.Property(x => x.Payload).HasColumnType("jsonb");
        b.HasIndex(x => new { x.Gateway, x.ExternalEventId }).IsUnique();
    }
}
