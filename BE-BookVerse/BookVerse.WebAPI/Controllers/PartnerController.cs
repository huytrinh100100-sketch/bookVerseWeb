using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Partner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

/// <summary>
/// Đối tác hợp nhất (tác giả + nhà xuất bản) - đúng route FE-Bookstore gọi: /api/v1/partners
/// </summary>
[Route("api/v1/partners")]
[Authorize]
public class PartnerController : BaseApiController
{
    private readonly IPartnerService _partnerService;

    public PartnerController(IPartnerService partnerService)
    {
        _partnerService = partnerService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.AuthorList)]
    public async Task<IActionResult> Search([FromQuery] PartnerSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _partnerService.SearchPartnersAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.AuthorView)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _partnerService.GetPartnerAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.AuthorCreate)]
    public async Task<IActionResult> Create([FromBody] CreatePartnerRequest request, CancellationToken cancellationToken)
    {
        var result = await _partnerService.CreatePartnerAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.AuthorUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePartnerRequest request, CancellationToken cancellationToken)
    {
        var result = await _partnerService.UpdatePartnerAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionConstants.AuthorDeleteTemp)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _partnerService.DeletePartnerAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
