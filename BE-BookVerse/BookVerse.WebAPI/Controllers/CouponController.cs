using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Coupon;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/coupons")]
[Authorize]
public class CouponController : BaseApiController
{
    private readonly ICouponService _couponService;

    public CouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.CouponList)]
    public async Task<IActionResult> Search([FromQuery] CouponSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _couponService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.CouponView)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _couponService.GetDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.CouponCreate)]
    public async Task<IActionResult> Create([FromBody] CreateCouponRequest request, CancellationToken cancellationToken)
    {
        var result = await _couponService.CreateAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.CouponUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCouponRequest request, CancellationToken cancellationToken)
    {
        var result = await _couponService.UpdateAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionConstants.CouponDeleteTemp)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _couponService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/permanent")]
    [Authorize(Policy = PermissionConstants.CouponDelete)]
    public async Task<IActionResult> HardDelete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _couponService.HardDeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Tra cứu voucher theo mã (FE: GET /coupons/validate/{code}).</summary>
    [HttpGet("validate/{code}")]
    [Authorize(Policy = PermissionConstants.CouponView)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
    {
        var result = await _couponService.GetByCodeAsync(code, cancellationToken);
        return Ok(result);
    }

    /// <summary>Tạm dừng voucher (FE: PUT /coupons/{id}/pause).</summary>
    [HttpPut("{id:guid}/pause")]
    [Authorize(Policy = PermissionConstants.CouponUpdate)]
    public async Task<IActionResult> Pause(Guid id, CancellationToken cancellationToken)
    {
        var result = await _couponService.PauseAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Kiểm tra voucher có hợp lệ và tính số tiền giảm.</summary>
    [HttpPost("validate")]
    [Authorize(Policy = PermissionConstants.ShopCheckout)]
    public async Task<IActionResult> Validate([FromBody] ValidateCouponRequest request, CancellationToken cancellationToken)
    {
        var result = await _couponService.ValidateAsync(request, cancellationToken);
        return Ok(result);
    }

    // ----- Chiến dịch -----

    [HttpGet("campaigns")]
    [Authorize(Policy = PermissionConstants.CampaignList)]
    public async Task<IActionResult> SearchCampaigns([FromQuery] CampaignSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _couponService.SearchCampaignsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("campaigns/{id:guid}")]
    [Authorize(Policy = PermissionConstants.CampaignView)]
    public async Task<IActionResult> GetCampaign(Guid id, CancellationToken cancellationToken)
    {
        var result = await _couponService.GetCampaignAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("campaigns")]
    [Authorize(Policy = PermissionConstants.CampaignCreate)]
    public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignRequest request, CancellationToken cancellationToken)
    {
        var result = await _couponService.CreateCampaignAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("campaigns/{id:guid}")]
    [Authorize(Policy = PermissionConstants.CampaignUpdate)]
    public async Task<IActionResult> UpdateCampaign(Guid id, [FromBody] UpdateCampaignRequest request, CancellationToken cancellationToken)
    {
        var result = await _couponService.UpdateCampaignAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("campaigns/{id:guid}")]
    [Authorize(Policy = PermissionConstants.CampaignDeleteTemp)]
    public async Task<IActionResult> DeleteCampaign(Guid id, CancellationToken cancellationToken)
    {
        var result = await _couponService.DeleteCampaignAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
