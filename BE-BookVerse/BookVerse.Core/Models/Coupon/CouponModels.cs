using BookVerse.Core.Constants;

namespace BookVerse.Core.Models.Coupon;

public class CouponSearchRequest
{
    public string? Keyword { get; set; }
    public CouponStatus? Status { get; set; }
    public DiscountType? DiscountType { get; set; }
    public CouponAudience? Audience { get; set; }
    public bool? IsDeleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CouponListItemResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public int PerUserUsageLimit { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CouponAudience Audience { get; set; }
    public CouponStatus Status { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CouponDetailResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public int PerUserUsageLimit { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CouponAudience Audience { get; set; }
    public CouponStatus Status { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public byte[]? RowVersion { get; set; }
    public Guid? CampaignId { get; set; }
    public string? CampaignName { get; set; }
}

public class CreateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; } = DiscountType.Percent;
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int UsageLimit { get; set; }
    public int PerUserUsageLimit { get; set; } = 1;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CouponAudience Audience { get; set; } = CouponAudience.Public;
    public CouponStatus Status { get; set; } = CouponStatus.Draft;
    public Guid? CampaignId { get; set; }
}

public class UpdateCouponRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DiscountType? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? MinOrderValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int? PerUserUsageLimit { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public CouponAudience? Audience { get; set; }
    public CouponStatus? Status { get; set; }
    public Guid? CampaignId { get; set; }
    public byte[]? RowVersion { get; set; }
}

/// <summary>FE: GET /coupons/validate/{code} - tra ve thong tin voucher kèm so tien giam.</summary>
public class ValidateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public decimal OrderAmount { get; set; }
}

public class ValidateCouponResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid CouponId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal MinOrderValue { get; set; }
    public CouponDetailResponse? Coupon { get; set; }
}

public class CampaignSearchRequest
{
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CampaignListItemResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int CouponCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CampaignDetailResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyCollection<CouponListItemResponse> Coupons { get; set; } = [];
}

public class CreateCampaignRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCampaignRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}
