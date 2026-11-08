using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CouponCode).HasMaxLength(50);
        builder.Property(x => x.OrderStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ShippingAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.FinalAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ShippingAddress).HasMaxLength(500);
        builder.Property(x => x.TrackingCode).HasMaxLength(100);
        builder.Property(x => x.CustomerNote).HasMaxLength(1000);
        builder.Property(x => x.StaffNote).HasMaxLength(1000);
        builder.Property(x => x.CancelReason).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("IX_Orders_Code");
        builder.HasIndex(x => x.CustomerId).HasDatabaseName("IX_Orders_CustomerId");
        builder.HasIndex(x => x.StaffId).HasDatabaseName("IX_Orders_StaffId");
        builder.HasIndex(x => new { x.OrderStatus, x.IsDeleted }).HasDatabaseName("IX_Orders_Status_Deleted");
        builder.HasIndex(x => x.OrderDate).HasDatabaseName("IX_Orders_OrderDate");
        builder.HasIndex(x => x.CouponId).HasDatabaseName("IX_Orders_CouponId");

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Staff)
            .WithMany()
            .HasForeignKey(x => x.StaffId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Coupon)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Province)
            .WithMany()
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Commune)
            .WithMany()
            .HasForeignKey(x => x.CommuneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
