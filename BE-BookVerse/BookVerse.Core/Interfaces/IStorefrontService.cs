using BookVerse.Core.Common;
using BookVerse.Core.Constants;
using BookVerse.Core.Models.Storefront;

namespace BookVerse.Core.Interfaces;

public interface IStorefrontService
{
    Task<ServiceResult<HomeResponse>> GetHomeAsync(CancellationToken cancellationToken = default);

    /// <summary>Tìm kiếm catalog dạng phân trang (dùng cho trang quản trị).</summary>
    Task<ServiceResult<PagedResult<CatalogItemResponse>>> SearchCatalogAsync(CatalogSearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>Tìm kiếm catalog kèm facet đếm số (đúng contract FE /shop/catalog).</summary>
    Task<ServiceResult<CatalogResult>> GetCatalogResultAsync(CatalogSearchRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult<CatalogDetailResponse>> GetCatalogDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<CatalogDetailResponse>> GetCatalogDetailBySlugAsync(string slug, CancellationToken cancellationToken = default);

    // ----- Giỏ hàng (dựa trên bảng CartItems) -----
    Task<ServiceResult<CartResponse>> GetCartAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ServiceResult<CartResponse>> AddToCartAsync(Guid customerId, CartItemRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<CartResponse>> UpdateCartItemAsync(Guid customerId, Guid bookId, UnitType unitType, int quantity, CancellationToken cancellationToken = default);
    Task<ServiceResult<CartResponse>> RemoveFromCartAsync(Guid customerId, Guid bookId, UnitType unitType, CancellationToken cancellationToken = default);

    /// <summary>Cập nhật dòng giỏ theo id dòng (FE: PUT /shop/cart/items/{itemId}).</summary>
    Task<ServiceResult<CartResponse>> UpdateCartItemByIdAsync(Guid customerId, Guid itemId, int quantity, CancellationToken cancellationToken = default);

    /// <summary>Xoá dòng giỏ theo id dòng (FE: DELETE /shop/cart/items/{itemId}).</summary>
    Task<ServiceResult<CartResponse>> RemoveCartItemByIdAsync(Guid customerId, Guid itemId, CancellationToken cancellationToken = default);

    /// <summary>Áp voucher lên giỏ hàng (FE: POST /shop/cart/coupon).</summary>
    Task<ServiceResult<CartResponse>> ApplyCouponAsync(Guid customerId, string code, CancellationToken cancellationToken = default);

    Task<ServiceResult<CheckoutResponse>> CheckoutAsync(Guid customerId, CheckoutRequest request, CancellationToken cancellationToken = default);

    // ----- Tài khoản khách trên storefront -----
    Task<ServiceResult<IReadOnlyCollection<CheckoutResponse>>> GetMyOrdersAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ServiceResult<ShopCustomerProfileResponse>> GetMyProfileAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<ShopAddressResponse>>> GetShopAddressesAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ServiceResult<ShopAddressResponse>> CreateShopAddressAsync(Guid customerId, CreateShopAddressRequest request, CancellationToken cancellationToken = default);

    /// <summary>Cập nhật địa chỉ của chính khách đang đăng nhập (FE: PUT /shop/addresses/{id}).</summary>
    Task<ServiceResult<ShopAddressResponse>> UpdateShopAddressAsync(Guid customerId, Guid addressId, CreateShopAddressRequest request, CancellationToken cancellationToken = default);

    /// <summary>Xoá địa chỉ của chính khách đang đăng nhập (FE: DELETE /shop/addresses/{id}).</summary>
    Task<ServiceResult<bool>> DeleteShopAddressAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken = default);

    /// <summary>Danh sách voucher đang hiệu lực dành cho khách (FE: GET /shop/coupons).</summary>
    Task<ServiceResult<IReadOnlyCollection<ShopCouponResponse>>> GetActiveCouponsAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>Cập nhật hồ sơ cá nhân của chính khách đang đăng nhập (FE: PUT /shop/profile).</summary>
    Task<ServiceResult<ShopCustomerProfileResponse>> UpdateMyProfileAsync(Guid customerId, UpdateShopProfileRequest request, CancellationToken cancellationToken = default);

    /// <summary>Hủy đơn hàng của chính khách đang đăng nhập (FE: PUT /shop/orders/{id}/cancel).</summary>
    Task<ServiceResult<bool>> CancelMyOrderAsync(Guid customerId, Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>Chi tiết đơn hàng của chính khách đang đăng nhập (FE: GET /shop/orders/{id}).</summary>
    Task<ServiceResult<ShopOrderDetailResponse>> GetMyOrderDetailAsync(Guid customerId, Guid orderId, CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> SubmitShopReviewAsync(Guid customerId, ShopReviewPayload request, CancellationToken cancellationToken = default);

    Task<ServiceResult<IReadOnlyCollection<WishlistItemResponse>>> GetWishlistAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> ToggleWishlistAsync(Guid customerId, Guid bookId, CancellationToken cancellationToken = default);
    Task<ServiceResult<TrackingResponse>> TrackAsync(TrackingRequest request, CancellationToken cancellationToken = default);
}
