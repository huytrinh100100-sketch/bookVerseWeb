using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/profile")]
[Authorize]
public class ProfileController : BaseApiController
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.ProfileView)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _profileService.GetAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Policy = PermissionConstants.ProfileUpdate)]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _profileService.UpdateAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("change-password")]
    [Authorize(Policy = PermissionConstants.ProfileChangePassword)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _profileService.ChangePasswordAsync(CurrentUserId, request, cancellationToken);
        return Ok(result);
    }
}
