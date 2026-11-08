using BookVerse.Core.Common;
using BookVerse.Core.Models.Geography;

namespace BookVerse.Core.Interfaces;

public interface ICommuneService
{
    Task<ServiceResult<PagedResult<CommuneListItemResponse>>> SearchAsync(CommuneSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<CommuneDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<CommuneListItemResponse>>> GetByProvinceAsync(Guid provinceId, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<ServiceResult<CommuneDetailResponse>> CreateAsync(CreateCommuneRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<CommuneDetailResponse>> UpdateAsync(Guid id, UpdateCommuneRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
}
