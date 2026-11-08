using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Storefront;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

/// <summary>
/// API cho kho mua hàng (storefront) - đúng route FE-Bookstore gọi: /api/v1/shop/*
/// </summary>
[Route("api/v1/shop")]
public class ShopController : BaseApiController
{
    private readonly IStorefrontService _storefrontService;

    public ShopController(IStorefrontService storefrontService)
    {
        _storefrontService = storefrontService;
    }

    // ------------------------------------------------------------------ Catalog

    /// <summary>Danh sách sách kèm facet đếm số (danh mục, NXB, ngôn ngữ, định dạng, khoảng giá).</summary>
    [HttpGet("catalog")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchCatalog([FromQuery] CatalogSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetCatalogResultAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Chi tiết sách theo slug (dùng cho trang chi tiết sản phẩm).</summary>
    [HttpGet("books/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBookBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetCatalogDetailBySlugAsync(slug, cancellationToken);
        return Ok(result);
    }

    // ------------------------------------------------------------------ Giỏ hàng

    [HttpGet("cart")]
    [Authorize(Policy = PermissionConstants.ShopCart)]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetCartAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("cart/items")]
    [Authorize(Policy = PermissionConstants.ShopCart)]
    public async Task<IActionResult> AddToCart([FromBody] CartItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.AddToCartAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("cart/items/{itemId:guid}")]
    [Authorize(Policy = PermissionConstants.ShopCart)]
    public async Task<IActionResult> UpdateCartItem(Guid itemId, [FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.UpdateCartItemByIdAsync(CurrentUserId, itemId, request.Quantity, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("cart/items/{itemId:guid}")]
    [Authorize(Policy = PermissionConstants.ShopCart)]
    public async Task<IActionResult> RemoveCartItem(Guid itemId, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.RemoveCartItemByIdAsync(CurrentUserId, itemId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Áp mã voucher lên giỏ hàng.</summary>
    [HttpPost("cart/coupon")]
    [Authorize(Policy = PermissionConstants.ShopCart)]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.ApplyCouponAsync(CurrentUserId, request.Code, cancellationToken);
        return Ok(result);
    }

    // ------------------------------------------------------------------ Địa chỉ

    [HttpGet("addresses")]
    [Authorize(Policy = PermissionConstants.ShopCart)]
    public async Task<IActionResult> GetAddresses(CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetShopAddressesAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("addresses")]
    [Authorize(Policy = PermissionConstants.ShopCart)]
    public async Task<IActionResult> CreateAddress([FromBody] CreateShopAddressRequest request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.CreateShopAddressAsync(CurrentUserId, request, cancellationToken);
        return Created(result);
    }

    /// <summary>Cập nhật địa chỉ của chính khách đang đăng nhập.</summary>
    [HttpPut("addresses/{id:guid}")]
    [Authorize(Policy = PermissionConstants.ShopCart)]
    public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] CreateShopAddressRequest request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.UpdateShopAddressAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Xóa địa chỉ của chính khách đang đăng nhập.</summary>
    [HttpDelete("addresses/{id:guid}")]
    [Authorize(Policy = PermissionConstants.ShopCart)]
    public async Task<IActionResult> DeleteAddress(Guid id, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.DeleteShopAddressAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    // ------------------------------------------------------------------ Thanh toán & đơn hàng

    [HttpPost("checkout")]
    [Authorize(Policy = PermissionConstants.ShopCheckout)]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.CheckoutAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("orders")]
    [Authorize(Policy = PermissionConstants.ShopOrder)]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetMyOrdersAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Chi tiết đơn hàng của chính khách đang đăng nhập (dòng sản phẩm + timeline).</summary>
    [HttpGet("orders/{id:guid}")]
    [Authorize(Policy = PermissionConstants.ShopOrder)]
    public async Task<IActionResult> GetMyOrderDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetMyOrderDetailAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Khách hủy đơn của chính mình (chỉ khi chưa thanh toán).</summary>
    [HttpPut("orders/{id:guid}/cancel")]
    [Authorize(Policy = PermissionConstants.ShopOrder)]
    public async Task<IActionResult> CancelMyOrder(Guid id, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.CancelMyOrderAsync(CurrentUserId, id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("profile")]
    [Authorize(Policy = PermissionConstants.ProfileView)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetMyProfileAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Cập nhật hồ sơ cá nhân của chính khách đang đăng nhập.</summary>
    [HttpPut("profile")]
    [Authorize(Policy = PermissionConstants.ProfileUpdate)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateShopProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.UpdateMyProfileAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    // ----- Yêu thích -----

    [HttpGet("wishlist")]
    [Authorize(Policy = PermissionConstants.ShopWishlist)]
    public async Task<IActionResult> GetWishlist(CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetWishlistAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("wishlist/{bookId:guid}/toggle")]
    [Authorize(Policy = PermissionConstants.ShopWishlist)]
    public async Task<IActionResult> ToggleWishlist(Guid bookId, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.ToggleWishlistAsync(CurrentUserId, bookId, cancellationToken);
        return Ok(result);
    }

    // ------------------------------------------------------------------ Đánh giá

    [HttpPost("reviews")]
    [Authorize(Policy = PermissionConstants.ShopReview)]
    public async Task<IActionResult> SubmitReview([FromBody] ShopReviewPayload request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.SubmitShopReviewAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    // ------------------------------------------------------------------ Voucher

    /// <summary>Voucher đang hiệu lực dành cho khách (FE: GET /shop/coupons).</summary>
    [HttpGet("coupons")]
    [Authorize(Policy = PermissionConstants.ShopCatalog)]
    public async Task<IActionResult> GetCoupons(CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetActiveCouponsAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
