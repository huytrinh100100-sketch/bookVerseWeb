using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(256);
        builder.Property(x => x.Content).HasMaxLength(4000);
        builder.Property(x => x.MediaUrls).HasMaxLength(2000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Sentiment).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.ReplyContent).HasMaxLength(2000);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);

        builder.HasIndex(x => x.BookId).HasDatabaseName("IX_Reviews_BookId");
        builder.HasIndex(x => x.CustomerId).HasDatabaseName("IX_Reviews_CustomerId");
        builder.HasIndex(x => new { x.Status, x.IsDeleted }).HasDatabaseName("IX_Reviews_Status_Deleted");
        builder.HasIndex(x => x.Rating).HasDatabaseName("IX_Reviews_Rating");

        builder.HasOne(x => x.Book)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
