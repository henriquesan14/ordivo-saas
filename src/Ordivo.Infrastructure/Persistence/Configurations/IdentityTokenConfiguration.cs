using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordivo.Domain.Authentication;

namespace Ordivo.Infrastructure.Persistence.Configurations;

internal sealed class IdentityTokenConfiguration : IEntityTypeConfiguration<IdentityToken>
{
    public void Configure(EntityTypeBuilder<IdentityToken> builder)
    {
        builder.ToTable("identity_tokens");
        builder.HasKey(token => token.Id);
        builder.Property(token => token.Id).ValueGeneratedNever();
        builder.Property(token => token.Email).HasMaxLength(254).IsRequired();
        builder.Property(token => token.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(token => token.ExpiresAt).IsRequired();
        builder.Property(token => token.CreatedAt).IsRequired();
        builder.Property(token => token.CreatedByName).HasMaxLength(120).IsRequired();
        builder.Property(token => token.UpdatedByName).HasMaxLength(120);
        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => new { token.UserId, token.Type });
        builder.Ignore(token => token.DomainEvents);
    }
}
