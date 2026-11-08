using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Constants;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Book;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class BookCopyService : IBookCopyService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public BookCopyService(AppDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<PagedResult<BookCopyListItemResponse>>> SearchAsync(BookSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.BookCopies
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.Book!.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Barcode.ToLower().Contains(keyword) ||
                (x.Book != null && x.Book.Title.ToLower().Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Barcode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new BookCopyListItemResponse
            {
                Id = x.Id,
                BookId = x.BookId,
                Barcode = x.Barcode,
                ConditionStatus = x.ConditionStatus,
                ShelfCode = x.ShelfCode,
                WarehouseId = x.WarehouseId,
                WarehouseName = x.Warehouse!.Name,
                AcquiredDate = x.AcquiredDate,
                DueDate = x.DueDate,
                ReturnedDate = x.ReturnedDate,
                Status = x.Status,
                CurrentOrderId = x.CurrentOrderId,
                Notes = x.Notes,
                IsDeleted = x.IsDeleted
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<BookCopyListItemResponse>>.Success(new PagedResult<BookCopyListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách bản sao thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<BookCopyListItemResponse>>> GetByBookAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        var items = await _context.BookCopies
            .AsNoTracking()
            .Where(x => x.BookId == bookId && !x.IsDeleted)
            .OrderBy(x => x.Barcode)
            .Select(x => new BookCopyListItemResponse
            {
                Id = x.Id,
                BookId = x.BookId,
                Barcode = x.Barcode,
                ConditionStatus = x.ConditionStatus,
                ShelfCode = x.ShelfCode,
                WarehouseId = x.WarehouseId,
                WarehouseName = x.Warehouse!.Name,
                AcquiredDate = x.AcquiredDate,
                DueDate = x.DueDate,
                ReturnedDate = x.ReturnedDate,
                Status = x.Status,
                CurrentOrderId = x.CurrentOrderId,
                Notes = x.Notes,
                IsDeleted = x.IsDeleted
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<BookCopyListItemResponse>>.Success(items, "Lấy danh sách bản sao thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<BookCopyListItemResponse>>> CreateAsync(CreateBookCopyRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == request.BookId, cancellationToken);
        if (book is null)
        {
            return ServiceResult<IReadOnlyCollection<BookCopyListItemResponse>>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        var quantity = request.Quantity <= 0 ? 1 : request.Quantity;
        var existingCount = await _context.BookCopies.CountAsync(x => x.BookId == request.BookId, cancellationToken);

        for (var i = 1; i <= quantity; i++)
        {
            _context.BookCopies.Add(new BookCopy
            {
                Id = Guid.NewGuid(),
                BookId = request.BookId,
                Barcode = string.IsNullOrWhiteSpace(request.Barcode)
                    ? ServiceHelpers.BuildBarcode(request.BookId, existingCount + i)
                    : $"{request.Barcode.Trim()}-{i:D3}",
                ConditionStatus = request.ConditionStatus,
                ShelfCode = request.ShelfCode,
                WarehouseId = request.WarehouseId,
                AcquiredDate = request.AcquiredDate ?? DateTime.UtcNow,
                Status = BookCopyStatus.Available,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = operatorId.ToString()
            });
        }

        book.TotalQuantity += quantity;
        book.StockForSell += quantity;
        book.UpdatedAt = DateTime.UtcNow;
        book.UpdatedBy = operatorId.ToString();

        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạo bản sao", "BookCopy", $"Tạo {quantity} bản sao cho sách {book.Title}.");

        return await GetByBookAsync(request.BookId, cancellationToken);
    }

    public async Task<ServiceResult<BookCopyListItemResponse>> UpdateAsync(Guid id, UpdateBookCopyRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var copy = await _context.BookCopies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (copy is null)
        {
            return ServiceResult<BookCopyListItemResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy bản sao.");
        }

        if (request.ConditionStatus is not null) copy.ConditionStatus = request.ConditionStatus;
        if (request.ShelfCode is not null) copy.ShelfCode = request.ShelfCode;
        if (request.WarehouseId.HasValue) copy.WarehouseId = request.WarehouseId;
        if (request.AcquiredDate.HasValue) copy.AcquiredDate = request.AcquiredDate;
        if (request.DueDate.HasValue) copy.DueDate = request.DueDate;
        if (request.ReturnedDate.HasValue) copy.ReturnedDate = request.ReturnedDate;
        if (request.Status.HasValue) copy.Status = request.Status.Value;
        if (request.Notes is not null) copy.Notes = request.Notes;

        copy.UpdatedAt = DateTime.UtcNow;
        copy.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Cập nhật bản sao", "BookCopy", $"Cập nhật bản sao {copy.Barcode}.");

        var result = await _context.BookCopies
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new BookCopyListItemResponse
            {
                Id = x.Id,
                BookId = x.BookId,
                Barcode = x.Barcode,
                ConditionStatus = x.ConditionStatus,
                ShelfCode = x.ShelfCode,
                WarehouseId = x.WarehouseId,
                WarehouseName = x.Warehouse!.Name,
                AcquiredDate = x.AcquiredDate,
                DueDate = x.DueDate,
                ReturnedDate = x.ReturnedDate,
                Status = x.Status,
                CurrentOrderId = x.CurrentOrderId,
                Notes = x.Notes,
                IsDeleted = x.IsDeleted
            })
            .FirstOrDefaultAsync(cancellationToken);

        return ServiceResult<BookCopyListItemResponse>.Success(result!, "Cập nhật bản sao thành công.");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var copy = await _context.BookCopies.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (copy is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy bản sao.");
        }

        copy.IsDeleted = true;
        copy.DeletedAt = DateTime.UtcNow;
        copy.DeletedBy = operatorId.ToString();
        copy.UpdatedAt = DateTime.UtcNow;
        copy.UpdatedBy = operatorId.ToString();

        var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == copy.BookId, cancellationToken);
        if (book is not null && book.TotalQuantity > 0)
        {
            book.TotalQuantity--;
            if (book.StockForSell > 0) book.StockForSell--;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa mềm bản sao", "BookCopy", $"Xóa mềm bản sao {copy.Barcode}.");

        return ServiceResult<bool>.Success(true, "Xóa mềm bản sao thành công.");
    }

    private async Task WriteAuditAsync(Guid? operatorId, string action, string module, string note)
    {
        await _auditLogService.WriteAsync(new AuditLog
        {
            ActorUserId = operatorId,
            Action = action,
            Module = module,
            Note = note,
            EntityType = "BookCopy",
            IsSuccess = true
        });
    }

    // ------------------------------------------------------------------ Tìm kiếm bản sao (FE: GET /book-copies)

    public async Task<ServiceResult<PagedResult<BookCopyListItemResponse>>> SearchCopiesAsync(BookCopySearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.BookCopies
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (request.BookId.HasValue)
        {
            var bookId = request.BookId.Value;
            query = query.Where(x => x.BookId == bookId);
        }

        if (request.WarehouseId.HasValue)
        {
            var warehouseId = request.WarehouseId.Value;
            query = query.Where(x => x.WarehouseId == warehouseId);
        }

        if (request.Status.HasValue)
        {
            var status = request.Status.Value;
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Barcode.ToLower().Contains(keyword) ||
                (x.Book != null && x.Book.Title.ToLower().Contains(keyword)) ||
                (x.ShelfCode != null && x.ShelfCode.ToLower().Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Barcode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new BookCopyListItemResponse
            {
                Id = x.Id,
                BookId = x.BookId,
                BookTitle = x.Book.Title,
                Barcode = x.Barcode,
                ConditionStatus = x.ConditionStatus,
                ShelfCode = x.ShelfCode,
                WarehouseId = x.WarehouseId,
                WarehouseName = x.Warehouse != null ? x.Warehouse.Name : null,
                AcquiredDate = x.AcquiredDate,
                DueDate = x.DueDate,
                ReturnedDate = x.ReturnedDate,
                Status = x.Status,
                CurrentOrderId = x.CurrentOrderId,
                Notes = x.Notes,
                IsDeleted = x.IsDeleted
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<BookCopyListItemResponse>>.Success(new PagedResult<BookCopyListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách bản sao thành công.");
    }
}
