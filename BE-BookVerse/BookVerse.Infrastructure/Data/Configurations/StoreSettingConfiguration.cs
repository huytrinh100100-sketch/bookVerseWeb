using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class StoreSettingConfiguration : IEntityTypeConfiguration<StoreSetting>
{
    public void Configure(EntityTypeBuilder<StoreSetting> builder)
    {
        builder.ToTable("StoreSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(4000);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.GroupKey).HasMaxLength(50);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(x => x.Key).IsUnique().HasDatabaseName("IX_StoreSettings_Key");
    }
}
