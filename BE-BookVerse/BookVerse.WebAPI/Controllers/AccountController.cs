using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/accounts")]
[Authorize]
public class AccountController : BaseApiController
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.AccountList)]
    public async Task<IActionResult> Search([FromQuery] AccountSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _accountService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.AccountView)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.GetDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.AccountCreate)]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _accountService.CreateAsync(request, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.AccountUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _accountService.UpdateAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/roles")]
    [Authorize(Policy = PermissionConstants.AccountAssignRoles)]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignRolesRequest request, CancellationToken cancellationToken)
    {
        var result = await _accountService.AssignRolesAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/lock")]
    [Authorize(Policy = PermissionConstants.AccountLock)]
    public async Task<IActionResult> Lock(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.LockAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/unlock")]
    [Authorize(Policy = PermissionConstants.AccountUnlock)]
    public async Task<IActionResult> Unlock(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.UnlockAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionConstants.AccountDeleteTemp)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/permanent")]
    [Authorize(Policy = PermissionConstants.AccountDelete)]
    public async Task<IActionResult> HardDelete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.HardDeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = PermissionConstants.AccountRestore)]
    public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.RestoreAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
