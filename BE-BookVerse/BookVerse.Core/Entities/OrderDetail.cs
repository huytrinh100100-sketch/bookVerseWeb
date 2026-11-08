using BookVerse.Core.Constants;

namespace BookVerse.Core.Entities;

public class OrderDetail
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid BookId { get; set; }

    public UnitType UnitType { get; set; } = UnitType.Sell;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }

    /// <summary>Snapshot dữ liệu sách tại thời điểm đặt hàng (hiển thị offline, không join).</summary>
    public string? BookTitle { get; set; }
    public string? IsbnSnapshot { get; set; }
    public string? AuthorName { get; set; }
    public string? PublisherName { get; set; }
    public string? CoverImageUrl { get; set; }

    /// <summary>Hạn trả - chỉ dùng cho UnitType = Rent.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Ngày trả thực tế - chỉ dùng cho UnitType = Rent.</summary>
    public DateTime? ReturnedDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Order Order { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
