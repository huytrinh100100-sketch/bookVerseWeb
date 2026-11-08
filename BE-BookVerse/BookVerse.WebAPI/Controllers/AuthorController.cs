using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Partner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/authors")]
[Authorize]
public class AuthorController : BaseApiController
{
    private readonly IPartnerService _partnerService;

    public AuthorController(IPartnerService partnerService)
    {
        _partnerService = partnerService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.AuthorList)]
    public async Task<IActionResult> Search([FromQuery] PartnerSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _partnerService.SearchAuthorsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.AuthorView)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _partnerService.GetAuthorAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.AuthorCreate)]
    public async Task<IActionResult> Create([FromBody] CreateAuthorRequest request, CancellationToken cancellationToken)
    {
        var result = await _partnerService.CreateAuthorAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.AuthorUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAuthorRequest request, CancellationToken cancellationToken)
    {
        var result = await _partnerService.UpdateAuthorAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionConstants.AuthorDeleteTemp)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _partnerService.DeleteAuthorAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
