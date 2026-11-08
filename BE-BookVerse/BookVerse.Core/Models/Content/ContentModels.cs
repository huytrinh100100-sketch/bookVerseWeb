using BookVerse.Core.Constants;

namespace BookVerse.Core.Models.Content;

public class ReviewSearchRequest
{
    public string? Keyword { get; set; }
    public Guid? BookId { get; set; }
    public ReviewStatus? Status { get; set; }
    public Sentiment? Sentiment { get; set; }
    public int? Rating { get; set; }
    public bool? IsDeleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ReviewListItemResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public ReviewStatus Status { get; set; }
    public Sentiment Sentiment { get; set; }
    public bool HasMedia { get; set; }
    public IReadOnlyCollection<string> MediaUrls { get; set; } = [];
    public bool IsVerifiedPurchase { get; set; }
    public int HelpfulCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ReplyContent { get; set; }
}

public class ReviewDetailResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool HasMedia { get; set; }
    public IReadOnlyCollection<string> MediaUrls { get; set; } = [];
    public ReviewStatus Status { get; set; }
    public Sentiment Sentiment { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public int HelpfulCount { get; set; }
    public string? ReplyContent { get; set; }
    public Guid? RepliedBy { get; set; }
    public DateTime? RepliedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateReviewStatusRequest
{
    public ReviewStatus Status { get; set; }
    public string? ReplyContent { get; set; }
}

/// <summary>FE: PUT /reviews/{id}/approve hoac /reject hoac /flag.</summary>
public class ApproveReviewRequest
{
    public string? ReplyContent { get; set; }
}

public class RejectReviewRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class FlagReviewRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class CreateReviewRequest
{
    public Guid BookId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public IReadOnlyCollection<string> MediaUrls { get; set; } = [];
}

public class ArticleSearchRequest
{
    public string? Keyword { get; set; }
    public Guid? CategoryId { get; set; }
    public ArticleStatus? Status { get; set; }
    public bool? IsDeleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ArticleListItemResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? CoverImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public IReadOnlyCollection<string> LinkedBookIds { get; set; } = [];
    public IReadOnlyCollection<string> LinkedBookTitles { get; set; } = [];
    public Guid? AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public Guid? EditorId { get; set; }
    public string? EditorName { get; set; }
    public int ViewCount { get; set; }
    public int ConversionCount { get; set; }
    public ArticleStatus Status { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ArticleDetailResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? CoverImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public IReadOnlyCollection<string> LinkedBookIds { get; set; } = [];
    public IReadOnlyCollection<string> LinkedBookTitles { get; set; } = [];
    public Guid? AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public Guid? EditorId { get; set; }
    public string? EditorName { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public int ViewCount { get; set; }
    public int ConversionCount { get; set; }
    public ArticleStatus Status { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateArticleRequest
{
    public string? Code { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? CoverImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? AuthorId { get; set; }
    public IReadOnlyCollection<string> LinkedBookIds { get; set; } = [];
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    public DateTime? PublishedAt { get; set; }
}

public class UpdateArticleRequest
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? CoverImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? AuthorId { get; set; }
    public Guid? EditorId { get; set; }
    public IReadOnlyCollection<string> LinkedBookIds { get; set; } = [];
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public ArticleStatus? Status { get; set; }
    public DateTime? PublishedAt { get; set; }
}
