using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class PurchaseOrderDetailConfiguration : IEntityTypeConfiguration<PurchaseOrderDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderDetail> builder)
    {
        builder.ToTable("PurchaseOrderDetails");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CreatedBy).HasMaxLength(100);

        builder.HasIndex(x => x.PurchaseOrderId).HasDatabaseName("IX_PurchaseOrderDetails_PurchaseOrderId");
        builder.HasIndex(x => x.BookId).HasDatabaseName("IX_PurchaseOrderDetails_BookId");

        builder.HasOne(x => x.PurchaseOrder)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Book)
            .WithMany()
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
