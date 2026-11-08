namespace BookVerse.Core.Entities;

/// <summary>
/// Phiếu nhập kho (Purchase Order) - dùng cho luồng đề xuất nhập kho của tồn kho.
/// Hiện tại chỉ dùng ở trạng thái Draft (FE: POST /inventory/purchase-orders/draft).
/// </summary>
public class PurchaseOrder
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }

    /// <summary>Số ngày dự kiến để hàng về kho.</summary>
    public int LeadTimeDays { get; set; }

    public string? Notes { get; set; }

    /// <summary>Tổng số lượng các dòng trong phiếu.</summary>
    public int TotalQuantity { get; set; }

    /// <summary>Trạng thái phiếu: Draft / Approved / Completed / Cancelled.</summary>
    public string Status { get; set; } = "Draft";

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Warehouse? Warehouse { get; set; }
    public ICollection<PurchaseOrderDetail> Details { get; set; } = [];
}
