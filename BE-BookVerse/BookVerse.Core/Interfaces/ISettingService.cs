using BookVerse.Core.Common;
using BookVerse.Core.Models.Setting;

namespace BookVerse.Core.Interfaces;

public interface ISettingService
{
    Task<ServiceResult<IReadOnlyCollection<SettingItemResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<SettingItemResponse>> UpdateAsync(UpdateSettingRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<PaymentGatewayResponse>>> GetPaymentGatewaysAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<CarrierResponse>>> GetCarriersAsync(CancellationToken cancellationToken = default);

    /// <summary>Cấu hình cửa hàng dạng nhóm (FE: GET /settings/store).</summary>
    Task<ServiceResult<StoreSettingResponse>> GetStoreAsync(CancellationToken cancellationToken = default);

    /// <summary>Cập nhật cấu hình cửa hàng (FE: PUT /settings/store).</summary>
    Task<ServiceResult<StoreSettingResponse>> UpdateStoreAsync(UpdateStoreSettingRequest request, Guid operatorId, CancellationToken cancellationToken = default);
}
