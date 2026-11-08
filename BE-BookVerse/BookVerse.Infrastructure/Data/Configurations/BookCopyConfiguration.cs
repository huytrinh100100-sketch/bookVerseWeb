using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class BookCopyConfiguration : IEntityTypeConfiguration<BookCopy>
{
    public void Configure(EntityTypeBuilder<BookCopy> builder)
    {
        builder.ToTable("BookCopies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Barcode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ConditionStatus).HasMaxLength(50);
        builder.Property(x => x.ShelfCode).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);

        builder.HasIndex(x => x.Barcode).IsUnique().HasDatabaseName("IX_BookCopies_Barcode");
        builder.HasIndex(x => x.BookId).HasDatabaseName("IX_BookCopies_BookId");
        builder.HasIndex(x => x.WarehouseId).HasDatabaseName("IX_BookCopies_WarehouseId");
        builder.HasIndex(x => x.Status).HasDatabaseName("IX_BookCopies_Status");
        builder.HasIndex(x => x.CurrentOrderId).HasDatabaseName("IX_BookCopies_CurrentOrderId");

        builder.HasOne(x => x.Book)
            .WithMany(x => x.Copies)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Warehouse)
            .WithMany(x => x.Copies)
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
