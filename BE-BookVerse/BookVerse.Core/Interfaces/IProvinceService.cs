using BookVerse.Core.Common;
using BookVerse.Core.Models.Geography;

namespace BookVerse.Core.Interfaces;

public interface IProvinceService
{
    Task<ServiceResult<PagedResult<ProvinceListItemResponse>>> SearchAsync(ProvinceSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<ProvinceDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<ProvinceListItemResponse>>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<ServiceResult<ProvinceDetailResponse>> CreateAsync(CreateProvinceRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<ProvinceDetailResponse>> UpdateAsync(Guid id, UpdateProvinceRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
}
