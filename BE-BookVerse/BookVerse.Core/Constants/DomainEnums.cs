namespace BookVerse.Core.Constants;

/// <summary>Trạng thái hiển thị của một cuốn sách trên storefront.</summary>
public enum BookStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

/// <summary>Đơn vị mua: bán (Sell) hoặc mượn (Rent).</summary>
public enum UnitType
{
    Sell = 0,
    Rent = 1
}

/// <summary>Trạng thái bản sao vật lý (nguồn sự thật cho nghiệp vụ mượn).</summary>
public enum BookCopyStatus
{
    Available = 0,
    Lent = 1,
    Reserved = 2,
    Lost = 3,
    Damaged = 4,
    Returned = 5
}

/// <summary>Trạng thái vòng đời đơn hàng.</summary>
public enum OrderStatus
{
    Pending = 0,
    Paid = 1,
    Processing = 2,
    Shipped = 3,
    Completed = 4,
    Cancelled = 5,
    Refunded = 6
}

/// <summary>Phương thức thanh toán.</summary>
public enum PaymentMethod
{
    COD = 0,
    MoMo = 1,
    VNPay = 2,
    OnePay = 3,
    BankTransfer = 4
}

/// <summary>Kiểu giảm giá của voucher.</summary>
public enum DiscountType
{
    Percent = 0,
    FixedAmount = 1,
    FreeShipping = 2,
    BuyXGetY = 3
}

/// <summary>Trạng thái voucher.</summary>
public enum CouponStatus
{
    Draft = 0,
    Active = 1,
    Scheduled = 2,
    Expired = 3,
    Paused = 4
}

/// <summary>Đối tượng áp dụng của voucher.</summary>
public enum CouponAudience
{
    Public = 0,
    VipOnly = 1,
    NewCustomer = 2
}

/// <summary>Hạng thành viên của khách hàng.</summary>
public enum VipTier
{
    Standard = 0,
    Silver = 1,
    Gold = 2,
    Platinum = 3
}

/// <summary>Trạng thái tài khoản khách hàng.</summary>
public enum CustomerStatus
{
    Active = 0,
    Suspended = 1
}

/// <summary>Phân loại đối tác: tác giả hoặc nhà xuất bản.</summary>
public enum PartnerType
{
    Author = 0,
    Publisher = 1
}

/// <summary>Trạng thái hợp đồng với đối tác.</summary>
public enum AgreementStatus
{
    Draft = 0,
    Active = 1,
    Expired = 2,
    Terminated = 3
}

/// <summary>Trạng thái duyệt của đánh giá sách.</summary>
public enum ReviewStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Flagged = 3,
    Spam = 4
}

/// <summary>Sắc thái của đánh giá (phân loại tự động theo số sao).</summary>
public enum Sentiment
{
    Positive = 0,
    Neutral = 1,
    Negative = 2
}

/// <summary>Trạng thái bài viết blog.</summary>
public enum ArticleStatus
{
    Draft = 0,
    InReview = 1,
    Scheduled = 2,
    Published = 3,
    Archived = 4
}
