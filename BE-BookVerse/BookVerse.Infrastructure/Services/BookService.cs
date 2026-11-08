using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Constants;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Book;
using BookVerse.Core.Models.Content;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class BookService : IBookService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public BookService(AppDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<PagedResult<BookListItemResponse>>> SearchAsync(BookSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = BuildQuery(request);

        var totalItems = await query.CountAsync(cancellationToken);

        query = ApplySort(query, request.SortBy, request.SortDescending);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<BookListItemResponse>>.Success(new PagedResult<BookListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách sách thành công.");
    }

    public async Task<ServiceResult<BookDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var book = await BuildDetailQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (book is null)
        {
            return ServiceResult<BookDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        var copies = await _context.BookCopies
            .AsNoTracking()
            .Where(x => x.BookId == id && !x.IsDeleted)
            .OrderBy(x => x.Barcode)
            .Select(x => new BookCopyListItemResponse
            {
                Id = x.Id,
                BookId = x.BookId,
                Barcode = x.Barcode,
                ConditionStatus = x.ConditionStatus,
                ShelfCode = x.ShelfCode,
                WarehouseId = x.WarehouseId,
                AcquiredDate = x.AcquiredDate,
                DueDate = x.DueDate,
                ReturnedDate = x.ReturnedDate,
                Status = x.Status,
                CurrentOrderId = x.CurrentOrderId,
                Notes = x.Notes,
                IsDeleted = x.IsDeleted
            })
            .ToListAsync(cancellationToken);

        var reviews = await _context.Reviews
            .AsNoTracking()
            .Where(x => x.BookId == id && !x.IsDeleted && x.Status == ReviewStatus.Approved)
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .Select(x => new ReviewListItemResponse
            {
                Id = x.Id,
                BookId = x.BookId,
                BookTitle = x.Book.Title,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer.FullName,
                Rating = x.Rating,
                Title = x.Title,
                Content = x.Content,
                Status = x.Status,
                Sentiment = x.Sentiment,
                IsVerifiedPurchase = x.IsVerifiedPurchase,
                HelpfulCount = x.HelpfulCount,
                ReplyContent = x.ReplyContent,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<BookDetailResponse>.Success(MapDetail(book, copies, reviews), "Lấy chi tiết sách thành công.");
    }

    public async Task<ServiceResult<BookDetailResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var book = await BuildDetailQuery().FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
        if (book is null)
        {
            return ServiceResult<BookDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        var copies = await _context.BookCopies
            .AsNoTracking()
            .Where(x => x.BookId == book.Id && !x.IsDeleted)
            .Select(x => new BookCopyListItemResponse
            {
                Id = x.Id,
                BookId = x.BookId,
                Barcode = x.Barcode,
                ConditionStatus = x.ConditionStatus,
                ShelfCode = x.ShelfCode,
                WarehouseId = x.WarehouseId,
                AcquiredDate = x.AcquiredDate,
                DueDate = x.DueDate,
                ReturnedDate = x.ReturnedDate,
                Status = x.Status,
                CurrentOrderId = x.CurrentOrderId,
                Notes = x.Notes,
                IsDeleted = x.IsDeleted
            })
            .ToListAsync(cancellationToken);

        var reviews = await _context.Reviews
            .AsNoTracking()
            .Where(x => x.BookId == book.Id && !x.IsDeleted && x.Status == ReviewStatus.Approved)
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .Select(x => new ReviewListItemResponse
            {
                Id = x.Id,
                BookId = x.BookId,
                BookTitle = x.Book.Title,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer.FullName,
                Rating = x.Rating,
                Title = x.Title,
                Content = x.Content,
                Status = x.Status,
                Sentiment = x.Sentiment,
                IsVerifiedPurchase = x.IsVerifiedPurchase,
                HelpfulCount = x.HelpfulCount,
                ReplyContent = x.ReplyContent,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<BookDetailResponse>.Success(MapDetail(book, copies, reviews), "Lấy chi tiết sách thành công.");
    }

    public async Task<ServiceResult<BookDetailResponse>> CreateAsync(CreateBookRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var code = string.IsNullOrWhiteSpace(request.Code)
            ? ServiceHelpers.BuildBookCode(await _context.Books.CountAsync(cancellationToken) + 1)
            : request.Code.Trim();

        if (await _context.Books.AnyAsync(x => x.Code == code && !x.IsDeleted, cancellationToken))
        {
            return ServiceResult<BookDetailResponse>.Failure(HttpStatusCode.Conflict, "Mã sách đã tồn tại.");
        }

        var slug = await BuildUniqueSlugAsync(request.Title, null, cancellationToken);

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Code = code,
            Title = request.Title.Trim(),
            Subtitle = request.Subtitle,
            Slug = slug,
            Isbn = request.Isbn,
            Description = request.Description,
            CoverImageUrl = request.CoverImageUrl,
            CategoryId = request.CategoryId,
            AuthorId = request.AuthorId,
            PublisherId = request.PublisherId,
            PriceForSell = ServiceHelpers.RoundMoney(request.PriceForSell),
            PriceForRent = ServiceHelpers.RoundMoney(request.PriceForRent),
            PageCount = request.PageCount,
            PublicationYear = request.PublicationYear,
            Language = request.Language,
            Format = request.Format,
            Weight = request.Weight?.ToString(),
            Dimensions = request.Dimensions,
            Status = request.Status,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        _context.Books.Add(book);

        // Tạo bản sao vật lý nếu có số lượng nhập kèm
        if (request.Quantity > 0)
        {
            for (var i = 1; i <= request.Quantity; i++)
            {
                _context.BookCopies.Add(new BookCopy
                {
                    Id = Guid.NewGuid(),
                    BookId = book.Id,
                    Barcode = ServiceHelpers.BuildBarcode(book.Id, i),
                    ConditionStatus = "Mới",
                    Status = BookCopyStatus.Available,
                    AcquiredDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = operatorId.ToString()
                });
            }

            book.TotalQuantity = request.Quantity;
            book.StockForSell = request.Quantity;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạo sách", "Book", $"Tạo sách {book.Title} ({book.Code}).");

        return await GetDetailAsync(book.Id, cancellationToken);
    }

    public async Task<ServiceResult<BookDetailResponse>> UpdateAsync(Guid id, UpdateBookRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (book is null)
        {
            return ServiceResult<BookDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        if (request.RowVersion is not null && book.RowVersion is not null)
        {
            _context.Entry(book).Property(x => x.RowVersion).OriginalValue = request.RowVersion;
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            book.Title = request.Title.Trim();
            book.Slug = await BuildUniqueSlugAsync(book.Title, book.Id, cancellationToken);
        }

        if (request.Subtitle is not null) book.Subtitle = request.Subtitle;
        if (request.Isbn is not null) book.Isbn = request.Isbn;
        if (request.Description is not null) book.Description = request.Description;
        if (request.CoverImageUrl is not null) book.CoverImageUrl = request.CoverImageUrl;
        if (request.CategoryId.HasValue) book.CategoryId = request.CategoryId;
        if (request.AuthorId.HasValue) book.AuthorId = request.AuthorId;
        if (request.PublisherId.HasValue) book.PublisherId = request.PublisherId;
        if (request.PriceForSell.HasValue) book.PriceForSell = ServiceHelpers.RoundMoney(request.PriceForSell.Value);
        if (request.PriceForRent.HasValue) book.PriceForRent = ServiceHelpers.RoundMoney(request.PriceForRent.Value);
        if (request.PageCount.HasValue) book.PageCount = request.PageCount.Value;
        if (request.PublicationYear.HasValue) book.PublicationYear = request.PublicationYear;
        if (request.Language is not null) book.Language = request.Language;
        if (request.Format is not null) book.Format = request.Format;
        if (request.Weight is not null) book.Weight = request.Weight.Value.ToString();
        if (request.Dimensions is not null) book.Dimensions = request.Dimensions;
        if (request.Status.HasValue) book.Status = request.Status.Value;
        if (request.IsFeatured.HasValue) book.IsFeatured = request.IsFeatured.Value;
        if (request.IsActive.HasValue) book.IsActive = request.IsActive.Value;

        book.UpdatedAt = DateTime.UtcNow;
        book.UpdatedBy = operatorId.ToString();

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<BookDetailResponse>.Failure(HttpStatusCode.Conflict, "Sách đã bị thay đổi bởi người khác. Vui lòng tải lại trang.");
        }

        await WriteAuditAsync(operatorId, "Cập nhật sách", "Book", $"Cập nhật sách {book.Title}.");

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<BookDetailResponse>> UpdateStatusAsync(Guid id, UpdateBookStatusRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (book is null)
        {
            return ServiceResult<BookDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        book.IsActive = request.IsActive;

        if (request.Status.HasValue)
        {
            book.Status = request.Status.Value;
        }
        else if (!request.IsActive && book.Status == BookStatus.Published)
        {
            // Tắt hoạt động thì đưa sách về trạng thái nháp để không hiển thị ngoài storefront.
            book.Status = BookStatus.Draft;
        }

        book.UpdatedAt = DateTime.UtcNow;
        book.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Đổi trạng thái sách", "Book", $"Đổi trạng thái sách {book.Title} sang {(request.IsActive ? "kích hoạt" : "ngừng hoạt động")}.");

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (book is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        book.IsDeleted = true;
        book.DeletedAt = DateTime.UtcNow;
        book.DeletedBy = operatorId.ToString();
        book.IsActive = false;
        book.UpdatedAt = DateTime.UtcNow;
        book.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa mềm sách", "Book", $"Xóa mềm sách {book.Title}.");

        return ServiceResult<bool>.Success(true, "Xóa mềm sách thành công.");
    }

    public async Task<ServiceResult<bool>> HardDeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (book is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        var hasOrders = await _context.OrderDetails.AnyAsync(x => x.BookId == id, cancellationToken);
        if (hasOrders)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Sách đã có trong đơn hàng, không thể xóa vĩnh viễn.");
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa vĩnh viễn sách", "Book", $"Xóa vĩnh viễn sách {book.Title}.");

        return ServiceResult<bool>.Success(true, "Xóa vĩnh viễn sách thành công.");
    }

    public async Task<ServiceResult<bool>> RestoreAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (book is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        book.IsDeleted = false;
        book.DeletedAt = null;
        book.DeletedBy = null;
        book.IsActive = true;
        book.UpdatedAt = DateTime.UtcNow;
        book.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Khôi phục sách", "Book", $"Khôi phục sách {book.Title}.");

        return ServiceResult<bool>.Success(true, "Khôi phục sách thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<BookListItemResponse>>> GetFeaturedAsync(int take = 8, CancellationToken cancellationToken = default)
    {
        var items = await _context.Books
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && x.Status == BookStatus.Published)
            .OrderByDescending(x => x.IsFeatured)
            .ThenByDescending(x => x.SoldCount)
            .Take(take)
            .Select(x => new BookListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Slug = x.Slug,
                CoverImageUrl = x.CoverImageUrl,
                AuthorName = x.Author!.Name,
                PublisherName = x.Publisher!.Name,
                CategoryName = x.Category!.Name,
                PriceForSell = x.PriceForSell,
                PriceForRent = x.PriceForRent,
                StockForSell = x.StockForSell,
                StockForRent = x.StockForRent,
                TotalQuantity = x.TotalQuantity,
                Status = x.Status,
                IsActive = x.IsActive,
                IsFeatured = x.IsFeatured,
                AverageRating = x.AverageRating,
                ReviewCount = x.ReviewCount,
                SoldCount = x.SoldCount,
                IsDeleted = x.IsDeleted,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<BookListItemResponse>>.Success(items, "Lấy sách nổi bật thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<BookListItemResponse>>> GetNewestAsync(int take = 8, CancellationToken cancellationToken = default)
    {
        var items = await _context.Books
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && x.Status == BookStatus.Published)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new BookListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Slug = x.Slug,
                CoverImageUrl = x.CoverImageUrl,
                AuthorName = x.Author!.Name,
                PublisherName = x.Publisher!.Name,
                CategoryName = x.Category!.Name,
                PriceForSell = x.PriceForSell,
                PriceForRent = x.PriceForRent,
                StockForSell = x.StockForSell,
                StockForRent = x.StockForRent,
                TotalQuantity = x.TotalQuantity,
                Status = x.Status,
                IsActive = x.IsActive,
                IsFeatured = x.IsFeatured,
                AverageRating = x.AverageRating,
                ReviewCount = x.ReviewCount,
                SoldCount = x.SoldCount,
                IsDeleted = x.IsDeleted,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<BookListItemResponse>>.Success(items, "Lấy sách mới thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<BookListItemResponse>>> GetBestSellersAsync(int take = 8, CancellationToken cancellationToken = default)
    {
        var items = await _context.Books
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && x.Status == BookStatus.Published)
            .OrderByDescending(x => x.SoldCount)
            .ThenByDescending(x => x.AverageRating)
            .Take(take)
            .Select(x => new BookListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Slug = x.Slug,
                CoverImageUrl = x.CoverImageUrl,
                AuthorName = x.Author!.Name,
                PublisherName = x.Publisher!.Name,
                CategoryName = x.Category!.Name,
                PriceForSell = x.PriceForSell,
                PriceForRent = x.PriceForRent,
                StockForSell = x.StockForSell,
                StockForRent = x.StockForRent,
                TotalQuantity = x.TotalQuantity,
                Status = x.Status,
                IsActive = x.IsActive,
                IsFeatured = x.IsFeatured,
                AverageRating = x.AverageRating,
                ReviewCount = x.ReviewCount,
                SoldCount = x.SoldCount,
                IsDeleted = x.IsDeleted,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<BookListItemResponse>>.Success(items, "Lấy sách bán chạy thành công.");
    }

    // ------------------------------------------------------------------ Helpers

    private IQueryable<BookListItemResponse> BuildQuery(BookSearchRequest request)
    {
        var query = from book in _context.Books.AsNoTracking()
                    join author in _context.Authors.AsNoTracking() on book.AuthorId equals author.Id into authorJoin
                    from author in authorJoin.DefaultIfEmpty()
                    join publisher in _context.Publishers.AsNoTracking() on book.PublisherId equals publisher.Id into publisherJoin
                    from publisher in publisherJoin.DefaultIfEmpty()
                    join category in _context.CommonCategories.AsNoTracking() on book.CategoryId equals category.Id into categoryJoin
                    from category in categoryJoin.DefaultIfEmpty()
                    select new BookListItemResponse
                    {
                        Id = book.Id,
                        Code = book.Code,
                        Title = book.Title,
                        Subtitle = book.Subtitle,
                        Slug = book.Slug,
                        Isbn = book.Isbn,
                        CoverImageUrl = book.CoverImageUrl,
                        CategoryId = book.CategoryId,
                        CategoryName = category != null ? category.Name : null,
                        AuthorId = book.AuthorId,
                        AuthorName = author != null ? author.Name : null,
                        PublisherId = book.PublisherId,
                        PublisherName = publisher != null ? publisher.Name : null,
                        PriceForSell = book.PriceForSell,
                        PriceForRent = book.PriceForRent,
                        PageCount = book.PageCount,
                        StockForSell = book.StockForSell,
                        StockForRent = book.StockForRent,
                        TotalQuantity = book.TotalQuantity,
                        Status = book.Status,
                        IsActive = book.IsActive,
                        IsFeatured = book.IsFeatured,
                        AverageRating = book.AverageRating,
                        ReviewCount = book.ReviewCount,
                        SoldCount = book.SoldCount,
                        IsDeleted = book.IsDeleted,
                        CreatedAt = book.CreatedAt
                    };

        query = query.Where(x => !x.IsDeleted);

        if (request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Status.Value);
        }

        if (request.IsDeleted.HasValue)
        {
            query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == request.CategoryId.Value);
        }

        if (request.AuthorId.HasValue)
        {
            query = query.Where(x => x.AuthorId == request.AuthorId.Value);
        }

        if (request.PublisherId.HasValue)
        {
            query = query.Where(x => x.PublisherId == request.PublisherId.Value);
        }

        if (request.UnitType == UnitType.Sell)
        {
            query = query.Where(x => x.StockForSell > 0);
        }
        else if (request.UnitType == UnitType.Rent)
        {
            query = query.Where(x => x.StockForRent > 0);
        }

        if (request.MinPrice.HasValue)
        {
            var price = request.UnitType == UnitType.Rent ? request.MinPrice.Value : request.MinPrice.Value;
            query = query.Where(x => x.PriceForSell >= price || x.PriceForRent >= price);
        }

        if (request.MaxPrice.HasValue)
        {
            var price = request.MaxPrice.Value;
            query = query.Where(x => x.PriceForSell <= price || x.PriceForRent <= price);
        }

        if (request.InStock == true)
        {
            query = query.Where(x => x.StockForSell > 0 || x.StockForRent > 0);
        }

        if (request.IsFeatured.HasValue)
        {
            query = query.Where(x => x.IsFeatured == request.IsFeatured.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Title.ToLower().Contains(keyword) ||
                (x.Code != null && x.Code.ToLower().Contains(keyword)) ||
                (x.AuthorName != null && x.AuthorName.ToLower().Contains(keyword)) ||
                (x.PublisherName != null && x.PublisherName.ToLower().Contains(keyword)) ||
                (x.CategoryName != null && x.CategoryName.ToLower().Contains(keyword)));
        }

        return query;
    }

    private IQueryable<BookListItemResponse> ApplySort(IQueryable<BookListItemResponse> query, string? sortBy, bool descending)
    {
        return (sortBy?.ToLowerInvariant()) switch
        {
            "title" => descending ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
            "price" => descending ? query.OrderByDescending(x => x.PriceForSell) : query.OrderBy(x => x.PriceForSell),
            "rating" => descending ? query.OrderByDescending(x => x.AverageRating) : query.OrderBy(x => x.AverageRating),
            "sold" => descending ? query.OrderByDescending(x => x.SoldCount) : query.OrderBy(x => x.SoldCount),
            "stock" => descending ? query.OrderByDescending(x => x.StockForSell) : query.OrderBy(x => x.StockForSell),
            _ => descending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
        };
    }

    private IQueryable<Book> BuildDetailQuery()
    {
        return from book in _context.Books.AsNoTracking()
                join author in _context.Authors.AsNoTracking() on book.AuthorId equals author.Id into authorJoin
                from author in authorJoin.DefaultIfEmpty()
                join publisher in _context.Publishers.AsNoTracking() on book.PublisherId equals publisher.Id into publisherJoin
                from publisher in publisherJoin.DefaultIfEmpty()
                join category in _context.CommonCategories.AsNoTracking() on book.CategoryId equals category.Id into categoryJoin
                from category in categoryJoin.DefaultIfEmpty()
                select new Book
                {
                    Id = book.Id,
                    Code = book.Code,
                    Title = book.Title,
                    Subtitle = book.Subtitle,
                    Slug = book.Slug,
                    Isbn = book.Isbn,
                    Description = book.Description,
                    CoverImageUrl = book.CoverImageUrl,
                    CategoryId = book.CategoryId,
                    AuthorId = book.AuthorId,
                    PublisherId = book.PublisherId,
                    PriceForSell = book.PriceForSell,
                    PriceForRent = book.PriceForRent,
                    PageCount = book.PageCount,
                    PublicationYear = book.PublicationYear,
                    Language = book.Language,
                    Format = book.Format,
                    Weight = book.Weight,
                    Dimensions = book.Dimensions,
                    TotalQuantity = book.TotalQuantity,
                    StockForSell = book.StockForSell,
                    StockForRent = book.StockForRent,
                    Status = book.Status,
                    IsActive = book.IsActive,
                    IsFeatured = book.IsFeatured,
                    AverageRating = book.AverageRating,
                    ReviewCount = book.ReviewCount,
                    SoldCount = book.SoldCount,
                    IsDeleted = book.IsDeleted,
                    CreatedAt = book.CreatedAt,
                    UpdatedAt = book.UpdatedAt,
                    RowVersion = book.RowVersion,
                    Author = author,
                    Publisher = publisher,
                    Category = category
                };
    }

    private static BookDetailResponse MapDetail(Book book, IReadOnlyCollection<BookCopyListItemResponse> copies, IReadOnlyCollection<ReviewListItemResponse> reviews)
    {
        return new BookDetailResponse
        {
            Id = book.Id,
            Code = book.Code,
            Title = book.Title,
            Subtitle = book.Subtitle,
            Slug = book.Slug,
            Isbn = book.Isbn,
            Description = book.Description,
            CoverImageUrl = book.CoverImageUrl,
            CategoryId = book.CategoryId,
            CategoryName = book.Category?.Name,
            AuthorId = book.AuthorId,
            AuthorName = book.Author?.Name,
            PublisherId = book.PublisherId,
            PublisherName = book.Publisher?.Name,
            PriceForSell = book.PriceForSell,
            PriceForRent = book.PriceForRent,
            PageCount = book.PageCount,
            PublicationYear = book.PublicationYear,
            Language = book.Language,
            Format = book.Format,
            Weight = ServiceHelpers.ParseWeight(book.Weight),
            Dimensions = book.Dimensions,
            TotalQuantity = book.TotalQuantity,
            StockForSell = book.StockForSell,
            StockForRent = book.StockForRent,
            Status = book.Status,
            IsActive = book.IsActive,
            IsFeatured = book.IsFeatured,
            AverageRating = book.AverageRating,
            ReviewCount = book.ReviewCount,
            SoldCount = book.SoldCount,
            IsDeleted = book.IsDeleted,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt,
            RowVersion = book.RowVersion,
            Copies = copies,
            Reviews = reviews
        };
    }

    private async Task<string> BuildUniqueSlugAsync(string title, Guid? excludeId, CancellationToken cancellationToken = default)
    {
        var baseSlug = ServiceHelpers.Slugify(title);
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = "book";
        }

        var slug = baseSlug;
        var suffix = 1;

        while (await _context.Books.AnyAsync(x => x.Slug == slug && x.Id != excludeId, cancellationToken))
        {
            suffix++;
            slug = $"{baseSlug}-{suffix}";
        }

        return slug;
    }

    private async Task WriteAuditAsync(Guid? operatorId, string action, string module, string note)
    {
        await _auditLogService.WriteAsync(new AuditLog
        {
            ActorUserId = operatorId,
            Action = action,
            Module = module,
            Note = note,
            EntityType = "Book",
            IsSuccess = true
        });
    }

    // ------------------------------------------------------------------ Điều chỉnh tồn kho (FE: PUT /books/{id}/stock)

    public async Task<ServiceResult<BookDetailResponse>> UpdateStockAsync(Guid id, UpdateBookStockRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (book is null)
        {
            return ServiceResult<BookDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        if (request.StockForSell < 0 || request.StockForRent < 0)
        {
            return ServiceResult<BookDetailResponse>.Failure(HttpStatusCode.BadRequest, "Số lượng tồn kho không được âm.");
        }

        var deltaSell = request.StockForSell - book.StockForSell;
        var deltaRent = request.StockForRent - book.StockForRent;
        var deltaTotal = deltaSell + deltaRent;

        if (deltaTotal != 0)
        {
            if (deltaTotal > 0)
            {
                var existingCount = await _context.BookCopies.CountAsync(x => x.BookId == book.Id, cancellationToken);

                for (var i = 1; i <= deltaTotal; i++)
                {
                    _context.BookCopies.Add(new BookCopy
                    {
                        Id = Guid.NewGuid(),
                        BookId = book.Id,
                        Barcode = ServiceHelpers.BuildBarcode(book.Id, existingCount + i),
                        ConditionStatus = "Mới",
                        Status = BookCopyStatus.Available,
                        AcquiredDate = DateTime.UtcNow,
                        Notes = request.Reason,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = operatorId.ToString()
                    });
                }
            }
            else
            {
                var removeCount = Math.Abs(deltaTotal);
                var copies = await _context.BookCopies
                    .Where(x => x.BookId == book.Id && !x.IsDeleted && x.Status == BookCopyStatus.Available)
                    .OrderBy(x => x.Barcode)
                    .Take(removeCount)
                    .ToListAsync(cancellationToken);

                foreach (var copy in copies)
                {
                    copy.Status = BookCopyStatus.Lost;
                    copy.Notes = request.Reason;
                    copy.UpdatedAt = DateTime.UtcNow;
                    copy.UpdatedBy = operatorId.ToString();
                }

                if (copies.Count < removeCount)
                {
                    return ServiceResult<BookDetailResponse>.Failure(HttpStatusCode.BadRequest, "Không đủ bản sao khả dụng để giảm tồn kho.");
                }
            }
        }

        book.StockForSell = request.StockForSell;
        book.StockForRent = request.StockForRent;
        book.TotalQuantity = Math.Max(0, book.TotalQuantity + deltaTotal);
        book.UpdatedAt = DateTime.UtcNow;
        book.UpdatedBy = operatorId.ToString();

        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Điều chỉnh tồn kho sách", "Book", $"Cập nhật tồn kho sách {book.Title}: bán {book.StockForSell}, mượn {book.StockForRent}. Lý do: {request.Reason}");

        return await GetDetailAsync(id, cancellationToken);
    }
}
