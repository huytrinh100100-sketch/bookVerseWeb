using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Constants;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Coupon;
using BookVerse.Core.Models.Order;
using BookVerse.Core.Models.Storefront;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class StorefrontService : IStorefrontService
{
    private const decimal FreeShippingThreshold = 500000m;
    private const decimal StandardShippingFee = 30000m;

    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public StorefrontService(AppDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<HomeResponse>> GetHomeAsync(CancellationToken cancellationToken = default)
    {
        var banners = await _context.StoreSettings
            .AsNoTracking()
            .Where(x => x.IsActive && x.GroupKey == "banner")
            .OrderBy(x => x.Key)
            .Select(x => new HomeBannerResponse
            {
                Id = x.Id,
                Title = x.Key,
                Subtitle = x.Description,
                Description = x.Value,
                ImageUrl = null,
                LinkUrl = null,
                BadgeText = null,
                SortOrder = 0,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        var categories = await _context.CommonCategories
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && x.Root!.Code == "book-category")
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new Core.Models.Category.CategoryTreeNodeResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Slug = x.Slug,
                RootId = x.RootId,
                RootCode = x.Root!.Code,
                RootName = x.Root!.Name,
                Type = x.Root!.Code,
                TypeLabel = x.Root!.Name,
                ParentId = x.ParentId,
                SortOrder = x.SortOrder,
                IsMegaMenu = x.IsMegaMenu,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted
            })
            .ToListAsync(cancellationToken);

        var featured = await BuildCatalogQuery()
            .Where(x => x.IsFeatured)
            .OrderByDescending(x => x.SoldCount)
            .Take(8)
            .ToListAsync(cancellationToken);

        var newest = await BuildCatalogQuery()
            .OrderByDescending(x => x.CreatedAt)
            .Take(8)
            .ToListAsync(cancellationToken);

        var bestSellers = await BuildCatalogQuery()
            .OrderByDescending(x => x.SoldCount)
            .ThenByDescending(x => x.AverageRating)
            .Take(8)
            .ToListAsync(cancellationToken);

        var articles = await _context.Articles
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Status == ArticleStatus.Published)
            .OrderByDescending(x => x.PublishedAt)
            .Take(6)
            .Select(x => new Core.Models.Content.ArticleListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Slug = x.Slug,
                Summary = x.Summary,
                CoverImageUrl = x.CoverImageUrl,
                CategoryId = x.CategoryId,
                CategoryName = x.Category!.Name,
                AuthorId = x.AuthorId,
                AuthorName = x.Author!.Name,
                ViewCount = x.ViewCount,
                Status = x.Status,
                PublishedAt = x.PublishedAt,
                IsDeleted = x.IsDeleted,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var coupons = await _context.Coupons
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Status == CouponStatus.Active && x.EndDate >= DateTime.UtcNow)
            .OrderByDescending(x => x.DiscountValue)
            .Take(6)
            .Select(x => new CouponListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                DiscountType = x.DiscountType,
                DiscountValue = x.DiscountValue,
                MinOrderValue = x.MinOrderValue,
                MaxDiscountAmount = x.MaxDiscountAmount,
                UsageLimit = x.UsageLimit,
                UsedCount = x.UsedCount,
                PerUserUsageLimit = x.PerUserUsageLimit,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Audience = x.Audience,
                Status = x.Status,
                IsDeleted = x.IsDeleted,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<HomeResponse>.Success(new HomeResponse
        {
            Banners = banners,
            Categories = categories,
            FeaturedBooks = featured,
            NewBooks = newest,
            BestSellers = bestSellers,
            Articles = articles,
            Coupons = coupons
        }, "Lấy dữ liệu trang chủ thành công.");
    }

    public async Task<ServiceResult<PagedResult<CatalogItemResponse>>> SearchCatalogAsync(CatalogSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 12 : Math.Min(request.PageSize, 60);

        var query = BuildCatalogQuery();

        if (request.CategoryId.HasValue)
        {
            var categoryId = request.CategoryId.Value;
            query = query.Where(x => _context.Books.Any(b => b.Id == x.Id && b.CategoryId == categoryId));
        }

        if (request.AuthorId.HasValue)
        {
            var authorId = request.AuthorId.Value;
            query = query.Where(x => _context.Books.Any(b => b.Id == x.Id && b.AuthorId == authorId));
        }

        if (request.PublisherId.HasValue)
        {
            var publisherId = request.PublisherId.Value;
            query = query.Where(x => _context.Books.Any(b => b.Id == x.Id && b.PublisherId == publisherId));
        }

        if (request.UnitType == UnitType.Sell)
        {
            query = query.Where(x => x.StockForSell > 0);
        }
        else if (request.UnitType == UnitType.Rent)
        {
            query = query.Where(x => x.StockForRent > 0);
        }

        if (request.InStock == true)
        {
            query = query.Where(x => x.StockForSell > 0 || x.StockForRent > 0);
        }

        if (request.MinPrice.HasValue)
        {
            var min = request.MinPrice.Value;
            query = query.Where(x => x.PriceForSell >= min || x.PriceForRent >= min);
        }

        if (request.MaxPrice.HasValue)
        {
            var max = request.MaxPrice.Value;
            query = query.Where(x => x.PriceForSell <= max || x.PriceForRent <= max);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Title.ToLower().Contains(keyword) ||
                (x.AuthorName != null && x.AuthorName.ToLower().Contains(keyword)) ||
                (x.CategoryName != null && x.CategoryName.ToLower().Contains(keyword)) ||
                (x.Isbn != null && x.Isbn.ToLower().Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        query = (request.SortBy?.ToLowerInvariant()) switch
        {
            "price_asc" => query.OrderBy(x => x.PriceForSell),
            "price_desc" => query.OrderByDescending(x => x.PriceForSell),
            "rating" => query.OrderByDescending(x => x.AverageRating),
            "sold" => query.OrderByDescending(x => x.SoldCount),
            "newest" => query.OrderByDescending(x => x.CreatedAt),
            _ => request.SortDescending ? query.OrderByDescending(x => x.SoldCount) : query.OrderBy(x => x.SoldCount)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<CatalogItemResponse>>.Success(new PagedResult<CatalogItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Tìm kiếm sách thành công.");
    }

    public async Task<ServiceResult<CatalogResult>> GetCatalogResultAsync(CatalogSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 12 : Math.Min(request.PageSize, 60);

        var query = BuildCatalogQuery();

        if (request.CategoryId.HasValue)
        {
            var categoryId = request.CategoryId.Value;
            query = query.Where(x => _context.Books.Any(b => b.Id == x.Id && b.CategoryId == categoryId));
        }

        if (request.AuthorId.HasValue)
        {
            var authorId = request.AuthorId.Value;
            query = query.Where(x => _context.Books.Any(b => b.Id == x.Id && b.AuthorId == authorId));
        }

        if (request.PublisherId.HasValue)
        {
            var publisherId = request.PublisherId.Value;
            query = query.Where(x => _context.Books.Any(b => b.Id == x.Id && b.PublisherId == publisherId));
        }

        if (request.UnitType == UnitType.Sell)
        {
            query = query.Where(x => x.StockForSell > 0);
        }
        else if (request.UnitType == UnitType.Rent)
        {
            query = query.Where(x => x.StockForRent > 0);
        }

        if (request.InStock == true)
        {
            query = query.Where(x => x.StockForSell > 0 || x.StockForRent > 0);
        }

        if (request.MinPrice.HasValue)
        {
            var min = request.MinPrice.Value;
            query = query.Where(x => x.PriceForSell >= min || x.PriceForRent >= min);
        }

        if (request.MaxPrice.HasValue)
        {
            var max = request.MaxPrice.Value;
            query = query.Where(x => x.PriceForSell <= max || x.PriceForRent <= max);
        }

        if (request.MinRating.HasValue)
        {
            var minRating = request.MinRating.Value;
            query = query.Where(x => x.AverageRating != null && x.AverageRating >= minRating);
        }

        if (request.Languages is { Count: > 0 })
        {
            var languages = request.Languages.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            if (languages.Count > 0)
            {
                query = query.Where(x => x.Language != null && languages.Contains(x.Language));
            }
        }

        if (request.Formats is { Count: > 0 })
        {
            var formats = request.Formats.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            if (formats.Count > 0)
            {
                query = query.Where(x => x.Format != null && formats.Contains(x.Format));
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Title.ToLower().Contains(keyword) ||
                (x.AuthorName != null && x.AuthorName.ToLower().Contains(keyword)) ||
                (x.CategoryName != null && x.CategoryName.ToLower().Contains(keyword)) ||
                (x.Isbn != null && x.Isbn.ToLower().Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        // FE storefront dung cac gia tri sortBy: curator | price-asc | price-desc | bestseller | newest
        query = (request.SortBy?.ToLowerInvariant()) switch
        {
            "price-asc" or "price_asc" => query.OrderBy(x => x.PriceForSell),
            "price-desc" or "price_desc" => query.OrderByDescending(x => x.PriceForSell),
            "bestseller" or "bestsellers" or "sold" => query.OrderByDescending(x => x.SoldCount),
            "newest" => query.OrderByDescending(x => x.CreatedAt),
            "rating" => query.OrderByDescending(x => x.AverageRating),
            "curator" => query.OrderByDescending(x => x.IsFeatured).ThenByDescending(x => x.SoldCount).ThenByDescending(x => x.AverageRating),
            _ => query.OrderByDescending(x => x.SoldCount).ThenByDescending(x => x.AverageRating)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var facets = await BuildCatalogFacetsAsync(request, cancellationToken);

        return ServiceResult<CatalogResult>.Success(new CatalogResult
        {
            Items = items,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize,
            Facets = facets
        }, "Tìm kiếm sách thành công.");
    }

    private async Task<CatalogFacets> BuildCatalogFacetsAsync(CatalogSearchRequest request, CancellationToken cancellationToken)
    {
        var baseQuery = BuildPublishedBookQuery();

        if (request.CategoryId.HasValue)
        {
            var categoryId = request.CategoryId.Value;
            baseQuery = baseQuery.Where(x => x.CategoryId == categoryId);
        }

        if (request.AuthorId.HasValue)
        {
            var authorId = request.AuthorId.Value;
            baseQuery = baseQuery.Where(x => x.AuthorId == authorId);
        }

        if (request.PublisherId.HasValue)
        {
            var publisherId = request.PublisherId.Value;
            baseQuery = baseQuery.Where(x => x.PublisherId == publisherId);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(x =>
                x.Title.ToLower().Contains(keyword) ||
                (x.Author != null && x.Author.Name.ToLower().Contains(keyword)) ||
                (x.Category != null && x.Category.Name.ToLower().Contains(keyword)) ||
                (x.Isbn != null && x.Isbn.ToLower().Contains(keyword)));
        }

        var categoryFacets = await baseQuery
            .Where(x => x.CategoryId != null && x.Category != null)
            .GroupBy(x => new { x.CategoryId, Name = x.Category!.Name })
            .Select(g => new CatalogFacetResponse
            {
                CategoryId = g.Key.CategoryId!.Value,
                CategoryName = g.Key.Name,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.CategoryName)
            .Take(30)
            .ToListAsync(cancellationToken);

        var publisherFacets = await baseQuery
            .Where(x => x.PublisherId != null && x.Publisher != null)
            .GroupBy(x => new { x.PublisherId, Name = x.Publisher!.Name })
            .Select(g => new CatalogFacetResponse
            {
                CategoryId = g.Key.PublisherId!.Value,
                CategoryName = g.Key.Name,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.CategoryName)
            .Take(30)
            .ToListAsync(cancellationToken);

        // Luu y: CategoryId = Guid.Empty la CONSTANT, EF Core se sinh SQL la chuoi literal
        // ('00000000-...') va doc bang GetGuid() se nem InvalidCastException.
        // Vi vay phai chi lay cot that tu GROUP BY o query, roi moi gan CategoryId o memory.
        var languageFacets = (await baseQuery
            .Where(x => x.Language != null)
            .GroupBy(x => x.Language!)
            .Select(g => new { Language = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Language)
            .Take(30)
            .ToListAsync(cancellationToken))
            .Select(x => new CatalogFacetResponse
            {
                CategoryId = Guid.Empty,
                CategoryName = x.Language,
                Count = x.Count
            })
            .ToList();

        var formatFacets = (await baseQuery
            .Where(x => x.Format != null)
            .GroupBy(x => x.Format!)
            .Select(g => new { Format = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Format)
            .Take(30)
            .ToListAsync(cancellationToken))
            .Select(x => new CatalogFacetResponse
            {
                CategoryId = Guid.Empty,
                CategoryName = x.Format,
                Count = x.Count
            })
            .ToList();

        var prices = await baseQuery.Select(x => x.PriceForSell).ToListAsync(cancellationToken);

        var buckets = new (string Label, decimal Min, decimal Max)[]
        {
            ("Dưới 100K", 0m, 100000m),
            ("100K - 200K", 100000m, 200000m),
            ("200K - 300K", 200000m, 300000m),
            ("300K - 500K", 300000m, 500000m),
            ("Trên 500K", 500000m, 0m)
        };

        var priceRanges = buckets
            .Select(b => new PriceRangeFacetResponse
            {
                Label = b.Label,
                Min = b.Min,
                Max = b.Max,
                Count = b.Max == 0m
                    ? prices.Count(p => p >= b.Min)
                    : prices.Count(p => p >= b.Min && p < b.Max)
            })
            .ToList();

        return new CatalogFacets
        {
            Categories = categoryFacets,
            Publishers = publisherFacets,
            Languages = languageFacets,
            Formats = formatFacets,
            PriceRanges = priceRanges
        };
    }

    public async Task<ServiceResult<CatalogDetailResponse>> GetCatalogDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (book is null)
        {
            return ServiceResult<CatalogDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        return await BuildCatalogDetailAsync(book.Id, cancellationToken);
    }

    public async Task<ServiceResult<CatalogDetailResponse>> GetCatalogDetailBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug && !x.IsDeleted, cancellationToken);
        if (book is null)
        {
            return ServiceResult<CatalogDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        return await BuildCatalogDetailAsync(book.Id, cancellationToken);
    }

    public async Task<ServiceResult<CartResponse>> GetCartAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var items = await BuildCartItemsAsync(customerId, cancellationToken);
        return await BuildCartResponseAsync(items, null, cancellationToken);
    }

    public async Task<ServiceResult<CartResponse>> AddToCartAsync(Guid customerId, CartItemRequest request, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.BookId && !x.IsDeleted, cancellationToken);
        if (book is null)
        {
            return ServiceResult<CartResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        var available = request.UnitType == UnitType.Rent ? book.StockForRent : book.StockForSell;
        if (available < request.Quantity)
        {
            return ServiceResult<CartResponse>.Failure(HttpStatusCode.BadRequest, $"Sách không đủ tồn kho (còn {available}).");
        }

        var unitPrice = request.UnitType == UnitType.Rent ? book.PriceForRent : book.PriceForSell;

        var existing = await _context.CartItems
            .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.BookId == request.BookId && x.UnitType == request.UnitType, cancellationToken);

        if (existing is null)
        {
            _context.CartItems.Add(new CartItem
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                BookId = request.BookId,
                UnitType = request.UnitType,
                Quantity = request.Quantity,
                UnitPrice = unitPrice,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.Quantity += request.Quantity;
            existing.UnitPrice = unitPrice;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var items = await BuildCartItemsAsync(customerId, cancellationToken);
        return await BuildCartResponseAsync(items, null, cancellationToken);
    }

    public async Task<ServiceResult<CartResponse>> UpdateCartItemAsync(Guid customerId, Guid bookId, UnitType unitType, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            return await RemoveFromCartAsync(customerId, bookId, unitType, cancellationToken);
        }

        var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(x => x.Id == bookId, cancellationToken);
        if (book is null)
        {
            return ServiceResult<CartResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        var available = unitType == UnitType.Rent ? book.StockForRent : book.StockForSell;
        if (available < quantity)
        {
            return ServiceResult<CartResponse>.Failure(HttpStatusCode.BadRequest, $"Sách không đủ tồn kho (còn {available}).");
        }

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.BookId == bookId && x.UnitType == unitType, cancellationToken);

        if (cartItem is null)
        {
            return ServiceResult<CartResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sản phẩm trong giỏ hàng.");
        }

        cartItem.Quantity = quantity;
        cartItem.UnitPrice = unitType == UnitType.Rent ? book.PriceForRent : book.PriceForSell;
        cartItem.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var items = await BuildCartItemsAsync(customerId, cancellationToken);
        return await BuildCartResponseAsync(items, null, cancellationToken);
    }

    public async Task<ServiceResult<CartResponse>> RemoveFromCartAsync(Guid customerId, Guid bookId, UnitType unitType, CancellationToken cancellationToken = default)
    {
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.BookId == bookId && x.UnitType == unitType, cancellationToken);

        if (existing is not null)
        {
            _context.CartItems.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var items = await BuildCartItemsAsync(customerId, cancellationToken);
        return await BuildCartResponseAsync(items, null, cancellationToken);
    }

    public async Task<ServiceResult<CartResponse>> UpdateCartItemByIdAsync(Guid customerId, Guid itemId, int quantity, CancellationToken cancellationToken = default)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(x => x.Id == itemId && x.CustomerId == customerId, cancellationToken);

        if (cartItem is null)
        {
            return ServiceResult<CartResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy dòng giỏ hàng.");
        }

        if (quantity <= 0)
        {
            return await RemoveCartItemByIdAsync(customerId, itemId, cancellationToken);
        }

        var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(x => x.Id == cartItem.BookId, cancellationToken);
        if (book is null)
        {
            return ServiceResult<CartResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        var available = cartItem.UnitType == UnitType.Rent ? book.StockForRent : book.StockForSell;
        if (available < quantity)
        {
            return ServiceResult<CartResponse>.Failure(HttpStatusCode.BadRequest, $"Sách không đủ tồn kho (còn {available}).");
        }

        cartItem.Quantity = quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var items = await BuildCartItemsAsync(customerId, cancellationToken);
        return await BuildCartResponseAsync(items, null, cancellationToken);
    }

    public async Task<ServiceResult<CartResponse>> RemoveCartItemByIdAsync(Guid customerId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(x => x.Id == itemId && x.CustomerId == customerId, cancellationToken);

        if (cartItem is null)
        {
            return ServiceResult<CartResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy dòng giỏ hàng.");
        }

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync(cancellationToken);

        var items = await BuildCartItemsAsync(customerId, cancellationToken);
        return await BuildCartResponseAsync(items, null, cancellationToken);
    }

    public async Task<ServiceResult<CartResponse>> ApplyCouponAsync(Guid customerId, string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return ServiceResult<CartResponse>.Failure(HttpStatusCode.BadRequest, "Mã voucher không được để trống.");
        }

        var normalized = code.Trim().ToUpperInvariant();
        var coupon = await _context.Coupons.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == normalized && !x.IsDeleted, cancellationToken);

        if (coupon is null)
        {
            return ServiceResult<CartResponse>.Failure(HttpStatusCode.NotFound, "Voucher không tồn tại.");
        }

        var now = DateTime.UtcNow;
        if (coupon.Status != CouponStatus.Active || coupon.StartDate > now || coupon.EndDate < now || coupon.UsedCount >= coupon.UsageLimit)
        {
            return ServiceResult<CartResponse>.Failure(HttpStatusCode.BadRequest, "Voucher không còn hiệu lực.");
        }

        var items = await BuildCartItemsAsync(customerId, cancellationToken);
        if (items.Count == 0)
        {
            return ServiceResult<CartResponse>.Failure(HttpStatusCode.BadRequest, "Giỏ hàng đang trống.");
        }

        var subTotal = ServiceHelpers.RoundMoney(items.Sum(x => x.LineTotal));
        if (subTotal < coupon.MinOrderValue)
        {
            return ServiceResult<CartResponse>.Failure(
                HttpStatusCode.BadRequest,
                $"Đơn hàng tối thiểu {coupon.MinOrderValue:N0} đ để dùng voucher này.");
        }

        var result = await BuildCartResponseAsync(items, coupon.Code, cancellationToken);
        if (!result.IsSuccess)
        {
            return result;
        }

        // Lưu mã voucher đang áp dụng vào từng dòng giỏ để checkout dùng lại.
        var cartRows = await _context.CartItems.Where(x => x.CustomerId == customerId).ToListAsync(cancellationToken);
        foreach (var row in cartRows)
        {
            row.UpdatedAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<ServiceResult<CheckoutResponse>> CheckoutAsync(Guid customerId, CheckoutRequest request, CancellationToken cancellationToken = default)
    {
        var cartItems = await BuildCartItemsAsync(customerId, cancellationToken);
        if (cartItems.Count == 0)
        {
            return ServiceResult<CheckoutResponse>.Failure(HttpStatusCode.BadRequest, "Giỏ hàng đang trống.");
        }

        var subTotal = ServiceHelpers.RoundMoney(cartItems.Sum(x => x.LineTotal));

        var discountAmount = 0m;
        Guid? couponId = null;
        string? couponCode = null;

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(x => x.Code == request.CouponCode.Trim().ToUpperInvariant() && !x.IsDeleted, cancellationToken);
            if (coupon is not null && coupon.Status == CouponStatus.Active && coupon.StartDate <= DateTime.UtcNow && coupon.EndDate >= DateTime.UtcNow && coupon.UsedCount < coupon.UsageLimit)
            {
                discountAmount = coupon.DiscountType == DiscountType.Percent
                    ? ServiceHelpers.RoundMoney(subTotal * coupon.DiscountValue / 100m)
                    : ServiceHelpers.RoundMoney(coupon.DiscountValue);

                if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                {
                    discountAmount = coupon.MaxDiscountAmount.Value;
                }

                discountAmount = Math.Min(discountAmount, subTotal);
                couponId = coupon.Id;
                couponCode = coupon.Code;
                coupon.UsedCount++;
            }
        }

        var shippingAmount = subTotal - discountAmount >= FreeShippingThreshold ? 0m : StandardShippingFee;
        var finalAmount = ServiceHelpers.RoundMoney(subTotal - discountAmount + shippingAmount);

        var now = DateTime.UtcNow;
        var sequence = await _context.Orders.CountAsync(cancellationToken) + 1;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Code = ServiceHelpers.BuildOrderCode(now, sequence),
            CustomerId = customerId,
            CouponId = couponId,
            CouponCode = couponCode,
            OrderDate = now,
            OrderStatus = request.PaymentMethod == PaymentMethod.COD ? OrderStatus.Pending : OrderStatus.Paid,
            PaymentMethod = request.PaymentMethod,
            IsPaid = request.PaymentMethod != PaymentMethod.COD,
            PaidAt = request.PaymentMethod != PaymentMethod.COD ? now : null,
            ShippingAddress = request.ShippingAddress,
            ProvinceId = request.ProvinceId,
            CommuneId = request.CommuneId,
            CustomerNote = request.Note,
            TotalAmount = subTotal,
            DiscountAmount = discountAmount,
            ShippingAmount = shippingAmount,
            FinalAmount = finalAmount,
            CreatedAt = now,
            CreatedBy = customerId.ToString()
        };

        foreach (var item in cartItems)
        {
            var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == item.BookId, cancellationToken);
            if (book is null)
            {
                continue;
            }

            _context.OrderDetails.Add(new OrderDetail
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                BookId = book.Id,
                UnitType = item.UnitType,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountAmount = 0,
                LineTotal = item.LineTotal,
                BookTitle = book.Title,
                IsbnSnapshot = book.Isbn,
                AuthorName = book.Author?.Name,
                PublisherName = book.Publisher?.Name,
                CoverImageUrl = book.CoverImageUrl,
                CreatedAt = now,
                CreatedBy = customerId.ToString()
            });

            if (item.UnitType == UnitType.Rent)
            {
                book.StockForRent -= item.Quantity;
            }
            else
            {
                book.StockForSell -= item.Quantity;
            }

            book.SoldCount += item.Quantity;
            book.UpdatedAt = now;
            book.UpdatedBy = customerId.ToString();
        }

        _context.Orders.Add(order);

        // Xoá giỏ hàng
        var cartEntities = await _context.CartItems.Where(x => x.CustomerId == customerId).ToListAsync(cancellationToken);
        _context.CartItems.RemoveRange(cartEntities);

        // Lưu địa chỉ nếu được yêu cầu
        if (request.SaveAddress && !string.IsNullOrWhiteSpace(request.ShippingAddress))
        {
            _context.CustomerAddresses.Add(new CustomerAddress
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                RecipientName = request.RecipientName,
                PhoneNumber = request.PhoneNumber,
                Address = request.ShippingAddress,
                ProvinceId = request.ProvinceId ?? Guid.Empty,
                CommuneId = request.CommuneId ?? Guid.Empty,
                IsDefault = false,
                CreatedAt = now,
                CreatedBy = customerId.ToString()
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            FromStatus = order.OrderStatus,
            ToStatus = order.OrderStatus,
            Note = "Đặt hàng từ storefront.",
            ChangedBy = customerId,
            ChangedAt = now
        });
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.WriteAsync(new AuditLog
        {
            ActorUserId = customerId,
            Action = "Đặt hàng",
            Module = "Storefront",
            Note = $"Đặt hàng {order.Code} - {order.FinalAmount:N0} đ.",
            EntityType = "Order",
            IsSuccess = true
        });

        return ServiceResult<CheckoutResponse>.Success(new CheckoutResponse
        {
            OrderId = order.Id,
            OrderCode = order.Code,
            TotalAmount = subTotal,
            ShippingAmount = shippingAmount,
            DiscountAmount = discountAmount,
            FinalAmount = finalAmount,
            PaymentMethod = request.PaymentMethod,
            Message = "Đặt hàng thành công."
        }, "Đặt hàng thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<CheckoutResponse>>> GetMyOrdersAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId && !x.IsDeleted)
            .OrderByDescending(x => x.OrderDate)
            .ToListAsync(cancellationToken);

        if (orders.Count == 0)
        {
            return ServiceResult<IReadOnlyCollection<CheckoutResponse>>.Success([], "Bạn chưa có đơn hàng nào.");
        }

        var orderIds = orders.Select(x => x.Id).ToList();

        var details = await _context.OrderDetails
            .AsNoTracking()
            .Where(x => orderIds.Contains(x.OrderId) && !x.IsDeleted)
            .Select(x => new { x.OrderId, x.BookTitle })
            .ToListAsync(cancellationToken);

        var detailsByOrder = details
            .GroupBy(x => x.OrderId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.BookTitle ?? string.Empty).ToList());

        var result = orders.Select(order =>
        {
            var titles = detailsByOrder.TryGetValue(order.Id, out var list) ? list : new List<string>();
            return new CheckoutResponse
            {
                Id = order.Id,
                OrderId = order.Id,
                Code = order.Code,
                OrderCode = order.Code,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                FinalAmount = order.FinalAmount,
                ShippingAmount = order.ShippingAmount,
                DiscountAmount = order.DiscountAmount,
                PaymentMethod = order.PaymentMethod,
                OrderStatus = order.OrderStatus,
                ItemCount = titles.Count,
                PreviewTitles = titles.Take(3).ToList(),
                TrackingCode = order.TrackingCode
            };
        }).ToList();

        return ServiceResult<IReadOnlyCollection<CheckoutResponse>>.Success(result, "Lấy danh sách đơn hàng của bạn thành công.");
    }

    public async Task<ServiceResult<ShopCustomerProfileResponse>> GetMyProfileAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == customerId && !x.IsDeleted, cancellationToken);
        if (customer is null)
        {
            return ServiceResult<ShopCustomerProfileResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy khách hàng.");
        }

        var wishlistCount = await _context.WishlistItems.CountAsync(x => x.CustomerId == customerId, cancellationToken);
        var addressCount = await _context.CustomerAddresses.CountAsync(x => x.CustomerId == customerId && !x.IsDeleted, cancellationToken);

        return ServiceResult<ShopCustomerProfileResponse>.Success(new ShopCustomerProfileResponse
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            AvatarUrl = customer.AvatarUrl,
            VipTier = customer.VipTier,
            RewardPoints = customer.RewardPoints,
            WishlistBookCount = wishlistCount,
            AddressCount = addressCount
        }, "Lấy hồ sơ khách hàng thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<ShopAddressResponse>>> GetShopAddressesAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var items = await _context.CustomerAddresses
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId && !x.IsDeleted)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new ShopAddressResponse
            {
                Id = x.Id,
                RecipientName = x.RecipientName,
                PhoneNumber = x.PhoneNumber,
                Address = x.Address,
                ProvinceId = x.ProvinceId,
                ProvinceName = x.Province != null ? x.Province.Name : string.Empty,
                CommuneId = x.CommuneId,
                CommuneName = x.Commune != null ? x.Commune.Name : string.Empty,
                IsDefault = x.IsDefault,
                Note = x.Note
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<ShopAddressResponse>>.Success(items, "Lấy danh sách địa chỉ thành công.");
    }

    public async Task<ServiceResult<ShopAddressResponse>> CreateShopAddressAsync(Guid customerId, CreateShopAddressRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RecipientName) || string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Address))
        {
            return ServiceResult<ShopAddressResponse>.Failure(HttpStatusCode.BadRequest, "Vui lòng nhập đầy đủ người nhận, số điện thoại và địa chỉ.");
        }

        if (request.IsDefault)
        {
            var currentDefaults = await _context.CustomerAddresses
                .Where(x => x.CustomerId == customerId && x.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var item in currentDefaults)
            {
                item.IsDefault = false;
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        var address = new CustomerAddress
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            RecipientName = request.RecipientName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Address = request.Address.Trim(),
            ProvinceId = request.ProvinceId,
            CommuneId = request.CommuneId,
            IsDefault = request.IsDefault,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = customerId.ToString()
        };

        _context.CustomerAddresses.Add(address);
        await _context.SaveChangesAsync(cancellationToken);

        var provinceName = await _context.Provinces
            .AsNoTracking()
            .Where(x => x.Id == address.ProvinceId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var communeName = await _context.Communes
            .AsNoTracking()
            .Where(x => x.Id == address.CommuneId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return ServiceResult<ShopAddressResponse>.Success(new ShopAddressResponse
        {
            Id = address.Id,
            RecipientName = address.RecipientName,
            PhoneNumber = address.PhoneNumber,
            Address = address.Address,
            ProvinceId = address.ProvinceId,
            ProvinceName = provinceName ?? string.Empty,
            CommuneId = address.CommuneId,
            CommuneName = communeName ?? string.Empty,
            IsDefault = address.IsDefault,
            Note = address.Note
        }, "Thêm địa chỉ thành công.");
    }

    public async Task<ServiceResult<ShopAddressResponse>> UpdateShopAddressAsync(Guid customerId, Guid addressId, CreateShopAddressRequest request, CancellationToken cancellationToken = default)
    {
        var address = await _context.CustomerAddresses
            .FirstOrDefaultAsync(x => x.Id == addressId && x.CustomerId == customerId && !x.IsDeleted, cancellationToken);

        if (address is null)
        {
            return ServiceResult<ShopAddressResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy địa chỉ cần cập nhật.");
        }

        if (string.IsNullOrWhiteSpace(request.RecipientName) || string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Address))
        {
            return ServiceResult<ShopAddressResponse>.Failure(HttpStatusCode.BadRequest, "Vui lòng nhập đầy đủ người nhận, số điện thoại và địa chỉ.");
        }

        if (request.IsDefault)
        {
            var currentDefaults = await _context.CustomerAddresses
                .Where(x => x.CustomerId == customerId && x.IsDefault && x.Id != addressId)
                .ToListAsync(cancellationToken);

            foreach (var item in currentDefaults)
            {
                item.IsDefault = false;
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        address.RecipientName = request.RecipientName.Trim();
        address.PhoneNumber = request.PhoneNumber.Trim();
        address.Address = request.Address.Trim();
        address.ProvinceId = request.ProvinceId;
        address.CommuneId = request.CommuneId;
        address.IsDefault = request.IsDefault;
        address.Note = request.Note;
        address.UpdatedAt = DateTime.UtcNow;
        address.UpdatedBy = customerId.ToString();

        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<ShopAddressResponse>.Success(await BuildShopAddressAsync(address, cancellationToken), "Cập nhật địa chỉ thành công.");
    }

    public async Task<ServiceResult<bool>> DeleteShopAddressAsync(Guid customerId, Guid addressId, CancellationToken cancellationToken = default)
    {
        var address = await _context.CustomerAddresses
            .FirstOrDefaultAsync(x => x.Id == addressId && x.CustomerId == customerId && !x.IsDeleted, cancellationToken);

        if (address is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy địa chỉ cần xóa.");
        }

        var isDefault = address.IsDefault;

        address.IsDeleted = true;
        address.DeletedAt = DateTime.UtcNow;
        address.DeletedBy = customerId.ToString();

        await _context.SaveChangesAsync(cancellationToken);

        // Nếu xóa địa chỉ mặc định thì chuyển địa chỉ còn lại gần nhất làm mặc định.
        if (isDefault)
        {
            var next = await _context.CustomerAddresses
                .Where(x => x.CustomerId == customerId && !x.IsDeleted)
                .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (next is not null)
            {
                next.IsDefault = true;
                next.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        return ServiceResult<bool>.Success(true, "Xóa địa chỉ thành công.");
    }

    private async Task<ShopAddressResponse> BuildShopAddressAsync(CustomerAddress address, CancellationToken cancellationToken)
    {
        var provinceName = await _context.Provinces
            .AsNoTracking()
            .Where(x => x.Id == address.ProvinceId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        var communeName = await _context.Communes
            .AsNoTracking()
            .Where(x => x.Id == address.CommuneId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return new ShopAddressResponse
        {
            Id = address.Id,
            RecipientName = address.RecipientName,
            PhoneNumber = address.PhoneNumber,
            Address = address.Address,
            ProvinceId = address.ProvinceId,
            ProvinceName = provinceName ?? string.Empty,
            CommuneId = address.CommuneId,
            CommuneName = communeName ?? string.Empty,
            IsDefault = address.IsDefault,
            Note = address.Note
        };
    }

    public async Task<ServiceResult<ShopCustomerProfileResponse>> UpdateMyProfileAsync(Guid customerId, UpdateShopProfileRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _context.Users.FirstOrDefaultAsync(x => x.Id == customerId && !x.IsDeleted, cancellationToken);

        if (customer is null)
        {
            return ServiceResult<ShopCustomerProfileResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy khách hàng.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return ServiceResult<ShopCustomerProfileResponse>.Failure(HttpStatusCode.BadRequest, "Họ tên không được để trống.");
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailTaken = await _context.Users
                .AnyAsync(x => x.Id != customerId && x.Email == request.Email.Trim() && !x.IsDeleted, cancellationToken);

            if (emailTaken)
            {
                return ServiceResult<ShopCustomerProfileResponse>.Failure(HttpStatusCode.Conflict, "Email này đã được sử dụng.");
            }

            customer.Email = request.Email.Trim();
        }

        customer.FullName = request.FullName.Trim();
        customer.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? customer.PhoneNumber : request.PhoneNumber.Trim();
        customer.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? customer.AvatarUrl : request.AvatarUrl.Trim();
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var wishlistCount = await _context.WishlistItems.CountAsync(x => x.CustomerId == customerId, cancellationToken);
        var addressCount = await _context.CustomerAddresses.CountAsync(x => x.CustomerId == customerId && !x.IsDeleted, cancellationToken);

        return ServiceResult<ShopCustomerProfileResponse>.Success(new ShopCustomerProfileResponse
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            AvatarUrl = customer.AvatarUrl,
            VipTier = customer.VipTier,
            RewardPoints = customer.RewardPoints,
            WishlistBookCount = wishlistCount,
            AddressCount = addressCount
        }, "Cập nhật hồ sơ thành công.");
    }

    public async Task<ServiceResult<bool>> CancelMyOrderAsync(Guid customerId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(x => x.Id == orderId && x.CustomerId == customerId && !x.IsDeleted, cancellationToken);

        if (order is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đơn hàng.");
        }

        if (order.OrderStatus is OrderStatus.Completed or OrderStatus.Cancelled or OrderStatus.Refunded)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Đơn hàng này không thể hủy.");
        }

        if (order.IsPaid)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Đơn hàng đã thanh toán, vui lòng liên hệ nhân viên hỗ trợ.");
        }

        var now = DateTime.UtcNow;
        var previousStatus = order.OrderStatus;

        order.OrderStatus = OrderStatus.Cancelled;
        order.CancelledAt = now;
        order.CancelledBy = customerId;
        order.CancelReason = "Khách hàng hủy đơn.";
        order.UpdatedAt = now;
        order.UpdatedBy = customerId.ToString();

        // Trả lại tồn kho sách đã trừ khi đặt hàng (checkout trừ ở Book.StockForSell/StockForRent).
        var details = await _context.OrderDetails
            .Where(x => x.OrderId == order.Id && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var detail in details)
        {
            var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == detail.BookId, cancellationToken);
            if (book is null)
            {
                continue;
            }

            if (detail.UnitType == UnitType.Rent)
            {
                book.StockForRent += detail.Quantity;
            }
            else
            {
                book.StockForSell += detail.Quantity;
            }

            book.SoldCount = Math.Max(0, book.SoldCount - detail.Quantity);
            book.UpdatedAt = now;
        }

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            FromStatus = previousStatus,
            ToStatus = OrderStatus.Cancelled,
            Note = "Khách hàng hủy đơn.",
            ChangedBy = customerId,
            ChangedAt = now
        });

        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true, "Hủy đơn hàng thành công.");
    }

    public async Task<ServiceResult<ShopOrderDetailResponse>> GetMyOrderDetailAsync(Guid customerId, Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == orderId && x.CustomerId == customerId && !x.IsDeleted, cancellationToken);

        if (order is null)
        {
            return ServiceResult<ShopOrderDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đơn hàng.");
        }

        var details = await _context.OrderDetails
            .AsNoTracking()
            .Where(x => x.OrderId == order.Id && !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new ShopOrderLineResponse
            {
                BookId = x.BookId,
                Title = x.BookTitle ?? string.Empty,
                Slug = x.Book != null ? x.Book.Slug : null,
                CoverImageUrl = x.CoverImageUrl,
                UnitType = x.UnitType,
                UnitPrice = x.UnitPrice,
                Quantity = x.Quantity,
                LineTotal = x.LineTotal
            })
            .ToListAsync(cancellationToken);

        var timeline = await _context.OrderStatusHistories
            .AsNoTracking()
            .Where(x => x.OrderId == order.Id)
            .OrderBy(x => x.ChangedAt)
            .Select(x => new OrderStatusHistoryResponse
            {
                Id = x.Id,
                FromStatus = x.FromStatus,
                ToStatus = x.ToStatus,
                Note = x.Note,
                ChangedAt = x.ChangedAt,
                ChangedBy = x.ChangedBy
            })
            .ToListAsync(cancellationToken);

        var recipientName = await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == customerId)
            .Select(x => x.FullName)
            .FirstOrDefaultAsync(cancellationToken);

        var phoneNumber = await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == customerId)
            .Select(x => x.PhoneNumber)
            .FirstOrDefaultAsync(cancellationToken);

        return ServiceResult<ShopOrderDetailResponse>.Success(new ShopOrderDetailResponse
        {
            Id = order.Id,
            Code = order.Code,
            OrderCode = order.Code,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            FinalAmount = order.FinalAmount,
            ShippingAmount = order.ShippingAmount,
            DiscountAmount = order.DiscountAmount,
            PaymentMethod = order.PaymentMethod,
            OrderStatus = order.OrderStatus,
            ItemCount = details.Sum(x => x.Quantity),
            PreviewTitles = details.Select(x => x.Title).ToList(),
            TrackingCode = order.TrackingCode,
            PaymentUrl = null,
            Message = string.Empty,
            RecipientName = recipientName,
            PhoneNumber = phoneNumber,
            ShippingAddress = order.ShippingAddress,
            CustomerNote = order.CustomerNote,
            Items = details,
            Timeline = timeline
        }, "Lấy chi tiết đơn hàng thành công.");
    }

    public async Task<ServiceResult<bool>> SubmitShopReviewAsync(Guid customerId, ShopReviewPayload request, CancellationToken cancellationToken = default)
    {        var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.BookId && !x.IsDeleted, cancellationToken);
        if (book is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        if (request.Rating is < 1 or > 5)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Điểm đánh giá phải từ 1 đến 5.");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Nội dung đánh giá không được để trống.");
        }

        var existing = await _context.Reviews
            .FirstOrDefaultAsync(x => x.BookId == request.BookId && x.CustomerId == customerId && !x.IsDeleted, cancellationToken);

        if (existing is not null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.Conflict, "Bạn đã đánh giá cuốn sách này rồi.");
        }

        var now = DateTime.UtcNow;

        var review = new Review
        {
            Id = Guid.NewGuid(),
            BookId = request.BookId,
            CustomerId = customerId,
            Rating = request.Rating,
            Title = request.Title,
            Content = request.Content.Trim(),
            MediaUrls = ServiceHelpers.JoinCsv(request.MediaUrls),
            Status = ReviewStatus.Pending,
            Sentiment = request.Rating >= 4 ? Sentiment.Positive : request.Rating == 3 ? Sentiment.Neutral : Sentiment.Negative,
            CreatedAt = now,
            CreatedBy = customerId.ToString()
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true, "Gửi đánh giá thành công. Đánh giá đang chờ duyệt.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<ShopCouponResponse>>> GetActiveCouponsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var coupons = await _context.Coupons
            .AsNoTracking()
            .Where(x => !x.IsDeleted
                        && x.Status == CouponStatus.Active
                        && x.StartDate <= now
                        && x.EndDate >= now
                        && (x.UsageLimit == 0 || x.UsedCount < x.UsageLimit))
            .OrderByDescending(x => x.DiscountValue)
            .Take(20)
            .Select(x => new ShopCouponResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                DiscountType = x.DiscountType.ToString(),
                DiscountValue = x.DiscountValue,
                MinOrderValue = x.MinOrderValue,
                MaxDiscountAmount = x.MaxDiscountAmount,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                UsageLimit = x.UsageLimit,
                UsedCount = x.UsedCount,
                Audience = x.Audience.ToString()
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<ShopCouponResponse>>.Success(coupons, "Lấy danh sách voucher thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<WishlistItemResponse>>> GetWishlistAsync(Guid customerId, CancellationToken cancellationToken = default)    {
        var items = await _context.WishlistItems
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new WishlistItemResponse
            {
                Id = x.Id,
                BookId = x.BookId,
                BookTitle = x.Book.Title,
                BookSlug = x.Book.Slug,
                CoverImageUrl = x.Book.CoverImageUrl,
                PriceForSell = x.Book.PriceForSell,
                PriceForRent = x.Book.PriceForRent,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<WishlistItemResponse>>.Success(items, "Lấy danh sách yêu thích thành công.");
    }

    public async Task<ServiceResult<bool>> ToggleWishlistAsync(Guid customerId, Guid bookId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.WishlistItems.FirstOrDefaultAsync(x => x.CustomerId == customerId && x.BookId == bookId, cancellationToken);
        if (existing is not null)
        {
            _context.WishlistItems.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
            return ServiceResult<bool>.Success(false, "Đã xoá khỏi danh sách yêu thích.");
        }

        var book = await _context.Books.AsNoTracking().AnyAsync(x => x.Id == bookId && !x.IsDeleted, cancellationToken);
        if (!book)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        _context.WishlistItems.Add(new WishlistItem
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            BookId = bookId,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<bool>.Success(true, "Đã thêm vào danh sách yêu thích.");
    }

    public async Task<ServiceResult<TrackingResponse>> TrackAsync(TrackingRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim();
        var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code && !x.IsDeleted, cancellationToken);
        if (order is null)
        {
            return ServiceResult<TrackingResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đơn hàng với mã này.");
        }

        var timeline = await _context.OrderStatusHistories
            .AsNoTracking()
            .Where(x => x.OrderId == order.Id)
            .OrderBy(x => x.ChangedAt)
            .Select(x => new OrderStatusHistoryResponse
            {
                Id = x.Id,
                FromStatus = x.FromStatus,
                ToStatus = x.ToStatus,
                Note = x.Note,
                ChangedBy = x.ChangedBy,
                ChangedByName = x.ChangedBy.HasValue ? _context.Users.Where(u => u.Id == x.ChangedBy.Value).Select(u => u.FullName).FirstOrDefault() : null,
                ChangedAt = x.ChangedAt
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<TrackingResponse>.Success(new TrackingResponse
        {
            OrderCode = order.Code,
            Status = order.OrderStatus.ToString(),
            TrackingCode = order.TrackingCode,
            OrderDate = order.OrderDate,
            FinalAmount = order.FinalAmount,
            Timeline = timeline
        }, "Tra cứu đơn hàng thành công.");
    }

    // ------------------------------------------------------------------ Helpers

    private IQueryable<CatalogItemResponse> BuildCatalogQuery()
    {
        var now = DateTime.UtcNow;

        return from book in _context.Books.AsNoTracking()
               join author in _context.Authors.AsNoTracking() on book.AuthorId equals author.Id into authorJoin
               from author in authorJoin.DefaultIfEmpty()
               join publisher in _context.Publishers.AsNoTracking() on book.PublisherId equals publisher.Id into publisherJoin
               from publisher in publisherJoin.DefaultIfEmpty()
               join category in _context.CommonCategories.AsNoTracking() on book.CategoryId equals category.Id into categoryJoin
               from category in categoryJoin.DefaultIfEmpty()
               where !book.IsDeleted && book.IsActive && book.Status == BookStatus.Published
               select new CatalogItemResponse
               {
                   Id = book.Id,
                   Code = book.Code,
                   Title = book.Title,
                   Subtitle = book.Subtitle,
                   Slug = book.Slug,
                   Isbn = book.Isbn,
                   CategoryId = book.CategoryId,
                   CategoryName = category != null ? category.Name : null,
                   AuthorId = book.AuthorId,
                   AuthorName = author != null ? author.Name : null,
                   PublisherId = book.PublisherId,
                   PublisherName = publisher != null ? publisher.Name : null,
                   CoverImageUrl = book.CoverImageUrl,
                   PriceForSell = book.PriceForSell,
                   PriceForRent = book.PriceForRent,
                   OriginalPrice = book.PriceForSell,
                   DiscountPercent = 0,
                   PageCount = book.PageCount,
                   TotalQuantity = book.TotalQuantity,
                   Language = book.Language,
                   Format = book.Format,
                   StockForSell = book.StockForSell,
                   StockForRent = book.StockForRent,
                   AverageRating = book.AverageRating,
                   ReviewCount = book.ReviewCount,
                   SoldCount = book.SoldCount,
                   Status = book.Status,
                   IsActive = book.IsActive,
                   IsFeatured = book.IsFeatured,
                   IsNew = book.CreatedAt >= now.AddDays(-30),
                   CreatedAt = book.CreatedAt,
                   UpdatedAt = book.UpdatedAt
               };
    }

    /// <summary>Nguồn sách đã publish dùng để tính facet (đếm số theo nhóm).</summary>
    private IQueryable<Book> BuildPublishedBookQuery()
    {
        return _context.Books.AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive && x.Status == BookStatus.Published);
    }

    private async Task<ServiceResult<CatalogDetailResponse>> BuildCatalogDetailAsync(Guid bookId, CancellationToken cancellationToken)
    {
        var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(x => x.Id == bookId, cancellationToken);
        if (book is null)
        {
            return ServiceResult<CatalogDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        var author = book.AuthorId.HasValue
            ? await _context.Authors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == book.AuthorId.Value, cancellationToken)
            : null;
        var publisher = book.PublisherId.HasValue
            ? await _context.Publishers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == book.PublisherId.Value, cancellationToken)
            : null;
        var category = book.CategoryId.HasValue
            ? await _context.CommonCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == book.CategoryId.Value, cancellationToken)
            : null;

        var reviews = await _context.Reviews
            .AsNoTracking()
            .Where(x => x.BookId == bookId && !x.IsDeleted && x.Status == ReviewStatus.Approved)
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .Select(x => new Core.Models.Content.ReviewListItemResponse
            {
                Id = x.Id,
                BookId = x.BookId,
                BookTitle = book.Title,
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

        var related = await BuildCatalogQuery()
            .Where(x => x.Id != bookId && x.CategoryName == (category == null ? "" : category.Name))
            .OrderByDescending(x => x.SoldCount)
            .Take(8)
            .ToListAsync(cancellationToken);

        return ServiceResult<CatalogDetailResponse>.Success(new CatalogDetailResponse
        {
            Id = book.Id,
            Title = book.Title,
            Subtitle = book.Subtitle,
            Slug = book.Slug,
            Isbn = book.Isbn,
            Description = book.Description,
            CoverImageUrl = book.CoverImageUrl,
            CategoryId = book.CategoryId,
            CategoryName = category?.Name,
            AuthorId = book.AuthorId,
            AuthorName = author?.Name,
            PublisherId = book.PublisherId,
            PublisherName = publisher?.Name,
            PriceForSell = book.PriceForSell,
            PriceForRent = book.PriceForRent,
            PageCount = book.PageCount,
            PublicationYear = book.PublicationYear,
            Language = book.Language,
            Format = book.Format,
            Weight = ServiceHelpers.ParseWeight(book.Weight),
            Dimensions = book.Dimensions,
            StockForSell = book.StockForSell,
            StockForRent = book.StockForRent,
            AverageRating = book.AverageRating,
            ReviewCount = book.ReviewCount,
            SoldCount = book.SoldCount,
            IsFeatured = book.IsFeatured,
            Reviews = reviews,
            RelatedBooks = related
        }, "Lấy chi tiết sách thành công.");
    }

    private async Task<List<CartItemResponse>> BuildCartItemsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var items = await _context.CartItems
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new CartItemResponse
            {
                Id = x.Id,
                BookId = x.BookId,
                Title = x.Book.Title,
                BookTitle = x.Book.Title,
                BookSlug = x.Book.Slug,
                CoverImageUrl = x.Book.CoverImageUrl,
                UnitType = x.UnitType,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                LineTotal = ServiceHelpers.RoundMoney(x.UnitPrice * x.Quantity),
                AvailableStock = x.UnitType == UnitType.Rent ? x.Book.StockForRent : x.Book.StockForSell
            })
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return items;
        }

        var bookIds = items.Select(x => x.BookId).Distinct().ToList();
        var wishlisted = await _context.WishlistItems
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId && bookIds.Contains(x.BookId))
            .Select(x => x.BookId)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.IsWishlisted = wishlisted.Contains(item.BookId);
        }

        return items;
    }

    private async Task<ServiceResult<CartResponse>> BuildCartResponseAsync(IReadOnlyCollection<CartItemResponse> items, string? couponCode, CancellationToken cancellationToken)
    {
        var subTotal = ServiceHelpers.RoundMoney(items.Sum(x => x.LineTotal));

        var discountAmount = 0m;
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var coupon = await _context.Coupons.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == couponCode.Trim().ToUpperInvariant() && !x.IsDeleted
                                       && x.Status == CouponStatus.Active
                                       && x.StartDate <= DateTime.UtcNow && x.EndDate >= DateTime.UtcNow, cancellationToken);

            if (coupon is not null)
            {
                discountAmount = coupon.DiscountType == DiscountType.Percent
                    ? ServiceHelpers.RoundMoney(subTotal * coupon.DiscountValue / 100m)
                    : ServiceHelpers.RoundMoney(coupon.DiscountValue);

                if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                {
                    discountAmount = coupon.MaxDiscountAmount.Value;
                }

                discountAmount = Math.Min(discountAmount, subTotal);
            }
        }

        var shippingAmount = subTotal - discountAmount >= FreeShippingThreshold ? 0m : StandardShippingFee;

        var finalAmount = ServiceHelpers.RoundMoney(subTotal - discountAmount + shippingAmount);
        var freeShippingRemaining = Math.Max(0m, FreeShippingThreshold - (subTotal - discountAmount));

        return ServiceResult<CartResponse>.Success(new CartResponse
        {
            Items = items,
            CouponCode = couponCode,
            SubTotal = subTotal,
            ShippingAmount = shippingAmount,
            DiscountAmount = discountAmount,
            TotalAmount = finalAmount,
            FinalAmount = finalAmount,
            FreeShippingThreshold = FreeShippingThreshold,
            FreeShippingRemaining = freeShippingRemaining,
            ItemCount = items.Count,
            TotalQuantity = items.Sum(x => x.Quantity)
        }, "Lấy giỏ hàng thành công.");
    }
}
