using BookVerse.Core.Common;
using BookVerse.Core.Models.Coupon;

namespace BookVerse.Core.Interfaces;

public interface ICouponService
{
    Task<ServiceResult<PagedResult<CouponListItemResponse>>> SearchAsync(CouponSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<CouponDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<CouponDetailResponse>> CreateAsync(CreateCouponRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<CouponDetailResponse>> UpdateAsync(Guid id, UpdateCouponRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> HardDeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<ValidateCouponResponse>> ValidateAsync(ValidateCouponRequest request, CancellationToken cancellationToken = default);

    /// <summary>Tra cứu voucher theo mã (FE: GET /coupons/validate/{code}).</summary>
    Task<ServiceResult<CouponDetailResponse>> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>Tạm dừng voucher (FE: PUT /coupons/{id}/pause).</summary>
    Task<ServiceResult<CouponDetailResponse>> PauseAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);

    // --- Chiến dịch ---
    Task<ServiceResult<PagedResult<CampaignListItemResponse>>> SearchCampaignsAsync(CampaignSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<CampaignDetailResponse>> GetCampaignAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<CampaignDetailResponse>> CreateCampaignAsync(CreateCampaignRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<CampaignDetailResponse>> UpdateCampaignAsync(Guid id, UpdateCampaignRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteCampaignAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
}
