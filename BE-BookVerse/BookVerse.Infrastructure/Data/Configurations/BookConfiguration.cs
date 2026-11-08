using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Subtitle).HasMaxLength(500);
        builder.Property(x => x.Slug).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Isbn).HasMaxLength(20);
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.CoverImageUrl).HasMaxLength(500);
        builder.Property(x => x.Language).HasMaxLength(50);
        builder.Property(x => x.Format).HasMaxLength(50);
        builder.Property(x => x.Weight).HasMaxLength(50);
        builder.Property(x => x.Dimensions).HasMaxLength(50);
        builder.Property(x => x.PriceForSell).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PriceForRent).HasColumnType("decimal(18,2)");
        builder.Property(x => x.AverageRating).HasColumnType("decimal(3,2)");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("IX_Books_Code");
        builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("IX_Books_Slug");
        builder.HasIndex(x => x.Isbn).HasDatabaseName("IX_Books_Isbn");
        builder.HasIndex(x => x.CategoryId).HasDatabaseName("IX_Books_CategoryId");
        builder.HasIndex(x => x.AuthorId).HasDatabaseName("IX_Books_AuthorId");
        builder.HasIndex(x => x.PublisherId).HasDatabaseName("IX_Books_PublisherId");
        builder.HasIndex(x => new { x.Status, x.IsActive, x.IsDeleted }).HasDatabaseName("IX_Books_Status_Active_Deleted");
        builder.HasIndex(x => x.IsFeatured).HasDatabaseName("IX_Books_IsFeatured");
        builder.HasIndex(x => x.Title).HasDatabaseName("IX_Books_Title");

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Author)
            .WithMany(x => x.Books)
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Publisher)
            .WithMany(x => x.Books)
            .HasForeignKey(x => x.PublisherId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
