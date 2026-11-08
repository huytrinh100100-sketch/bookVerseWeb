using BookVerse.Core.Constants;

namespace BookVerse.Core.Entities;

public class Coupon
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DiscountType DiscountType { get; set; } = DiscountType.Percent;
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public int PerUserUsageLimit { get; set; } = 1;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CouponAudience Audience { get; set; } = CouponAudience.Public;
    public CouponStatus Status { get; set; } = CouponStatus.Draft;

    /// <summary>Thuộc chiến dịch nào (null = voucher độc lập).</summary>
    public Guid? CampaignId { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public byte[]? RowVersion { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
    public Campaign? Campaign { get; set; }
}
