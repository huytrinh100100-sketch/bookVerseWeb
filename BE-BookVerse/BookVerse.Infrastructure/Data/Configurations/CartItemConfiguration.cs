using BookVerse.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookVerse.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitType).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => new { x.CustomerId, x.BookId, x.UnitType })
            .IsUnique()
            .HasDatabaseName("IX_CartItems_Customer_Book_UnitType");

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.CartItems)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Book)
            .WithMany(x => x.CartItems)
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
