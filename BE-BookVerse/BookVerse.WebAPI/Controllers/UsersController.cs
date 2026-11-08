using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

/// <summary>
/// Nhân viên (tài khoản có vai trò) - đúng route FE-Bookstore gọi: /api/v1/users/staff
/// </summary>
[Route("api/v1/users")]
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IRoleService _roleService;

    public UsersController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>Danh sách nhân viên dùng cho các bộ chọn người xử lý (đơn hàng, bài viết...).</summary>
    [HttpGet("staff")]
    [Authorize(Policy = PermissionConstants.AccountList)]
    public async Task<IActionResult> GetStaff(CancellationToken cancellationToken)
    {
        var result = await _roleService.GetStaffAsync(cancellationToken);
        return Ok(result);
    }
}
