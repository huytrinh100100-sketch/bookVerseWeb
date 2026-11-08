using BookVerse.Core.Common;
using BookVerse.Core.Models.Partner;

namespace BookVerse.Core.Interfaces;

public interface IPartnerService
{
    // --- Tác giả ---
    Task<ServiceResult<PagedResult<AuthorListItemResponse>>> SearchAuthorsAsync(PartnerSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<AuthorDetailResponse>> GetAuthorAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<AuthorDetailResponse>> CreateAuthorAsync(CreateAuthorRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<AuthorDetailResponse>> UpdateAuthorAsync(Guid id, UpdateAuthorRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAuthorAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);

    // --- Nhà xuất bản ---
    Task<ServiceResult<PagedResult<PublisherListItemResponse>>> SearchPublishersAsync(PartnerSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<PublisherDetailResponse>> GetPublisherAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<PublisherDetailResponse>> CreatePublisherAsync(CreatePublisherRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<PublisherDetailResponse>> UpdatePublisherAsync(Guid id, UpdatePublisherRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeletePublisherAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);

    // --- Đối tác hợp nhất (tác giả + NXB) theo FE: /api/v1/partners ---
    Task<ServiceResult<PagedResult<PartnerListItemResponse>>> SearchPartnersAsync(PartnerSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<PartnerDetailResponse>> GetPartnerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<PartnerDetailResponse>> CreatePartnerAsync(CreatePartnerRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<PartnerDetailResponse>> UpdatePartnerAsync(Guid id, UpdatePartnerRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeletePartnerAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
}
