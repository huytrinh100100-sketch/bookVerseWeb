using BookVerse.Core.Entities;
using BookVerse.Core.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // ----- Identity / phân quyền -----
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ----- Danh mục động + địa giới -----
    public DbSet<CommonCategory> CommonCategories => Set<CommonCategory>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<Commune> Communes => Set<Commune>();

    // ----- Catalog sách -----
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookCopy> BookCopies => Set<BookCopy>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderDetail> PurchaseOrderDetails => Set<PurchaseOrderDetail>();

    // ----- Bán hàng -----
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Carrier> Carriers => Set<Carrier>();
    public DbSet<PaymentGateway> PaymentGateways => Set<PaymentGateway>();

    // ----- Khách hàng & nội dung -----
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<StoreSetting> StoreSettings => Set<StoreSetting>();

    public DbSet<AccountSearchRow> AccountSearchRows => Set<AccountSearchRow>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        builder.Entity<AccountSearchRow>(entity =>
        {
            entity.HasNoKey();
            entity.ToView(null);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Tự động gắn CreatedAt / UpdatedAt cho mọi entity có các trường này.
    /// Đồng thời đặt RowVersion (optimistic concurrency) cho entity cần nó.
    /// </summary>
    private void ApplyAuditTimestamps()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            var createdAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
            if (createdAtProperty is not null && createdAtProperty.Metadata.ClrType == typeof(DateTime))
            {
                var currentValue = createdAtProperty.CurrentValue as DateTime?;
                if (entry.State == EntityState.Added && (!currentValue.HasValue || currentValue.Value == default))
                {
                    createdAtProperty.CurrentValue = utcNow;
                }
            }

            var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
            if (updatedAtProperty is not null && updatedAtProperty.Metadata.ClrType == typeof(DateTime?))
            {
                updatedAtProperty.CurrentValue = utcNow;
            }
        }
    }
}
