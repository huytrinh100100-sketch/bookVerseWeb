using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Constants;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Content;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class ContentService : IContentService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public ContentService(AppDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    // ------------------------------------------------------------------ Đánh giá

    public async Task<ServiceResult<PagedResult<ReviewListItemResponse>>> SearchReviewsAsync(ReviewSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = from review in _context.Reviews.AsNoTracking()
                    join book in _context.Books.AsNoTracking() on review.BookId equals book.Id
                    join customer in _context.Users.AsNoTracking() on review.CustomerId equals customer.Id
                    select new ReviewListItemResponse
                    {
                        Id = review.Id,
                        BookId = review.BookId,
                        BookTitle = book.Title,
                        CustomerId = review.CustomerId,
                        CustomerName = customer.FullName,
                        Rating = review.Rating,
                        Title = review.Title,
                        Content = review.Content,
                        Status = review.Status,
                        Sentiment = review.Sentiment,
                        IsVerifiedPurchase = review.IsVerifiedPurchase,
                        HelpfulCount = review.HelpfulCount,
                        ReplyContent = review.ReplyContent,
                        CreatedAt = review.CreatedAt
                    };

        if (request.BookId.HasValue) query = query.Where(x => x.BookId == request.BookId.Value);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (request.Sentiment.HasValue) query = query.Where(x => x.Sentiment == request.Sentiment.Value);
        if (request.Rating.HasValue) query = query.Where(x => x.Rating == request.Rating.Value);
        if (request.IsDeleted.HasValue) query = query.Where(x => _context.Reviews.Any(r => r.Id == x.Id && r.IsDeleted == request.IsDeleted.Value));

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Content.ToLower().Contains(keyword) ||
                (x.Title != null && x.Title.ToLower().Contains(keyword)) ||
                x.BookTitle.ToLower().Contains(keyword));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<ReviewListItemResponse>>.Success(new PagedResult<ReviewListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách đánh giá thành công.");
    }

    public async Task<ServiceResult<ReviewDetailResponse>> GetReviewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (review is null)
        {
            return ServiceResult<ReviewDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đánh giá.");
        }

        var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(x => x.Id == review.BookId, cancellationToken);
        var customer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == review.CustomerId, cancellationToken);

        return ServiceResult<ReviewDetailResponse>.Success(new ReviewDetailResponse
        {
            Id = review.Id,
            BookId = review.BookId,
            BookTitle = book?.Title ?? string.Empty,
            CustomerId = review.CustomerId,
            CustomerName = customer?.FullName ?? string.Empty,
            Rating = review.Rating,
            Title = review.Title,
            Content = review.Content,
            MediaUrls = ServiceHelpers.SplitCsv(review.MediaUrls),
            Status = review.Status,
            Sentiment = review.Sentiment,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            HelpfulCount = review.HelpfulCount,
            ReplyContent = review.ReplyContent,
            RepliedBy = review.RepliedBy,
            RepliedAt = review.RepliedAt,
            IsDeleted = review.IsDeleted,
            CreatedAt = review.CreatedAt
        }, "Lấy chi tiết đánh giá thành công.");
    }

    public async Task<ServiceResult<ReviewDetailResponse>> CreateReviewAsync(CreateReviewRequest request, Guid customerId, CancellationToken cancellationToken = default)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            return ServiceResult<ReviewDetailResponse>.Failure(HttpStatusCode.BadRequest, "Điểm đánh giá phải từ 1 đến 5.");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return ServiceResult<ReviewDetailResponse>.Failure(HttpStatusCode.BadRequest, "Nội dung đánh giá không được để trống.");
        }

        var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.BookId && !x.IsDeleted, cancellationToken);
        if (book is null)
        {
            return ServiceResult<ReviewDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        var hasPurchased = await _context.OrderDetails
            .AnyAsync(x => x.BookId == request.BookId && x.Order.CustomerId == customerId && x.Order.OrderStatus == OrderStatus.Completed, cancellationToken);

        var review = new Review
        {
            Id = Guid.NewGuid(),
            BookId = request.BookId,
            CustomerId = customerId,
            Rating = request.Rating,
            Title = request.Title,
            Content = request.Content.Trim(),
            MediaUrls = string.Join(",", request.MediaUrls ?? []),
            Status = ReviewStatus.Pending,
            Sentiment = request.Rating >= 4 ? Sentiment.Positive : (request.Rating <= 2 ? Sentiment.Negative : Sentiment.Neutral),
            IsVerifiedPurchase = hasPurchased,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = customerId.ToString()
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(customerId, "Viết đánh giá", "Review", $"Viết đánh giá sách {book.Title}.");

        return await GetReviewAsync(review.Id, cancellationToken);
    }

    public async Task<ServiceResult<ReviewDetailResponse>> UpdateReviewStatusAsync(Guid id, UpdateReviewStatusRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (review is null)
        {
            return ServiceResult<ReviewDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đánh giá.");
        }

        review.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.ReplyContent))
        {
            review.ReplyContent = request.ReplyContent.Trim();
            review.RepliedBy = operatorId;
            review.RepliedAt = DateTime.UtcNow;
        }

        review.UpdatedAt = DateTime.UtcNow;
        review.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        // Cập nhật điểm trung bình của sách
        if (request.Status == ReviewStatus.Approved)
        {
            await RecalculateBookRatingAsync(review.BookId, cancellationToken);
        }

        await WriteAuditAsync(operatorId, "Duyệt đánh giá", "Review", $"Cập nhật trạng thái đánh giá sang {request.Status}.");

        return await GetReviewAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> DeleteReviewAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (review is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đánh giá.");
        }

        review.IsDeleted = true;
        review.DeletedAt = DateTime.UtcNow;
        review.DeletedBy = operatorId.ToString();
        review.UpdatedAt = DateTime.UtcNow;
        review.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await RecalculateBookRatingAsync(review.BookId, cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa đánh giá", "Review", $"Xóa đánh giá {id}.");

        return ServiceResult<bool>.Success(true, "Xóa đánh giá thành công.");
    }

    private async Task RecalculateBookRatingAsync(Guid bookId, CancellationToken cancellationToken)
    {
        var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == bookId, cancellationToken);
        if (book is null)
        {
            return;
        }

        var approved = await _context.Reviews
            .Where(x => x.BookId == bookId && !x.IsDeleted && x.Status == ReviewStatus.Approved)
            .ToListAsync(cancellationToken);

        book.ReviewCount = approved.Count;
        book.AverageRating = approved.Count > 0
            ? Math.Round((decimal)approved.Average(x => x.Rating), 2)
            : null;

        await _context.SaveChangesAsync(cancellationToken);
    }

    // ------------------------------------------------------------------ Bài viết

    public async Task<ServiceResult<PagedResult<ArticleListItemResponse>>> SearchArticlesAsync(ArticleSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = from article in _context.Articles.AsNoTracking()
                    join category in _context.CommonCategories.AsNoTracking() on article.CategoryId equals category.Id into categoryJoin
                    from category in categoryJoin.DefaultIfEmpty()
                    join author in _context.Authors.AsNoTracking() on article.AuthorId equals author.Id into authorJoin
                    from author in authorJoin.DefaultIfEmpty()
                    select new ArticleListItemResponse
                    {
                        Id = article.Id,
                        Code = article.Code,
                        Title = article.Title,
                        Slug = article.Slug,
                        Summary = article.Summary,
                        CoverImageUrl = article.CoverImageUrl,
                        CategoryId = article.CategoryId,
                        CategoryName = category != null ? category.Name : null,
                        AuthorId = article.AuthorId,
                        AuthorName = author != null ? author.Name : string.Empty,
                        ViewCount = article.ViewCount,
                        Status = article.Status,
                        PublishedAt = article.PublishedAt,
                        IsDeleted = article.IsDeleted,
                        CreatedAt = article.CreatedAt
                    };

        if (request.CategoryId.HasValue) query = query.Where(x => x.CategoryId == request.CategoryId.Value);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (request.IsDeleted.HasValue) query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Title.ToLower().Contains(keyword) ||
                (x.Summary != null && x.Summary.ToLower().Contains(keyword)) ||
                x.AuthorName.ToLower().Contains(keyword));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<ArticleListItemResponse>>.Success(new PagedResult<ArticleListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách bài viết thành công.");
    }

    public async Task<ServiceResult<ArticleDetailResponse>> GetArticleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var article = await _context.Articles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (article is null)
        {
            return ServiceResult<ArticleDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy bài viết.");
        }

        var category = article.CategoryId.HasValue
            ? await _context.CommonCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == article.CategoryId.Value, cancellationToken)
            : null;
        var author = await _context.Authors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == article.AuthorId, cancellationToken);

        return ServiceResult<ArticleDetailResponse>.Success(new ArticleDetailResponse
        {
            Id = article.Id,
            Code = article.Code,
            Title = article.Title,
            Slug = article.Slug,
            Summary = article.Summary,
            Content = article.Content,
            CoverImageUrl = article.CoverImageUrl,
            CategoryId = article.CategoryId,
            CategoryName = category?.Name,
            AuthorId = article.AuthorId,
            AuthorName = author?.Name ?? string.Empty,
            EditorId = article.EditorId,
            LinkedBookIds = ServiceHelpers.SplitCsv(article.LinkedBookIds),
            LinkedBookTitles = await ResolveBookTitlesAsync(article.LinkedBookIds, cancellationToken),
            SeoTitle = article.SeoTitle,
            SeoDescription = article.SeoDescription,
            ViewCount = article.ViewCount,
            ConversionCount = article.ConversionCount,
            Status = article.Status,
            PublishedAt = article.PublishedAt,
            IsDeleted = article.IsDeleted,
            CreatedAt = article.CreatedAt
        }, "Lấy chi tiết bài viết thành công.");
    }

    public async Task<ServiceResult<ArticleDetailResponse>> GetArticleBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var article = await _context.Articles.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug && !x.IsDeleted, cancellationToken);
        if (article is null)
        {
            return ServiceResult<ArticleDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy bài viết.");
        }

        return await GetArticleAsync(article.Id, cancellationToken);
    }

    public async Task<ServiceResult<ArticleDetailResponse>> CreateArticleAsync(CreateArticleRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        if (await _context.Articles.AnyAsync(x => x.Code == request.Code.Trim() && !x.IsDeleted, cancellationToken))
        {
            return ServiceResult<ArticleDetailResponse>.Failure(HttpStatusCode.Conflict, "Mã bài viết đã tồn tại.");
        }

        var slug = await BuildUniqueSlugAsync(request.Title, null, cancellationToken);

        var article = new Article
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Title = request.Title.Trim(),
            Slug = slug,
            Summary = request.Summary,
            Content = request.Content,
            CoverImageUrl = request.CoverImageUrl,
            CategoryId = request.CategoryId,
            AuthorId = request.AuthorId,
            LinkedBookIds = string.Join(",", request.LinkedBookIds ?? []),
            SeoTitle = request.SeoTitle,
            SeoDescription = request.SeoDescription,
            Status = request.Status,
            PublishedAt = request.Status == ArticleStatus.Published ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        _context.Articles.Add(article);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạo bài viết", "Article", $"Tạo bài viết {article.Title}.");

        return await GetArticleAsync(article.Id, cancellationToken);
    }

    public async Task<ServiceResult<ArticleDetailResponse>> UpdateArticleAsync(Guid id, UpdateArticleRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var article = await _context.Articles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (article is null)
        {
            return ServiceResult<ArticleDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy bài viết.");
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            article.Title = request.Title.Trim();
            article.Slug = await BuildUniqueSlugAsync(article.Title, article.Id, cancellationToken);
        }

        if (request.Summary is not null) article.Summary = request.Summary;
        if (request.Content is not null) article.Content = request.Content;
        if (request.CoverImageUrl is not null) article.CoverImageUrl = request.CoverImageUrl;
        if (request.CategoryId.HasValue) article.CategoryId = request.CategoryId.Value;
        if (request.AuthorId.HasValue) article.AuthorId = request.AuthorId.Value;
        if (request.EditorId.HasValue) article.EditorId = request.EditorId;
        if (request.LinkedBookIds is not null) article.LinkedBookIds = string.Join(",", request.LinkedBookIds);
        if (request.SeoTitle is not null) article.SeoTitle = request.SeoTitle;
        if (request.SeoDescription is not null) article.SeoDescription = request.SeoDescription;

        if (request.Status.HasValue)
        {
            article.Status = request.Status.Value;
            if (request.Status.Value == ArticleStatus.Published && article.PublishedAt is null)
            {
                article.PublishedAt = DateTime.UtcNow;
            }
        }

        article.UpdatedAt = DateTime.UtcNow;
        article.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Cập nhật bài viết", "Article", $"Cập nhật bài viết {article.Title}.");

        return await GetArticleAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> DeleteArticleAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var article = await _context.Articles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (article is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy bài viết.");
        }

        article.IsDeleted = true;
        article.Status = ArticleStatus.Archived;
        article.DeletedAt = DateTime.UtcNow;
        article.DeletedBy = operatorId.ToString();
        article.UpdatedAt = DateTime.UtcNow;
        article.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa bài viết", "Article", $"Xóa bài viết {article.Title}.");

        return ServiceResult<bool>.Success(true, "Xóa bài viết thành công.");
    }

    private async Task<string> BuildUniqueSlugAsync(string title, Guid? excludeId, CancellationToken cancellationToken = default)
    {
        var baseSlug = ServiceHelpers.Slugify(title);
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = "article";
        }

        var slug = baseSlug;
        var suffix = 1;

        while (await _context.Articles.AnyAsync(x => x.Slug == slug && x.Id != excludeId, cancellationToken))
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }

        return slug;
    }

    /// <summary>Tra tên sách từ danh sách id (CSV) để hiển thị linkedBookTitles.</summary>
    private async Task<IReadOnlyCollection<string>> ResolveBookTitlesAsync(string? linkedBookIds, CancellationToken cancellationToken = default)
    {
        var ids = ServiceHelpers.SplitCsv(linkedBookIds)
            .Select(x => Guid.TryParse(x, out var g) ? g : Guid.Empty)
            .Where(x => x != Guid.Empty)
            .ToList();

        if (ids.Count == 0)
        {
            return [];
        }

        return await _context.Books
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    private async Task WriteAuditAsync(Guid? operatorId, string action, string module, string note)
    {
        await _auditLogService.WriteAsync(new AuditLog
        {
            ActorUserId = operatorId,
            Action = action,
            Module = module,
            Note = note,
            EntityType = module,
            IsSuccess = true
        });
    }

    // ------------------------------------------------------------------ Duyệt / từ chối / cờ đánh giá (FE: /reviews/*)

    public async Task<ServiceResult<ReviewListItemResponse>> ApproveReviewAsync(Guid id, ApproveReviewRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (review is null)
        {
            return ServiceResult<ReviewListItemResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đánh giá.");
        }

        review.Status = ReviewStatus.Approved;

        if (!string.IsNullOrWhiteSpace(request.ReplyContent))
        {
            review.ReplyContent = request.ReplyContent.Trim();
            review.RepliedBy = operatorId;
            review.RepliedAt = DateTime.UtcNow;
        }

        review.UpdatedAt = DateTime.UtcNow;
        review.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await RecalculateBookRatingAsync(review.BookId, cancellationToken);

        await WriteAuditAsync(operatorId, "Duyệt đánh giá", "Review", $"Duyệt đánh giá {id}.");

        return await GetReviewListItemAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<ReviewListItemResponse>> RejectReviewAsync(Guid id, RejectReviewRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (review is null)
        {
            return ServiceResult<ReviewListItemResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đánh giá.");
        }

        review.Status = ReviewStatus.Rejected;
        review.ReplyContent = string.IsNullOrWhiteSpace(request.Reason) ? review.ReplyContent : request.Reason.Trim();
        review.RepliedBy = operatorId;
        review.RepliedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;
        review.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await RecalculateBookRatingAsync(review.BookId, cancellationToken);

        await WriteAuditAsync(operatorId, "Từ chối đánh giá", "Review", $"Từ chối đánh giá {id}. Lý do: {request.Reason}");

        return await GetReviewListItemAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<ReviewListItemResponse>> FlagReviewAsync(Guid id, FlagReviewRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (review is null)
        {
            return ServiceResult<ReviewListItemResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đánh giá.");
        }

        review.Status = ReviewStatus.Flagged;
        review.ReplyContent = string.IsNullOrWhiteSpace(request.Reason) ? review.ReplyContent : request.Reason.Trim();
        review.RepliedBy = operatorId;
        review.RepliedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;
        review.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await RecalculateBookRatingAsync(review.BookId, cancellationToken);

        await WriteAuditAsync(operatorId, "Đánh dấu vi phạm", "Review", $"Đánh dấu vi phạm đánh giá {id}. Lý do: {request.Reason}");

        return await GetReviewListItemAsync(id, cancellationToken);
    }

    private async Task<ServiceResult<ReviewListItemResponse>> GetReviewListItemAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _context.Reviews
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ReviewListItemResponse
            {
                Id = x.Id,
                BookId = x.BookId,
                BookTitle = x.Book.Title,
                CoverImageUrl = x.Book.CoverImageUrl,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer.FullName,
                Rating = x.Rating,
                Title = x.Title,
                Content = x.Content,
                Status = x.Status,
                Sentiment = x.Sentiment,
                HasMedia = x.MediaUrls != null && x.MediaUrls.Length > 0,
                MediaUrls = ServiceHelpers.SplitCsv(x.MediaUrls),
                IsVerifiedPurchase = x.IsVerifiedPurchase,
                HelpfulCount = x.HelpfulCount,
                ReplyContent = x.ReplyContent,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return ServiceResult<ReviewListItemResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đánh giá.");
        }

        return ServiceResult<ReviewListItemResponse>.Success(item, "Lấy đánh giá thành công.");
    }

    // ------------------------------------------------------------------ Xuất bản bài viết (FE: /articles/{id}/publish)

    public async Task<ServiceResult<ArticleDetailResponse>> PublishArticleAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var article = await _context.Articles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (article is null)
        {
            return ServiceResult<ArticleDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy bài viết.");
        }

        article.Status = ArticleStatus.Published;
        article.PublishedAt ??= DateTime.UtcNow;
        article.UpdatedAt = DateTime.UtcNow;
        article.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xuất bản bài viết", "Article", $"Xuất bản bài viết {article.Title}.");

        return await GetArticleAsync(id, cancellationToken);
    }
}
