using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.ToTable("OrderDetails");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UnitType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.BookTitle).HasMaxLength(500);
        builder.Property(x => x.IsbnSnapshot).HasMaxLength(20);
        builder.Property(x => x.AuthorName).HasMaxLength(256);
        builder.Property(x => x.PublisherName).HasMaxLength(256);
        builder.Property(x => x.CoverImageUrl).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);

        builder.HasIndex(x => x.OrderId).HasDatabaseName("IX_OrderDetails_OrderId");
        builder.HasIndex(x => x.BookId).HasDatabaseName("IX_OrderDetails_BookId");

        builder.HasOne(x => x.Order)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Book)
            .WithMany(x => x.OrderDetails)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
