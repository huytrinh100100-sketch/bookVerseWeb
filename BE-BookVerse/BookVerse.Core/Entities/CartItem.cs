using BookVerse.Core.Constants;

namespace BookVerse.Core.Entities;

/// <summary>
/// Chi tiết giỏ hàng của khách hàng (tách biệt với WishlistItem).
/// Mỗi dòng gắn với 1 sách + 1 đơn vị (Bán / Mượn) + số lượng.
/// </summary>
public class CartItem
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid BookId { get; set; }
    public UnitType UnitType { get; set; } = UnitType.Sell;
    public int Quantity { get; set; } = 1;

    /// <summary>Đơn giá tại thời điểm thêm vào giỏ (snapshot để hiển thị).</summary>
    public decimal UnitPrice { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser Customer { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
