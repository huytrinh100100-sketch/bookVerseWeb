using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Constants;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Book;
using BookVerse.Core.Models.Partner;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class PartnerService : IPartnerService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public PartnerService(AppDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    // ------------------------------------------------------------------ Tác giả

    public async Task<ServiceResult<PagedResult<AuthorListItemResponse>>> SearchAuthorsAsync(PartnerSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.Authors.AsNoTracking();

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (request.IsDeleted.HasValue)
        {
            query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
        }

        if (request.AgreementStatus.HasValue)
        {
            query = query.Where(x => x.AgreementStatus == request.AgreementStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(keyword) ||
                (x.Code != null && x.Code.ToLower().Contains(keyword)) ||
                (x.Country != null && x.Country.ToLower().Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuthorListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Country = x.Country,
                AvatarUrl = x.AvatarUrl,
                AgreementStatus = x.AgreementStatus,
                AgreementExpiresAt = x.AgreementExpiresAt,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                BookCount = x.Books.Count(b => !b.IsDeleted),
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<AuthorListItemResponse>>.Success(new PagedResult<AuthorListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách tác giả thành công.");
    }

    public async Task<ServiceResult<AuthorDetailResponse>> GetAuthorAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var author = await _context.Authors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (author is null)
        {
            return ServiceResult<AuthorDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tác giả.");
        }

        var books = await _context.Books
            .AsNoTracking()
            .Where(x => x.AuthorId == id && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new BookListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Slug = x.Slug,
                CoverImageUrl = x.CoverImageUrl,
                AuthorName = x.Author!.Name,
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

        return ServiceResult<AuthorDetailResponse>.Success(new AuthorDetailResponse
        {
            Id = author.Id,
            Code = author.Code,
            Name = author.Name,
            Country = author.Country,
            Website = author.Website,
            Email = author.Email,
            PhoneNumber = author.PhoneNumber,
            Biography = author.Biography,
            AvatarUrl = author.AvatarUrl,
            AgreementStatus = author.AgreementStatus,
            AgreementExpiresAt = author.AgreementExpiresAt,
            IsActive = author.IsActive,
            IsDeleted = author.IsDeleted,
            CreatedAt = author.CreatedAt,
            Books = books
        }, "Lấy chi tiết tác giả thành công.");
    }

    public async Task<ServiceResult<AuthorDetailResponse>> CreateAuthorAsync(CreateAuthorRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        if (await _context.Authors.AnyAsync(x => x.Code == request.Code.Trim() && !x.IsDeleted, cancellationToken))
        {
            return ServiceResult<AuthorDetailResponse>.Failure(HttpStatusCode.Conflict, "Mã tác giả đã tồn tại.");
        }

        var author = new Author
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Country = request.Country,
            Website = request.Website,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Biography = request.Biography,
            AvatarUrl = request.AvatarUrl,
            AgreementStatus = request.AgreementStatus,
            AgreementExpiresAt = request.AgreementExpiresAt,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        _context.Authors.Add(author);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạo tác giả", "Author", $"Tạo tác giả {author.Name}.");

        return await GetAuthorAsync(author.Id, cancellationToken);
    }

    public async Task<ServiceResult<AuthorDetailResponse>> UpdateAuthorAsync(Guid id, UpdateAuthorRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var author = await _context.Authors.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (author is null)
        {
            return ServiceResult<AuthorDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tác giả.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name)) author.Name = request.Name.Trim();
        if (request.Country is not null) author.Country = request.Country;
        if (request.Website is not null) author.Website = request.Website;
        if (request.Email is not null) author.Email = request.Email;
        if (request.PhoneNumber is not null) author.PhoneNumber = request.PhoneNumber;
        if (request.Biography is not null) author.Biography = request.Biography;
        if (request.AvatarUrl is not null) author.AvatarUrl = request.AvatarUrl;
        if (request.AgreementStatus.HasValue) author.AgreementStatus = request.AgreementStatus.Value;
        if (request.AgreementExpiresAt.HasValue) author.AgreementExpiresAt = request.AgreementExpiresAt;
        if (request.IsActive.HasValue) author.IsActive = request.IsActive.Value;

        author.UpdatedAt = DateTime.UtcNow;
        author.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Cập nhật tác giả", "Author", $"Cập nhật tác giả {author.Name}.");

        return await GetAuthorAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> DeleteAuthorAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var author = await _context.Authors.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (author is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tác giả.");
        }

        var hasBooks = await _context.Books.AnyAsync(x => x.AuthorId == id && !x.IsDeleted, cancellationToken);
        if (hasBooks)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Tác giả đang có sách, không thể xóa.");
        }

        author.IsDeleted = true;
        author.IsActive = false;
        author.DeletedAt = DateTime.UtcNow;
        author.DeletedBy = operatorId.ToString();
        author.UpdatedAt = DateTime.UtcNow;
        author.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa tác giả", "Author", $"Xóa tác giả {author.Name}.");

        return ServiceResult<bool>.Success(true, "Xóa tác giả thành công.");
    }

    // ------------------------------------------------------------------ NXB

    public async Task<ServiceResult<PagedResult<PublisherListItemResponse>>> SearchPublishersAsync(PartnerSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.Publishers.AsNoTracking();

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (request.IsDeleted.HasValue)
        {
            query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
        }

        if (request.AgreementStatus.HasValue)
        {
            query = query.Where(x => x.AgreementStatus == request.AgreementStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Name.ToLower().Contains(keyword) ||
                (x.Code != null && x.Code.ToLower().Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PublisherListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Country = x.Country,
                LogoUrl = x.LogoUrl,
                AgreementStatus = x.AgreementStatus,
                AgreementExpiresAt = x.AgreementExpiresAt,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                BookCount = x.Books.Count(b => !b.IsDeleted),
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<PublisherListItemResponse>>.Success(new PagedResult<PublisherListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách nhà xuất bản thành công.");
    }

    public async Task<ServiceResult<PublisherDetailResponse>> GetPublisherAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var publisher = await _context.Publishers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (publisher is null)
        {
            return ServiceResult<PublisherDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy nhà xuất bản.");
        }

        var books = await _context.Books
            .AsNoTracking()
            .Where(x => x.PublisherId == id && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new BookListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Slug = x.Slug,
                CoverImageUrl = x.CoverImageUrl,
                PublisherName = x.Publisher!.Name,
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

        return ServiceResult<PublisherDetailResponse>.Success(new PublisherDetailResponse
        {
            Id = publisher.Id,
            Code = publisher.Code,
            Name = publisher.Name,
            Country = publisher.Country,
            Website = publisher.Website,
            Email = publisher.Email,
            PhoneNumber = publisher.PhoneNumber,
            Description = publisher.Description,
            LogoUrl = publisher.LogoUrl,
            AgreementStatus = publisher.AgreementStatus,
            AgreementExpiresAt = publisher.AgreementExpiresAt,
            IsActive = publisher.IsActive,
            IsDeleted = publisher.IsDeleted,
            CreatedAt = publisher.CreatedAt,
            Books = books
        }, "Lấy chi tiết nhà xuất bản thành công.");
    }

    public async Task<ServiceResult<PublisherDetailResponse>> CreatePublisherAsync(CreatePublisherRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        if (await _context.Publishers.AnyAsync(x => x.Code == request.Code.Trim() && !x.IsDeleted, cancellationToken))
        {
            return ServiceResult<PublisherDetailResponse>.Failure(HttpStatusCode.Conflict, "Mã nhà xuất bản đã tồn tại.");
        }

        var publisher = new Publisher
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Country = request.Country,
            Website = request.Website,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Description = request.Description,
            LogoUrl = request.LogoUrl,
            AgreementStatus = request.AgreementStatus,
            AgreementExpiresAt = request.AgreementExpiresAt,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        _context.Publishers.Add(publisher);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạo nhà xuất bản", "Publisher", $"Tạo NXB {publisher.Name}.");

        return await GetPublisherAsync(publisher.Id, cancellationToken);
    }

    public async Task<ServiceResult<PublisherDetailResponse>> UpdatePublisherAsync(Guid id, UpdatePublisherRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var publisher = await _context.Publishers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (publisher is null)
        {
            return ServiceResult<PublisherDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy nhà xuất bản.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name)) publisher.Name = request.Name.Trim();
        if (request.Country is not null) publisher.Country = request.Country;
        if (request.Website is not null) publisher.Website = request.Website;
        if (request.Email is not null) publisher.Email = request.Email;
        if (request.PhoneNumber is not null) publisher.PhoneNumber = request.PhoneNumber;
        if (request.Description is not null) publisher.Description = request.Description;
        if (request.LogoUrl is not null) publisher.LogoUrl = request.LogoUrl;
        if (request.AgreementStatus.HasValue) publisher.AgreementStatus = request.AgreementStatus.Value;
        if (request.AgreementExpiresAt.HasValue) publisher.AgreementExpiresAt = request.AgreementExpiresAt;
        if (request.IsActive.HasValue) publisher.IsActive = request.IsActive.Value;

        publisher.UpdatedAt = DateTime.UtcNow;
        publisher.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Cập nhật nhà xuất bản", "Publisher", $"Cập nhật NXB {publisher.Name}.");

        return await GetPublisherAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> DeletePublisherAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var publisher = await _context.Publishers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (publisher is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy nhà xuất bản.");
        }

        var hasBooks = await _context.Books.AnyAsync(x => x.PublisherId == id && !x.IsDeleted, cancellationToken);
        if (hasBooks)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Nhà xuất bản đang có sách, không thể xóa.");
        }

        publisher.IsDeleted = true;
        publisher.IsActive = false;
        publisher.DeletedAt = DateTime.UtcNow;
        publisher.DeletedBy = operatorId.ToString();
        publisher.UpdatedAt = DateTime.UtcNow;
        publisher.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa nhà xuất bản", "Publisher", $"Xóa NXB {publisher.Name}.");

        return ServiceResult<bool>.Success(true, "Xóa nhà xuất bản thành công.");
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

    // ------------------------------------------------------------------ Đối tác hợp nhất (FE: /api/v1/partners)

    public async Task<ServiceResult<PagedResult<PartnerListItemResponse>>> SearchPartnersAsync(PartnerSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var authors = _context.Authors.AsNoTracking();
        var publishers = _context.Publishers.AsNoTracking();

        if (request.Type.HasValue)
        {
            if (request.Type.Value == PartnerType.Author)
            {
                publishers = publishers.Where(x => false);
            }
            else
            {
                authors = authors.Where(x => false);
            }
        }

        if (request.IsActive.HasValue)
        {
            authors = authors.Where(x => x.IsActive == request.IsActive.Value);
            publishers = publishers.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (request.IsDeleted.HasValue)
        {
            authors = authors.Where(x => x.IsDeleted == request.IsDeleted.Value);
            publishers = publishers.Where(x => x.IsDeleted == request.IsDeleted.Value);
        }

        if (request.AgreementStatus.HasValue)
        {
            authors = authors.Where(x => x.AgreementStatus == request.AgreementStatus.Value);
            publishers = publishers.Where(x => x.AgreementStatus == request.AgreementStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Country))
        {
            var country = request.Country.Trim();
            authors = authors.Where(x => x.Country != null && x.Country.Contains(country));
            publishers = publishers.Where(x => x.Country != null && x.Country.Contains(country));
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            authors = authors.Where(x =>
                x.Name.ToLower().Contains(keyword) ||
                (x.Code != null && x.Code.ToLower().Contains(keyword)));
            publishers = publishers.Where(x =>
                x.Name.ToLower().Contains(keyword) ||
                (x.Code != null && x.Code.ToLower().Contains(keyword)));
        }

        var authorQuery = authors.Select(x => new PartnerListItemResponse
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Type = PartnerType.Author,
            Country = x.Country,
            Website = x.Website,
            Email = x.Email,
            PhoneNumber = x.PhoneNumber,
            Biography = x.Biography,
            AvatarUrl = x.AvatarUrl,
            Description = null,
            LogoUrl = null,
            BookCount = x.Books.Count(b => !b.IsDeleted),
            AgreementStatus = x.AgreementStatus,
            AgreementExpiresAt = x.AgreementExpiresAt,
            IsActive = x.IsActive,
            IsDeleted = x.IsDeleted,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });

        var publisherQuery = publishers.Select(x => new PartnerListItemResponse
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Type = PartnerType.Publisher,
            Country = x.Country,
            Website = x.Website,
            Email = x.Email,
            PhoneNumber = x.PhoneNumber,
            Description = x.Description,
            LogoUrl = x.LogoUrl,
            Biography = null,
            AvatarUrl = null,
            BookCount = x.Books.Count(b => !b.IsDeleted),
            AgreementStatus = x.AgreementStatus,
            AgreementExpiresAt = x.AgreementExpiresAt,
            IsActive = x.IsActive,
            IsDeleted = x.IsDeleted,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });

        var combined = authorQuery.Concat(publisherQuery);

        var totalItems = await combined.CountAsync(cancellationToken);

        var items = await combined
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<PartnerListItemResponse>>.Success(new PagedResult<PartnerListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách đối tác thành công.");
    }

    public async Task<ServiceResult<PartnerDetailResponse>> GetPartnerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var author = await _context.Authors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (author is not null)
        {
            var authorBooks = await _context.Books
                .AsNoTracking()
                .Where(x => x.AuthorId == id && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new BookListItemResponse
                {
                    Id = x.Id,
                    Code = x.Code,
                    Title = x.Title,
                    Slug = x.Slug,
                    CoverImageUrl = x.CoverImageUrl,
                    AuthorName = x.Author!.Name,
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

            return ServiceResult<PartnerDetailResponse>.Success(new PartnerDetailResponse
            {
                Id = author.Id,
                Code = author.Code,
                Name = author.Name,
                Type = PartnerType.Author,
                Country = author.Country,
                Website = author.Website,
                Email = author.Email,
                PhoneNumber = author.PhoneNumber,
                Biography = author.Biography,
                AvatarUrl = author.AvatarUrl,
                BookCount = authorBooks.Count,
                AgreementStatus = author.AgreementStatus,
                AgreementExpiresAt = author.AgreementExpiresAt,
                IsActive = author.IsActive,
                IsDeleted = author.IsDeleted,
                CreatedAt = author.CreatedAt,
                Books = authorBooks
            }, "Lấy chi tiết đối tác thành công.");
        }

        var publisher = await _context.Publishers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (publisher is null)
        {
            return ServiceResult<PartnerDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đối tác.");
        }

        var publisherBooks = await _context.Books
            .AsNoTracking()
            .Where(x => x.PublisherId == id && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new BookListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Slug = x.Slug,
                CoverImageUrl = x.CoverImageUrl,
                PublisherName = x.Publisher!.Name,
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

        return ServiceResult<PartnerDetailResponse>.Success(new PartnerDetailResponse
        {
            Id = publisher.Id,
            Code = publisher.Code,
            Name = publisher.Name,
            Type = PartnerType.Publisher,
            Country = publisher.Country,
            Website = publisher.Website,
            Email = publisher.Email,
            PhoneNumber = publisher.PhoneNumber,
            Description = publisher.Description,
            LogoUrl = publisher.LogoUrl,
            BookCount = publisherBooks.Count,
            AgreementStatus = publisher.AgreementStatus,
            AgreementExpiresAt = publisher.AgreementExpiresAt,
            IsActive = publisher.IsActive,
            IsDeleted = publisher.IsDeleted,
            CreatedAt = publisher.CreatedAt,
            Books = publisherBooks
        }, "Lấy chi tiết đối tác thành công.");
    }

    public async Task<ServiceResult<PartnerDetailResponse>> CreatePartnerAsync(CreatePartnerRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ServiceResult<PartnerDetailResponse>.Failure(HttpStatusCode.BadRequest, "Tên đối tác không được để trống.");
        }

        if (request.Type == PartnerType.Author)
        {
            var author = new Author
            {
                Id = Guid.NewGuid(),
                Code = string.IsNullOrWhiteSpace(request.Code)
                    ? $"AUTH-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}"
                    : request.Code.Trim(),
                Name = request.Name.Trim(),
                Country = request.Country,
                Website = request.Website,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Biography = request.Biography,
                AvatarUrl = request.AvatarUrl,
                AgreementStatus = request.AgreementStatus,
                AgreementExpiresAt = request.AgreementExpiresAt,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = operatorId.ToString()
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync(cancellationToken);

            await WriteAuditAsync(operatorId, "Tạo tác giả", "Partner", $"Tạo tác giả {author.Name}.");

            return await GetPartnerAsync(author.Id, cancellationToken);
        }

        var publisher = new Publisher
        {
            Id = Guid.NewGuid(),
            Code = string.IsNullOrWhiteSpace(request.Code)
                ? $"PUB-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}"
                : request.Code.Trim(),
            Name = request.Name.Trim(),
            Country = request.Country,
            Website = request.Website,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Description = request.Description,
            LogoUrl = request.LogoUrl,
            AgreementStatus = request.AgreementStatus,
            AgreementExpiresAt = request.AgreementExpiresAt,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        _context.Publishers.Add(publisher);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạo nhà xuất bản", "Partner", $"Tạo NXB {publisher.Name}.");

        return await GetPartnerAsync(publisher.Id, cancellationToken);
    }

    public async Task<ServiceResult<PartnerDetailResponse>> UpdatePartnerAsync(Guid id, UpdatePartnerRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var author = await _context.Authors.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (author is not null)
        {
            if (!string.IsNullOrWhiteSpace(request.Name)) author.Name = request.Name.Trim();
            if (request.Country is not null) author.Country = request.Country;
            if (request.Website is not null) author.Website = request.Website;
            if (request.Email is not null) author.Email = request.Email;
            if (request.PhoneNumber is not null) author.PhoneNumber = request.PhoneNumber;
            if (request.Biography is not null) author.Biography = request.Biography;
            if (request.AvatarUrl is not null) author.AvatarUrl = request.AvatarUrl;
            if (request.AgreementStatus.HasValue) author.AgreementStatus = request.AgreementStatus.Value;
            if (request.AgreementExpiresAt.HasValue) author.AgreementExpiresAt = request.AgreementExpiresAt.Value;
            if (request.IsActive.HasValue) author.IsActive = request.IsActive.Value;

            author.UpdatedAt = DateTime.UtcNow;
            author.UpdatedBy = operatorId.ToString();
            await _context.SaveChangesAsync(cancellationToken);

            await WriteAuditAsync(operatorId, "Cập nhật tác giả", "Partner", $"Cập nhật tác giả {author.Name}.");

            return await GetPartnerAsync(id, cancellationToken);
        }

        var publisher = await _context.Publishers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (publisher is null)
        {
            return ServiceResult<PartnerDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đối tác.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name)) publisher.Name = request.Name.Trim();
        if (request.Country is not null) publisher.Country = request.Country;
        if (request.Website is not null) publisher.Website = request.Website;
        if (request.Email is not null) publisher.Email = request.Email;
        if (request.PhoneNumber is not null) publisher.PhoneNumber = request.PhoneNumber;
        if (request.Description is not null) publisher.Description = request.Description;
        if (request.LogoUrl is not null) publisher.LogoUrl = request.LogoUrl;
        if (request.AgreementStatus.HasValue) publisher.AgreementStatus = request.AgreementStatus.Value;
        if (request.AgreementExpiresAt.HasValue) publisher.AgreementExpiresAt = request.AgreementExpiresAt.Value;
        if (request.IsActive.HasValue) publisher.IsActive = request.IsActive.Value;

        publisher.UpdatedAt = DateTime.UtcNow;
        publisher.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Cập nhật nhà xuất bản", "Partner", $"Cập nhật NXB {publisher.Name}.");

        return await GetPartnerAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> DeletePartnerAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var author = await _context.Authors.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (author is not null)
        {
            var hasBooks = await _context.Books.AnyAsync(x => x.AuthorId == id && !x.IsDeleted, cancellationToken);
            if (hasBooks)
            {
                return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Tác giả đang có sách, không thể xóa.");
            }

            author.IsDeleted = true;
            author.IsActive = false;
            author.DeletedAt = DateTime.UtcNow;
            author.DeletedBy = operatorId.ToString();
            author.UpdatedAt = DateTime.UtcNow;
            author.UpdatedBy = operatorId.ToString();
            await _context.SaveChangesAsync(cancellationToken);

            await WriteAuditAsync(operatorId, "Xóa tác giả", "Partner", $"Xóa tác giả {author.Name}.");

            return ServiceResult<bool>.Success(true, "Xóa tác giả thành công.");
        }

        var publisher = await _context.Publishers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (publisher is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đối tác.");
        }

        var publisherHasBooks = await _context.Books.AnyAsync(x => x.PublisherId == id && !x.IsDeleted, cancellationToken);
        if (publisherHasBooks)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Nhà xuất bản đang có sách, không thể xóa.");
        }

        publisher.IsDeleted = true;
        publisher.IsActive = false;
        publisher.DeletedAt = DateTime.UtcNow;
        publisher.DeletedBy = operatorId.ToString();
        publisher.UpdatedAt = DateTime.UtcNow;
        publisher.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa nhà xuất bản", "Partner", $"Xóa NXB {publisher.Name}.");

        return ServiceResult<bool>.Success(true, "Xóa nhà xuất bản thành công.");
    }
}
