using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Setting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/settings")]
[Authorize]
public class SettingController : BaseApiController
{
    private readonly ISettingService _settingService;

    public SettingController(ISettingService settingService)
    {
        _settingService = settingService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.SettingView)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _settingService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Policy = PermissionConstants.SettingUpdate)]
    public async Task<IActionResult> Update([FromBody] UpdateSettingRequest request, CancellationToken cancellationToken)
    {
        var result = await _settingService.UpdateAsync(request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Cấu hình cửa hàng (FE: GET /settings/store).</summary>
    [HttpGet("store")]
    [Authorize(Policy = PermissionConstants.SettingView)]
    public async Task<IActionResult> GetStore(CancellationToken cancellationToken)
    {
        var result = await _settingService.GetStoreAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Cập nhật cấu hình cửa hàng (FE: PUT /settings/store).</summary>
    [HttpPut("store")]
    [Authorize(Policy = PermissionConstants.SettingUpdate)]
    public async Task<IActionResult> UpdateStore([FromBody] UpdateStoreSettingRequest request, CancellationToken cancellationToken)
    {
        var result = await _settingService.UpdateStoreAsync(request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("payment-gateways")]
    [Authorize(Policy = PermissionConstants.SettingView)]
    public async Task<IActionResult> GetPaymentGateways(CancellationToken cancellationToken)
    {
        var result = await _settingService.GetPaymentGatewaysAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("carriers")]
    [Authorize(Policy = PermissionConstants.SettingView)]
    public async Task<IActionResult> GetCarriers(CancellationToken cancellationToken)
    {
        var result = await _settingService.GetCarriersAsync(cancellationToken);
        return Ok(result);
    }
}
