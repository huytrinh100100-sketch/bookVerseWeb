using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Partner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/publishers")]
[Authorize]
public class PublisherController : BaseApiController
{
    private readonly IPartnerService _partnerService;

    public PublisherController(IPartnerService partnerService)
    {
        _partnerService = partnerService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.PublisherList)]
    public async Task<IActionResult> Search([FromQuery] PartnerSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _partnerService.SearchPublishersAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.PublisherView)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _partnerService.GetPublisherAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.PublisherCreate)]
    public async Task<IActionResult> Create([FromBody] CreatePublisherRequest request, CancellationToken cancellationToken)
    {
        var result = await _partnerService.CreatePublisherAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.PublisherUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePublisherRequest request, CancellationToken cancellationToken)
    {
        var result = await _partnerService.UpdatePublisherAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionConstants.PublisherDeleteTemp)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _partnerService.DeletePublisherAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
