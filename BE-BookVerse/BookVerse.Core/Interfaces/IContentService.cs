using BookVerse.Core.Common;
using BookVerse.Core.Models.Content;

namespace BookVerse.Core.Interfaces;

public interface IContentService
{
    // --- Đánh giá ---
    Task<ServiceResult<PagedResult<ReviewListItemResponse>>> SearchReviewsAsync(ReviewSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<ReviewDetailResponse>> GetReviewAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<ReviewDetailResponse>> CreateReviewAsync(CreateReviewRequest request, Guid customerId, CancellationToken cancellationToken = default);
    Task<ServiceResult<ReviewDetailResponse>> UpdateReviewStatusAsync(Guid id, UpdateReviewStatusRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteReviewAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>Duyệt đánh giá (FE: PUT /reviews/{id}/approve).</summary>
    Task<ServiceResult<ReviewListItemResponse>> ApproveReviewAsync(Guid id, ApproveReviewRequest request, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>Từ chối đánh giá (FE: PUT /reviews/{id}/reject).</summary>
    Task<ServiceResult<ReviewListItemResponse>> RejectReviewAsync(Guid id, RejectReviewRequest request, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>Đánh dấu đánh giá vi phạm (FE: PUT /reviews/{id}/flag).</summary>
    Task<ServiceResult<ReviewListItemResponse>> FlagReviewAsync(Guid id, FlagReviewRequest request, Guid operatorId, CancellationToken cancellationToken = default);

    // --- Bài viết ---
    Task<ServiceResult<PagedResult<ArticleListItemResponse>>> SearchArticlesAsync(ArticleSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<ArticleDetailResponse>> GetArticleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<ArticleDetailResponse>> GetArticleBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<ServiceResult<ArticleDetailResponse>> CreateArticleAsync(CreateArticleRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<ArticleDetailResponse>> UpdateArticleAsync(Guid id, UpdateArticleRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteArticleAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>Xuất bản bài viết (FE: PUT /articles/{id}/publish).</summary>
    Task<ServiceResult<ArticleDetailResponse>> PublishArticleAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
}
