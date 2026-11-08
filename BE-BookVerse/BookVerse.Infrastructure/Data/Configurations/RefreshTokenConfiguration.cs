using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.RevokedReason).HasMaxLength(500);
        builder.Property(x => x.ReplacedByTokenHash).HasMaxLength(128);
        builder.Property(x => x.CreatedByIpAddress).HasMaxLength(64);
        builder.Property(x => x.RevokedByIpAddress).HasMaxLength(64);

        builder.HasIndex(x => x.TokenHash).IsUnique().HasDatabaseName("IX_RefreshTokens_TokenHash");
        builder.HasIndex(x => new { x.UserId, x.ExpiresAt }).HasDatabaseName("IX_RefreshTokens_User_Expires");
        builder.HasIndex(x => x.ExpiresAt).HasDatabaseName("IX_RefreshTokens_ExpiresAt");

        builder.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
