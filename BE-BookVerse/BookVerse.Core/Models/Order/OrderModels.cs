using BookVerse.Core.Constants;

namespace BookVerse.Core.Models.Order;

public class OrderSearchRequest
{
    public string? Keyword { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? StaffId { get; set; }
    public OrderStatus? OrderStatus { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public bool? IsPaid { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public bool? IsDeleted { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class OrderListItemResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public DateTime OrderDate { get; set; }
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ProvinceName { get; set; }
    public string? CommuneName { get; set; }
    public string? CouponCode { get; set; }
    public int TotalItems { get; set; }
}

public class OrderDetailResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public Guid? StaffId { get; set; }
    public string? StaffName { get; set; }
    public Guid? CouponId { get; set; }
    public string? CouponCode { get; set; }
    public DateTime OrderDate { get; set; }
    public int ItemCount { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
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
    public string? ProvinceName { get; set; }
    public string? CommuneName { get; set; }
    public string? TrackingCode { get; set; }
    public string? CustomerNote { get; set; }
    public string? StaffNote { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public byte[]? RowVersion { get; set; }
    public IReadOnlyCollection<OrderDetailItemResponse> Details { get; set; } = [];
}

public class OrderDetailItemResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string? BookSlug { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? AuthorName { get; set; }
    public string? IsbnSnapshot { get; set; }
    public UnitType UnitType { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
}

public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public Guid? StaffId { get; set; }
    public string? CouponCode { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
    public string? ShippingAddress { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? CommuneId { get; set; }
    public string? CustomerNote { get; set; }
    public string? StaffNote { get; set; }
    public List<CreateOrderDetailRequest> Details { get; set; } = [];
}

public class CreateOrderDetailRequest
{
    public Guid BookId { get; set; }
    public UnitType UnitType { get; set; } = UnitType.Sell;
    public int Quantity { get; set; } = 1;
    public decimal? UnitPrice { get; set; }
    public DateTime? DueDate { get; set; }
}

public class UpdateOrderRequest
{
    public Guid? StaffId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? ShippingAddress { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? CommuneId { get; set; }
    public string? TrackingCode { get; set; }
    public string? CustomerNote { get; set; }
    public string? StaffNote { get; set; }
    public byte[]? RowVersion { get; set; }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus OrderStatus { get; set; }
    public string? Reason { get; set; }
    public string? StaffNote { get; set; }
    public byte[]? RowVersion { get; set; }
}

public class CancelOrderRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class RefundOrderRequest
{
    public string Reason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class MarkPaidRequest
{
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
    public string? TransactionCode { get; set; }
}

public class OrderStatusHistoryResponse
{
    public Guid Id { get; set; }
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Note { get; set; }
    public Guid? ChangedBy { get; set; }
    public string? ChangedByName { get; set; }
    public DateTime ChangedAt { get; set; }
}

/// <summary>Kết quả xuất danh sách đơn hàng (FE: GET /orders/export).</summary>
public class OrderExportResult
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "text/csv";
    public string Content { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
}
