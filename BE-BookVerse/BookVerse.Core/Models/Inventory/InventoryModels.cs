using BookVerse.Core.Constants;

namespace BookVerse.Core.Models.Inventory;

public class WarehouseListItemResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public Guid? ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int CopyCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateWarehouseRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public Guid? ProvinceId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateWarehouseRequest
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public Guid? ProvinceId { get; set; }
    public bool? IsActive { get; set; }
}

public class InventorySearchRequest
{
    public string? Keyword { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? PublisherId { get; set; }
    public string? StockLevel { get; set; }
    public BookCopyStatus? Status { get; set; }
    public int? LowStockThreshold { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class InventoryListItemResponse
{
    public Guid BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string? Isbn { get; set; }
    public Guid? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public Guid? PublisherId { get; set; }
    public string? PublisherName { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public decimal PriceForSell { get; set; }
    public decimal PriceForRent { get; set; }
    public int TotalQuantity { get; set; }
    public int StockForSell { get; set; }
    public int StockForRent { get; set; }
    public int ReservedQuantity { get; set; }
    public int LentQuantity { get; set; }
    public string Velocity { get; set; } = "slow";
    public int SafetyStock { get; set; }
    public int ReorderPoint { get; set; }
    public int AvailableCopies { get; set; }
    public int Threshold { get; set; }
    public bool IsLowStock { get; set; }
}

public class RestockRequest
{
    public Guid BookId { get; set; }
    public Guid? WarehouseId { get; set; }
    public UnitType UnitType { get; set; } = UnitType.Sell;
    public int Quantity { get; set; }
    public string ConditionStatus { get; set; } = "Moi";
    public string? ShelfCode { get; set; }
    public string? Reason { get; set; }
}

/// <summary>FE: POST /inventory/purchase-orders/draft.</summary>
public class DraftPurchaseOrderRequest
{
    public Guid WarehouseId { get; set; }
    public int LeadTimeDays { get; set; }
    public string? Notes { get; set; }
    public List<DraftPurchaseOrderDetail> Details { get; set; } = [];
}

public class DraftPurchaseOrderDetail
{
    public Guid BookId { get; set; }
    public int Quantity { get; set; }
}

public class DraftPurchaseOrderResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public int LeadTimeDays { get; set; }
    public string? Notes { get; set; }
    public int TotalQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RestockAllRequest
{
    public Guid? WarehouseId { get; set; }
    public int QuantityPerBook { get; set; } = 5;
    public string? Reason { get; set; }
}

public class AuditSheetResponse
{
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public int TotalBooks { get; set; }
    public int TotalCopies { get; set; }
    public IReadOnlyCollection<AuditSheetRow> Rows { get; set; } = [];
}

public class AuditSheetRow
{
    public Guid BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public int SystemQuantity { get; set; }
    public int ActualQuantity { get; set; }
    public int Difference { get; set; }
    public string? Note { get; set; }
}
