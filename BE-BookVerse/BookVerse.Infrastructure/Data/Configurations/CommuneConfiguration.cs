using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class CommuneConfiguration : IEntityTypeConfiguration<Commune>
{
    public void Configure(EntityTypeBuilder<Commune> builder)
    {
        builder.ToTable("Communes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Kind).HasMaxLength(50);
        builder.Property(x => x.RegionCode).HasMaxLength(20);
        builder.Property(x => x.CreatedBy).HasMaxLength(100);
        builder.Property(x => x.UpdatedBy).HasMaxLength(100);
        builder.Property(x => x.DeletedBy).HasMaxLength(100);

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("IX_Communes_Code");
        builder.HasIndex(x => x.ProvinceId).HasDatabaseName("IX_Communes_ProvinceId");
        builder.HasIndex(x => new { x.IsActive, x.IsDeleted }).HasDatabaseName("IX_Communes_Active_Deleted");

        builder.HasOne(x => x.Province)
            .WithMany(x => x.Communes)
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
