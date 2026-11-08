using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.DiscountType).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.DiscountValue).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MinOrderValue).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MaxDiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Audience).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("IX_Coupons_Code");
        builder.HasIndex(x => new { x.Status, x.IsDeleted }).HasDatabaseName("IX_Coupons_Status_Deleted");
        builder.HasIndex(x => new { x.StartDate, x.EndDate }).HasDatabaseName("IX_Coupons_DateRange");

        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.Coupons)
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
