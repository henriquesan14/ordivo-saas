using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ordivo.Infrastructure.Persistence.Configurations;
internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b){b.ToTable("outbox_messages");b.HasKey(x=>x.Id);b.Property(x=>x.Type).HasMaxLength(500);b.Property(x=>x.Payload).HasColumnType("jsonb");b.Property(x=>x.Error).HasMaxLength(4000);b.HasIndex(x=>new{x.ProcessedAt,x.NextAttemptAt});}
}
internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> b){b.ToTable("idempotency_records");b.HasKey(x=>x.Id);b.Property(x=>x.Scope).HasMaxLength(500);b.Property(x=>x.Key).HasMaxLength(200);b.Property(x=>x.ContentType).HasMaxLength(200);b.HasIndex(x=>new{x.Scope,x.Key}).IsUnique();b.HasIndex(x=>x.ExpiresAt);}
}
