using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Geography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/provinces")]
[Authorize]
public class ProvinceController : BaseApiController
{
    private readonly IProvinceService _provinceService;

    public ProvinceController(IProvinceService provinceService)
    {
        _provinceService = provinceService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.ProvinceList)]
    public async Task<IActionResult> Search([FromQuery] ProvinceSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _provinceService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Danh sách tỉnh/thành phố dùng cho dropdown (chỉ lấy đang hoạt động).</summary>
    [HttpGet("all")]
    [Authorize(Policy = PermissionConstants.ProvinceList)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var result = await _provinceService.GetAllAsync(activeOnly, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionConstants.ProvinceView)]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _provinceService.GetDetailAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.ProvinceCreate)]
    public async Task<IActionResult> Create([FromBody] CreateProvinceRequest request, CancellationToken cancellationToken)
    {
        var result = await _provinceService.CreateAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.ProvinceUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProvinceRequest request, CancellationToken cancellationToken)
    {
        var result = await _provinceService.UpdateAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionConstants.ProvinceDeleteTemp)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _provinceService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
