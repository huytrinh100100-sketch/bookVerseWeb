using BookVerse.Core.Constants;

namespace BookVerse.Core.Entities;

/// <summary>Bản sao vật lý của một cuốn sách - nguồn sự thật cho nghiệp vụ mượn.</summary>
public class BookCopy
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string ConditionStatus { get; set; } = "Mới";
    public string? ShelfCode { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime? AcquiredDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public BookCopyStatus Status { get; set; } = BookCopyStatus.Available;
    public Guid? CurrentOrderId { get; set; }
    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public byte[]? RowVersion { get; set; }

    public Book Book { get; set; } = null!;
    public Warehouse? Warehouse { get; set; }
}
