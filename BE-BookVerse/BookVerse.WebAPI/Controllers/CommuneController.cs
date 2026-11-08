using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Geography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/communes")]
[Authorize]
public class CommuneController : BaseApiController
{
    private readonly ICommuneService _communeService;

    public CommuneController(ICommuneService communeService)
    {
        _communeService = communeService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.CommuneList)]
    public async Task<IActionResult> Search([FromQuery] CommuneSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _communeService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("by-province/{provinceId:guid}")]
    [Authorize(Policy = PermissionConstants.CommuneList)]
    public async Task<IActionResult> GetByProvince(Guid provinceId, [FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var result = await _communeService.GetByProvinceAsync(provinceId, activeOnly, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.CommuneView)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _communeService.GetDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.CommuneCreate)]
    public async Task<IActionResult> Create([FromBody] CreateCommuneRequest request, CancellationToken cancellationToken)
    {
        var result = await _communeService.CreateAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.CommuneUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommuneRequest request, CancellationToken cancellationToken)
    {
        var result = await _communeService.UpdateAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionConstants.CommuneDeleteTemp)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _communeService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
