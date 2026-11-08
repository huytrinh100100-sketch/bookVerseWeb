using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Storefront;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/storefront")]
[Authorize]
public class StorefrontController : BaseApiController
{
    private readonly IStorefrontService _storefrontService;

    public StorefrontController(IStorefrontService storefrontService)
    {
        _storefrontService = storefrontService;
    }

    /// <summary>Dữ liệu trang chủ cửa hàng: banner, danh mục, sách nổi bật, bài viết, voucher.</summary>
    [HttpGet("home")]
    [Authorize(Policy = PermissionConstants.ShopCatalog)]
    public async Task<IActionResult> GetHome(CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetHomeAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Danh mục sách trên storefront với bộ lọc và phân trang.</summary>
    [HttpGet("catalog")]
    [Authorize(Policy = PermissionConstants.ShopCatalog)]
    public async Task<IActionResult> SearchCatalog([FromQuery] CatalogSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.SearchCatalogAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("catalog/{id:guid}")]
    [Authorize(Policy = PermissionConstants.ShopCatalog)]
    public async Task<IActionResult> GetCatalogDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetCatalogDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet("catalog/slug/{slug}")]
    [Authorize(Policy = PermissionConstants.ShopCatalog)]
    public async Task<IActionResult> GetCatalogDetailBySlug(string slug, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetCatalogDetailBySlugAsync(slug, cancellationToken);
        return Ok(result);
    }

    // ----- Giỏ hàng -----

    [HttpGet("cart")]
    [Authorize(Policy = PermissionConstants.ShopCart)]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetCartAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("cart")]
    [Authorize(Policy = PermissionConstants.ShopCart)]
    public async Task<IActionResult> AddToCart([FromBody] CartItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.AddToCartAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("cart/{bookId:guid}")]
    [Authorize(Policy = PermissionConstants.ShopCart)]
    public async Task<IActionResult> UpdateCartItem(Guid bookId, [FromQuery] int quantity, [FromQuery] BookVerse.Core.Constants.UnitType unitType = BookVerse.Core.Constants.UnitType.Sell, CancellationToken cancellationToken = default)
    {
        var result = await _storefrontService.UpdateCartItemAsync(CurrentUserId, bookId, unitType, quantity, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("cart/{bookId:guid}")]
    [Authorize(Policy = PermissionConstants.ShopCart)]
    public async Task<IActionResult> RemoveFromCart(Guid bookId, [FromQuery] BookVerse.Core.Constants.UnitType unitType = BookVerse.Core.Constants.UnitType.Sell, CancellationToken cancellationToken = default)
    {
        var result = await _storefrontService.RemoveFromCartAsync(CurrentUserId, bookId, unitType, cancellationToken);
        return Ok(result);
    }

    // ----- Thanh toán -----

    [HttpPost("checkout")]
    [Authorize(Policy = PermissionConstants.ShopCheckout)]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.CheckoutAsync(CurrentUserId, request, cancellationToken);
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

    // ----- Tra cứu đơn hàng -----

    [HttpPost("tracking")]
    [AllowAnonymous]
    public async Task<IActionResult> Track([FromBody] TrackingRequest request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.TrackAsync(request, cancellationToken);
        return Ok(result);
    }
}
