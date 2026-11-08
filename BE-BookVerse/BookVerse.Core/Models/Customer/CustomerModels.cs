using BookVerse.Core.Constants;
using BookVerse.Core.Models.Order;

namespace BookVerse.Core.Models.Customer;

public class CustomerSearchRequest
{
    public string? Keyword { get; set; }
    public VipTier? VipTier { get; set; }
    public CustomerStatus? Status { get; set; }
    public Guid? ProvinceId { get; set; }
    public decimal? MinLifetimeSpend { get; set; }
    public decimal? MaxLifetimeSpend { get; set; }
    public bool? IsDeleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CustomerListItemResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public Guid? CommuneId { get; set; }
    public string? CommuneName { get; set; }
    public string? Address { get; set; }
    public VipTier VipTier { get; set; }
    public int OrderCount { get; set; }
    public decimal LifetimeSpend { get; set; }
    public string? PreferredCategory { get; set; }
    public CustomerStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastOrderDate { get; set; }
}

public class CustomerDetailResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Address { get; set; }
    public Guid? ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public Guid? CommuneId { get; set; }
    public string? CommuneName { get; set; }
    public DateTime? Birthday { get; set; }
    public VipTier VipTier { get; set; }
    public CustomerStatus Status { get; set; }
    public int OrderCount { get; set; }
    public decimal LifetimeSpend { get; set; }
    public string? PreferredCategory { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int RewardPoints { get; set; }
    public int WishlistBookCount { get; set; }
    public int AddressCount { get; set; }
    public IReadOnlyCollection<OrderListItemResponse> RecentOrders { get; set; } = [];
    public IReadOnlyCollection<CustomerAddressResponse> Addresses { get; set; } = [];
}

public class CreateCustomerRequest
{
    public string? UserName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? CommuneId { get; set; }
    public DateTime? Birthday { get; set; }
    public VipTier VipTier { get; set; } = VipTier.Standard;
    public string? Note { get; set; }
}

public class UpdateCustomerRequest
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? CommuneId { get; set; }
    public DateTime? Birthday { get; set; }
    public VipTier? VipTier { get; set; }
    public CustomerStatus? Status { get; set; }
    public Guid? PreferredCategoryId { get; set; }
    public string? Note { get; set; }
    public int? RewardPoints { get; set; }
}

public class CustomerAddressResponse
{
    public Guid Id { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Guid ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public Guid CommuneId { get; set; }
    public string? CommuneName { get; set; }
    public bool IsDefault { get; set; }
    public string? Note { get; set; }
}

public class CreateCustomerAddressRequest
{
    public string RecipientName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Guid ProvinceId { get; set; }
    public Guid CommuneId { get; set; }
    public bool IsDefault { get; set; }
    public string? Note { get; set; }
}

public class UpdateCustomerAddressRequest
{
    public string? RecipientName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? CommuneId { get; set; }
    public bool? IsDefault { get; set; }
    public string? Note { get; set; }
}

public class SendVoucherRequest
{
    public Guid CustomerId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public string? Note { get; set; }
}

/// <summary>FE: POST /customers/{id}/voucher - gửi voucher cho một khách hàng cụ thể.</summary>
public class SendVoucherToCustomerRequest
{
    public string CouponCode { get; set; } = string.Empty;
    public string? Note { get; set; }
}

/// <summary>Xuất dữ liệu PII khách hàng (FE: POST /customers/export-pii).</summary>
public class ExportPiiRequest
{
    public List<Guid> Ids { get; set; } = [];
}

public class ExportPiiResponse
{
    public int TotalCustomers { get; set; }
    public IReadOnlyCollection<CustomerPiiRow> Rows { get; set; } = [];
    public DateTime ExportedAt { get; set; }
}

public class CustomerPiiRow
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? ProvinceName { get; set; }
    public string? CommuneName { get; set; }
    public DateTime? Birthday { get; set; }
}
