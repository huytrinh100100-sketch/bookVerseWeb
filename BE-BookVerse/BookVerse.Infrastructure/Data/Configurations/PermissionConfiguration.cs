using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.GroupCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.GroupName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("IX_Permissions_Code");
        builder.HasIndex(x => x.GroupCode).HasDatabaseName("IX_Permissions_GroupCode");
        builder.HasIndex(x => x.SortOrder).HasDatabaseName("IX_Permissions_SortOrder");
    }
}
