using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.ToTable("Articles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(1000);
        builder.Property(x => x.Content).HasMaxLength(8000);
        builder.Property(x => x.CoverImageUrl).HasMaxLength(500);
        builder.Property(x => x.LinkedBookIds).HasMaxLength(1000);
        builder.Property(x => x.SeoTitle).HasMaxLength(300);
        builder.Property(x => x.SeoDescription).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("IX_Articles_Code");
        builder.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("IX_Articles_Slug");
        builder.HasIndex(x => new { x.Status, x.IsDeleted }).HasDatabaseName("IX_Articles_Status_Deleted");
        builder.HasIndex(x => x.CategoryId).HasDatabaseName("IX_Articles_CategoryId");

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
