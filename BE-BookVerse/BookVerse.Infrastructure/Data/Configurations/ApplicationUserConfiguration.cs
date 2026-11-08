using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        builder.Property(x => x.FullName).HasMaxLength(256);
        builder.Property(x => x.AvatarUrl).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.Property(x => x.VipTier).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.CustomerStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.LifetimeSpend).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => new { x.IsActive, x.LockoutEnd })
            .HasDatabaseName("IX_Users_Active_Lockout");
        builder.HasIndex(x => new { x.IsActive, x.IsDeleted })
            .HasDatabaseName("IX_Users_Active_Deleted");
        builder.HasIndex(x => x.VipTier).HasDatabaseName("IX_Users_VipTier");
        builder.HasIndex(x => x.ProvinceId).HasDatabaseName("IX_Users_ProvinceId");
        builder.HasIndex(x => x.CommuneId).HasDatabaseName("IX_Users_CommuneId");

        builder.HasOne(x => x.Province)
            .WithMany()
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Commune)
            .WithMany()
            .HasForeignKey(x => x.CommuneId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
