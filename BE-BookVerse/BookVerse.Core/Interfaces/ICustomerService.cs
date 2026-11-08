using BookVerse.Core.Common;
using BookVerse.Core.Models.Customer;

namespace BookVerse.Core.Interfaces;

public interface ICustomerService
{
    Task<ServiceResult<PagedResult<CustomerListItemResponse>>> SearchAsync(CustomerSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<CustomerDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<CustomerDetailResponse>> CreateAsync(CreateCustomerRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<CustomerDetailResponse>> UpdateAsync(Guid id, UpdateCustomerRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> SendVoucherAsync(SendVoucherRequest request, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>Gửi voucher cho một khách cụ thể (FE: POST /customers/{id}/voucher).</summary>
    Task<ServiceResult<bool>> SendVoucherToCustomerAsync(Guid customerId, string couponCode, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>Xuất dữ liệu PII của các khách hàng (FE: POST /customers/export-pii).</summary>
    Task<ServiceResult<ExportPiiResponse>> ExportPiiAsync(ExportPiiRequest request, Guid operatorId, CancellationToken cancellationToken = default);

    // --- Sổ địa chỉ ---
    Task<ServiceResult<IReadOnlyCollection<CustomerAddressResponse>>> GetAddressesAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ServiceResult<CustomerAddressResponse>> CreateAddressAsync(Guid customerId, CreateCustomerAddressRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<CustomerAddressResponse>> UpdateAddressAsync(Guid addressId, UpdateCustomerAddressRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAddressAsync(Guid addressId, Guid operatorId, CancellationToken cancellationToken = default);
}
