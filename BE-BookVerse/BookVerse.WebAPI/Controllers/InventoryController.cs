using BookVerse.Core.Constants;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Book;
using BookVerse.Core.Models.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookVerse.WebAPI.Controllers;

[Route("api/v1/inventory")]
[Authorize]
public class InventoryController : BaseApiController
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>Báo cáo tồn kho toàn hệ thống.</summary>
    [HttpGet]
    [Authorize(Policy = PermissionConstants.InventoryView)]
    public async Task<IActionResult> Search([FromQuery] InventorySearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.SearchAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("books/{bookId:guid}")]
    [Authorize(Policy = PermissionConstants.InventoryView)]
    public async Task<IActionResult> GetByBook(Guid bookId, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetByBookAsync(bookId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Điều chỉnh tồn kho (âm = xuất, dương = nhập).</summary>
    [HttpPost("adjust")]
    [Authorize(Policy = PermissionConstants.BookAdjustStock)]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockRequest request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.AdjustStockAsync(request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Tạo phiếu nhập tồn kho.</summary>
    [HttpPost("restock")]
    [Authorize(Policy = PermissionConstants.InventoryRestock)]
    public async Task<IActionResult> Restock([FromBody] RestockRequest request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.RestockAsync(request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Lập phiếu nhập kho ở trạng thái nháp (FE: POST /inventory/purchase-orders/draft).</summary>
    [HttpPost("purchase-orders/draft")]
    [Authorize(Policy = PermissionConstants.InventoryRestock)]
    public async Task<IActionResult> CreateDraftPurchaseOrder([FromBody] DraftPurchaseOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.CreateDraftPurchaseOrderAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    /// <summary>Đề xuất nhập kho cho toàn bộ mục tồn kho thấp (FE: POST /inventory/restock-all).</summary>
    [HttpPost("restock-all")]
    [Authorize(Policy = PermissionConstants.InventoryRestock)]
    public async Task<IActionResult> RestockAll([FromBody] RestockAllRequest request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.RestockAllAsync(request, CurrentUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Phiếu kiểm kê tồn kho (FE: GET /inventory/audit-sheet).</summary>
    [HttpGet("audit-sheet")]
    [Authorize(Policy = PermissionConstants.InventoryView)]
    public async Task<IActionResult> GetAuditSheet(CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetAuditSheetAsync(cancellationToken);
        return Ok(result);
    }

    // ----- Kho hàng -----

    [HttpGet("warehouses")]
    [Authorize(Policy = PermissionConstants.WarehouseList)]
    public async Task<IActionResult> GetWarehouses(CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetWarehousesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("warehouses")]
    [Authorize(Policy = PermissionConstants.WarehouseCreate)]
    public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.CreateWarehouseAsync(request, CurrentUserId, cancellationToken);
        return Created(result);
    }

    [HttpPut("warehouses/{id:guid}")]
    [Authorize(Policy = PermissionConstants.WarehouseUpdate)]
    public async Task<IActionResult> UpdateWarehouse(Guid id, [FromBody] UpdateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.UpdateWarehouseAsync(id, request, CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
