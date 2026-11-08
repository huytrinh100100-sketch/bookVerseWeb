using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

/// <summary>
/// Kho hàng - đúng route FE-Bookstore gọi: /api/v1/warehouses
/// </summary>
[Route("api/v1/warehouses")]
[Authorize]
public class WarehousesController : BaseApiController
{
    private readonly IInventoryService _inventoryService;

    public WarehousesController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.WarehouseList)]
    public async Task<IActionResult> GetWarehouses(CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetWarehousesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.WarehouseCreate)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.CreateWarehouseAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionConstants.WarehouseUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.UpdateWarehouseAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
