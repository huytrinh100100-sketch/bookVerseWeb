using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/permissions")]
[Authorize]
public class PermissionController : BaseApiController
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.PermissionList)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _permissionService.GetAllAsync(cancellationToken);
        return Ok(result);
    }
}
