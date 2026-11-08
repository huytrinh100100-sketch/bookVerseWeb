using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/roles")]
[Authorize]
public class RoleController : BaseApiController
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.RoleList)]
    public async Task<IActionResult> Search([FromQuery] RoleSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.RoleView)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleService.GetDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.RoleCreate)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.CreateAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.RoleUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdateAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/permissions")]
    [Authorize(Policy = PermissionConstants.RoleAssignPermissions)]
    public async Task<IActionResult> AssignPermissions(Guid id, [FromBody] AssignPermissionsRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.AssignPermissionsAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Nhân bản vai trò (FE: POST /roles/{id}/clone).</summary>
    [HttpPost("{id:guid}/clone")]
    [Authorize(Policy = PermissionConstants.RoleCreate)]
    public async Task<IActionResult> Clone(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleService.CloneAsync(id, CurrentUserId, cancellationToken);
        return Created(result);
    }

    /// <summary>Gán vai trò cho một tài khoản (FE: POST /roles/assign).</summary>
    [HttpPost("assign")]
    [Authorize(Policy = PermissionConstants.AccountAssignRoles)]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _roleService.AssignRoleAsync(request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionConstants.RoleDeleteTemp)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/permanent")]
    [Authorize(Policy = PermissionConstants.RoleDelete)]
    public async Task<IActionResult> HardDelete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleService.HardDeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = PermissionConstants.RoleRestore)]
    public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleService.RestoreAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
