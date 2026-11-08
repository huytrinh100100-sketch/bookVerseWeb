using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class CommonCategoryConfiguration : IEntityTypeConfiguration<CommonCategory>
{
    public void Configure(EntityTypeBuilder<CommonCategory> builder)
    {
        builder.ToTable("CommonCategories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(256);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.IconUrl).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);

        builder.HasIndex(x => new { x.RootId, x.Code }).IsUnique();
        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.Slug);
        builder.HasIndex(x => x.ParentId);
        builder.HasIndex(x => x.RootId);
        builder.HasIndex(x => new { x.RootId, x.IsActive, x.IsDeleted });

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Root)
            .WithMany(x => x.RootItems)
            .HasForeignKey(x => x.RootId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
