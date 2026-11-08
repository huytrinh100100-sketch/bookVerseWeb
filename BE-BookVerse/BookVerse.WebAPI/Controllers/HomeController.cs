using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/home")]
[Authorize]
public class HomeController : BaseApiController
{
    private readonly IHomeService _homeService;

    public HomeController(IHomeService homeService)
    {
        _homeService = homeService;
    }

    /// <summary>Thông tin trang chủ sau đăng nhập: vai trò, số quyền, lần đăng nhập cuối.</summary>
    [HttpGet]
    [Authorize(Policy = PermissionConstants.HomeView)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _homeService.GetAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
