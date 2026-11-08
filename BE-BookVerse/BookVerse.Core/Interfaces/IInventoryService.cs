using BookVerse.Core.Common;
using BookVerse.Core.Models.Book;
using BookVerse.Core.Models.Inventory;

namespace BookVerse.Core.Interfaces;

public interface IInventoryService
{
    Task<ServiceResult<PagedResult<InventoryListItemResponse>>> SearchAsync(InventorySearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<InventoryListItemResponse>> GetByBookAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> AdjustStockAsync(AdjustStockRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> RestockAsync(RestockRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<WarehouseListItemResponse>>> GetWarehousesAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<WarehouseListItemResponse>> CreateWarehouseAsync(CreateWarehouseRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<WarehouseListItemResponse>> UpdateWarehouseAsync(Guid id, UpdateWarehouseRequest request, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>Lập phiếu nhập kho ở trạng thái nháp (FE: POST /inventory/purchase-orders/draft).</summary>
    Task<ServiceResult<DraftPurchaseOrderResponse>> CreateDraftPurchaseOrderAsync(DraftPurchaseOrderRequest request, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>Tạo đề xuất nhập kho cho toàn bộ mục thấp tồn (FE: POST /inventory/restock-all).</summary>
    Task<ServiceResult<int>> RestockAllAsync(RestockAllRequest request, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>Phiếu kiểm kê tồn kho (FE: GET /inventory/audit-sheet).</summary>
    Task<ServiceResult<AuditSheetResponse>> GetAuditSheetAsync(CancellationToken cancellationToken = default);
}
