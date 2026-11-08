namespace BookVerse.Core.Entities;

/// <summary>Dòng chi tiết của phiếu nhập kho.</summary>
public class PurchaseOrderDetail
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid BookId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
