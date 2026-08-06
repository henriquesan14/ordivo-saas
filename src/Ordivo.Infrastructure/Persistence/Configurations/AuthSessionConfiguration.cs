using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordivo.Domain.Authentication;

namespace Ordivo.Infrastructure.Persistence.Configurations;

internal sealed class AuthSessionConfiguration : IEntityTypeConfiguration<AuthSession>
{
    public void Configure(EntityTypeBuilder<AuthSession> builder)
    {
        builder.ToTable("auth_sessions");
        builder.HasKey(session => session.Id);
        builder.Property(session => session.Id).ValueGeneratedNever();
        builder.Property(session => session.UserId).IsRequired();
        builder.Property(session => session.SubjectType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(session => session.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(session => session.ExpiresAt).IsRequired();
        builder.Property(session => session.Version).IsConcurrencyToken().IsRequired();
        builder.Property(session => session.FamilyId).IsRequired();
        builder.Property(session => session.CreatedAt).IsRequired();
        builder.Property(session => session.CreatedByName).HasMaxLength(120).IsRequired();
        builder.Property(session => session.UpdatedByName).HasMaxLength(120);
        builder.HasIndex(session => session.TokenHash).IsUnique();
        builder.HasIndex(session => new { session.UserId, session.SubjectType });
        builder.HasIndex(session => session.FamilyId);
        builder.Ignore(session => session.DomainEvents);
    }
}
