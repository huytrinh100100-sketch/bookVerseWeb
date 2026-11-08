using BookVerse.Core.Constants;

namespace BookVerse.Core.Entities;

/// <summary>Lịch sử thay đổi trạng thái đơn hàng (dùng cho timeline tracking).</summary>
public class OrderStatusHistory
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Note { get; set; }
    public Guid? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }

    public Order Order { get; set; } = null!;
}
