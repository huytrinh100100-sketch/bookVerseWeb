using BookVerse.Core.Constants;

namespace BookVerse.Core.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid? StaffId { get; set; }
    public Guid? CouponId { get; set; }

    /// <summary>Snapshot mã voucher tại thời điểm đặt hàng (lịch sử không bị ảnh hưởng khi voucher bị sửa/xóa).</summary>
    public string? CouponCode { get; set; }

    public DateTime OrderDate { get; set; }
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

    public decimal TotalAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }

    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public Guid? CancelledBy { get; set; }
    public string? CancelReason { get; set; }

    public string? ShippingAddress { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? CommuneId { get; set; }
    public string? TrackingCode { get; set; }
    public string? CustomerNote { get; set; }
    public string? StaffNote { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public byte[]? RowVersion { get; set; }

    public ApplicationUser Customer { get; set; } = null!;
    public ApplicationUser? Staff { get; set; }
    public Coupon? Coupon { get; set; }
    public ICollection<OrderDetail> Details { get; set; } = [];
    public Province? Province { get; set; }
    public Commune? Commune { get; set; }
}
