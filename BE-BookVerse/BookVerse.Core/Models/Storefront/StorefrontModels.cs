using BookVerse.Core.Constants;
using BookVerse.Core.Models.Category;
using BookVerse.Core.Models.Content;
using BookVerse.Core.Models.Coupon;
using BookVerse.Core.Models.Order;

namespace BookVerse.Core.Models.Storefront;

// ============================================================ CATALOG

public class CatalogSearchRequest
{
    public string? Keyword { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? AuthorId { get; set; }
    public Guid? PublisherId { get; set; }
    public UnitType? UnitType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinRating { get; set; }
    public List<string>? Languages { get; set; }
    public List<string>? Formats { get; set; }
    public bool? InStock { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

/// <summary>Shape catalog đúng theo FE (CatalogResult).</summary>
public class CatalogResult
{
    public IReadOnlyCollection<CatalogItemResponse> Items { get; set; } = [];
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public CatalogFacets Facets { get; set; } = new();
}

public class CatalogFacets
{
    public IReadOnlyCollection<CatalogFacetResponse> Categories { get; set; } = [];
    public IReadOnlyCollection<CatalogFacetResponse> Publishers { get; set; } = [];
    public IReadOnlyCollection<CatalogFacetResponse> Languages { get; set; } = [];
    public IReadOnlyCollection<CatalogFacetResponse> Formats { get; set; } = [];
    public IReadOnlyCollection<PriceRangeFacetResponse> PriceRanges { get; set; } = [];
}

public class CatalogFacetResponse
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class PriceRangeFacetResponse
{
    public string Label { get; set; } = string.Empty;
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public int Count { get; set; }
}

public class CatalogItemResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public Guid? PublisherId { get; set; }
    public string? PublisherName { get; set; }
    public string? CoverImageUrl { get; set; }
    public decimal PriceForSell { get; set; }
    public decimal PriceForRent { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public int PageCount { get; set; }
    public int TotalQuantity { get; set; }
    public string? Language { get; set; }
    public string? Format { get; set; }
    public int StockForSell { get; set; }
    public int StockForRent { get; set; }
    public decimal? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int SoldCount { get; set; }
    public BookStatus Status { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CatalogDetailResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public Guid? PublisherId { get; set; }
    public string? PublisherName { get; set; }
    public decimal PriceForSell { get; set; }
    public decimal PriceForRent { get; set; }
    public int PageCount { get; set; }
    public int? PublicationYear { get; set; }
    public string? Language { get; set; }
    public string? Format { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public int TotalQuantity { get; set; }
    public int StockForSell { get; set; }
    public int StockForRent { get; set; }
    public decimal? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int SoldCount { get; set; }
    public BookStatus Status { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public IReadOnlyCollection<ReviewListItemResponse> Reviews { get; set; } = [];
    public IReadOnlyCollection<CatalogItemResponse> RelatedBooks { get; set; } = [];
}

// ============================================================ HOME

public class HomeBannerResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string? BadgeText { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class HomeResponse
{
    public IReadOnlyCollection<HomeBannerResponse> Banners { get; set; } = [];
    public IReadOnlyCollection<CategoryTreeNodeResponse> Categories { get; set; } = [];
    public IReadOnlyCollection<CatalogItemResponse> FeaturedBooks { get; set; } = [];
    public IReadOnlyCollection<CatalogItemResponse> NewBooks { get; set; } = [];
    public IReadOnlyCollection<CatalogItemResponse> BestSellers { get; set; } = [];
    public IReadOnlyCollection<ArticleListItemResponse> Articles { get; set; } = [];
    public IReadOnlyCollection<CouponListItemResponse> Coupons { get; set; } = [];
}

// ============================================================ CART

public class CartItemRequest
{
    public Guid BookId { get; set; }
    public UnitType UnitType { get; set; } = UnitType.Sell;
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}

/// <summary>Shape giỏ hàng đúng theo FE (CartState).</summary>
public class CartResponse
{
    public IReadOnlyCollection<CartItemResponse> Items { get; set; } = [];
    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal FreeShippingThreshold { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal FinalAmount { get; set; }
    public int TotalQuantity { get; set; }
    public decimal? FreeShippingRemaining { get; set; }
}

public class CartItemResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? BookTitle { get; set; }
    public string? BookSlug { get; set; }
    public string? CoverImageUrl { get; set; }
    public UnitType UnitType { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public bool IsWishlisted { get; set; }
    public int AvailableStock { get; set; }
}

public class ApplyCouponRequest
{
    public string Code { get; set; } = string.Empty;
}

// ============================================================ CHECKOUT

public class CheckoutRequest
{
    public Guid? AddressId { get; set; }
    public string? CarrierCode { get; set; }
    public decimal? ShippingAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? CommuneId { get; set; }
    public string? RecipientName { get; set; }
    public string? PhoneNumber { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
    public string? CouponCode { get; set; }
    public string? CustomerNote { get; set; }
    public string? Note { get; set; }
    public bool SaveAddress { get; set; }
    public List<CheckoutDetailRequest> Details { get; set; } = [];
}

public class CheckoutDetailRequest
{
    public Guid BookId { get; set; }
    public UnitType UnitType { get; set; } = UnitType.Sell;
    public int Quantity { get; set; } = 1;
}

/// <summary>Shape đơn hàng của khách đúng theo FE (ShopOrderSummary).</summary>
public class CheckoutResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string OrderCode { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public int ItemCount { get; set; }
    public IReadOnlyCollection<string> PreviewTitles { get; set; } = [];
    public string? TrackingCode { get; set; }
    public string? PaymentUrl { get; set; }
    public string Message { get; set; } = string.Empty;
}

// ============================================================ WISHLIST / PROFILE / ADDRESS

public class WishlistItemResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string? BookSlug { get; set; }
    public string? CoverImageUrl { get; set; }
    public decimal PriceForSell { get; set; }
    public decimal PriceForRent { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Shape hồ sơ khách đúng theo FE (ShopCustomerProfile).</summary>
public class ShopCustomerProfileResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public VipTier VipTier { get; set; } = VipTier.Standard;
    public int RewardPoints { get; set; }
    public int WishlistBookCount { get; set; }
    public int AddressCount { get; set; }
}

public class ShopAddressResponse
{
    public Guid Id { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Guid ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public Guid CommuneId { get; set; }
    public string? CommuneName { get; set; }
    public bool IsDefault { get; set; }
    public string? Note { get; set; }
}

public class CreateShopAddressRequest
{
    public string RecipientName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Guid ProvinceId { get; set; }
    public Guid CommuneId { get; set; }
    public bool IsDefault { get; set; }
    public string? Note { get; set; }
}

/// <summary>Cập nhật hồ sơ cá nhân của khách đang đăng nhập.</summary>
public class UpdateShopProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
}

/// <summary>Chi tiết đơn hàng của khách: kèm dòng sản phẩm và lịch sử trạng thái.</summary>
public class ShopOrderDetailResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string OrderCode { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public int ItemCount { get; set; }
    public IReadOnlyCollection<string> PreviewTitles { get; set; } = [];
    public string? TrackingCode { get; set; }
    public string? PaymentUrl { get; set; }
    public string Message { get; set; } = string.Empty;

    public string? RecipientName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ShippingAddress { get; set; }
    public string? CarrierName { get; set; }
    public string? CustomerNote { get; set; }

    public IReadOnlyCollection<ShopOrderLineResponse> Items { get; set; } = [];
    public IReadOnlyCollection<OrderStatusHistoryResponse> Timeline { get; set; } = [];
}

public class ShopOrderLineResponse
{
    public Guid BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? CoverImageUrl { get; set; }
    public UnitType UnitType { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

/// <summary>Voucher khách hàng có thể dùng trên storefront.</summary>
public class ShopCouponResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MinOrderValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public string? Audience { get; set; }
}


public class ShopReviewPayload
{
    public Guid BookId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public IReadOnlyCollection<string> MediaUrls { get; set; } = [];
}

// ============================================================ TRACKING

public class TrackingRequest
{
    public string Code { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class TrackingResponse
{
    public string OrderCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TrackingCode { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal FinalAmount { get; set; }
    public IReadOnlyCollection<OrderStatusHistoryResponse> Timeline { get; set; } = [];
}
