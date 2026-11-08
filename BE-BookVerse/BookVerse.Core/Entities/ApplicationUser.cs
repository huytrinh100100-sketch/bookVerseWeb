using BookVerse.Core.Constants;
using Microsoft.AspNetCore.Identity;

namespace BookVerse.Core.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public string? AvatarUrl { get; set; }

    // ----- Hồ sơ khách hàng (Customer) -----
    public Guid? ProvinceId { get; set; }
    public Guid? CommuneId { get; set; }
    public string? Address { get; set; }
    public DateTime? Birthday { get; set; }
    public VipTier VipTier { get; set; } = VipTier.Standard;
    public CustomerStatus CustomerStatus { get; set; } = CustomerStatus.Active;
    public decimal LifetimeSpend { get; set; }
    public int RewardPoints { get; set; }
    public Guid? PreferredCategoryId { get; set; }
    public string? Note { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public string? LastLoginLocation { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<CustomerAddress> Addresses { get; set; } = [];
    public ICollection<WishlistItem> WishlistItems { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public Province? Province { get; set; }
    public Commune? Commune { get; set; }
}
