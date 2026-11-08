using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Constants;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Book;
using BookVerse.Core.Models.Inventory;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private const int DefaultLowStockThreshold = 5;

    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public InventoryService(AppDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<PagedResult<InventoryListItemResponse>>> SearchAsync(InventorySearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = from book in _context.Books.AsNoTracking()
                    join author in _context.Authors.AsNoTracking() on book.AuthorId equals author.Id into authorJoin
                    from author in authorJoin.DefaultIfEmpty()
                    join category in _context.CommonCategories.AsNoTracking() on book.CategoryId equals category.Id into categoryJoin
                    from category in categoryJoin.DefaultIfEmpty()
                    where !book.IsDeleted
                    select new InventoryListItemResponse
                    {
                        BookId = book.Id,
                        Title = book.Title,
                        CoverImageUrl = book.CoverImageUrl,
                        Isbn = book.Isbn,
                        AuthorId = book.AuthorId,
                        AuthorName = author != null ? author.Name : null,
                        CategoryId = book.CategoryId,
                        CategoryName = category != null ? category.Name : null,
                        PriceForSell = book.PriceForSell,
                        PriceForRent = book.PriceForRent,
                        TotalQuantity = book.TotalQuantity,
                        StockForSell = book.StockForSell,
                        StockForRent = book.StockForRent,
                        AvailableCopies = book.TotalQuantity,
                        LentQuantity = 0,
                        Threshold = DefaultLowStockThreshold
                    };

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Title.ToLower().Contains(keyword) ||
                (x.Isbn != null && x.Isbn.ToLower().Contains(keyword)) ||
                (x.AuthorName != null && x.AuthorName.ToLower().Contains(keyword)));
        }

        if (request.Status.HasValue)
        {
            var status = request.Status.Value;
            query = query.Where(x => _context.BookCopies.Any(c => c.BookId == x.BookId && !c.IsDeleted && c.Status == status));
        }

        if (request.WarehouseId.HasValue)
        {
            var warehouseId = request.WarehouseId.Value;
            query = query.Where(x => _context.BookCopies.Any(c => c.BookId == x.BookId && !c.IsDeleted && c.WarehouseId == warehouseId));
        }

        var threshold = request.LowStockThreshold ?? DefaultLowStockThreshold;
        query = query.Where(x => x.TotalQuantity >= 0);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.TotalQuantity)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.LentQuantity = await _context.BookCopies.CountAsync(x => x.BookId == item.BookId && !x.IsDeleted && x.Status == BookCopyStatus.Lent, cancellationToken);
            item.IsLowStock = item.StockForSell <= threshold;
        }

        return ServiceResult<PagedResult<InventoryListItemResponse>>.Success(new PagedResult<InventoryListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy báo cáo tồn kho thành công.");
    }

    public async Task<ServiceResult<InventoryListItemResponse>> GetByBookAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(x => x.Id == bookId, cancellationToken);
        if (book is null)
        {
            return ServiceResult<InventoryListItemResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        var lent = await _context.BookCopies.CountAsync(x => x.BookId == bookId && !x.IsDeleted && x.Status == BookCopyStatus.Lent, cancellationToken);

        return ServiceResult<InventoryListItemResponse>.Success(new InventoryListItemResponse
        {
            BookId = book.Id,
            Title = book.Title,
            CoverImageUrl = book.CoverImageUrl,
            Isbn = book.Isbn,
            PriceForSell = book.PriceForSell,
            PriceForRent = book.PriceForRent,
            TotalQuantity = book.TotalQuantity,
            StockForSell = book.StockForSell,
            StockForRent = book.StockForRent,
            AvailableCopies = book.TotalQuantity - lent,
            LentQuantity = lent,
            Threshold = DefaultLowStockThreshold,
            IsLowStock = book.StockForSell <= DefaultLowStockThreshold
        }, "Lấy tồn kho sách thành công.");
    }

    public async Task<ServiceResult<bool>> AdjustStockAsync(AdjustStockRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == request.BookId, cancellationToken);
        if (book is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        if (request.Quantity == 0)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Số lượng điều chỉnh không được bằng 0.");
        }

        if (request.Quantity > 0)
        {
            // Nhập thêm: tạo bản sao vật lý tương ứng
            var existingCount = await _context.BookCopies.CountAsync(x => x.BookId == book.Id, cancellationToken);
            for (var i = 1; i <= request.Quantity; i++)
            {
                _context.BookCopies.Add(new BookCopy
                {
                    Id = Guid.NewGuid(),
                    BookId = book.Id,
                    Barcode = ServiceHelpers.BuildBarcode(book.Id, existingCount + i),
                    ConditionStatus = "Mới",
                    WarehouseId = request.WarehouseId,
                    Status = BookCopyStatus.Available,
                    AcquiredDate = DateTime.UtcNow,
                    Notes = request.Reason,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = operatorId.ToString()
                });
            }

            book.TotalQuantity += request.Quantity;
            book.StockForSell += request.Quantity;
        }
        else
        {
            // Xuất bớt: đánh dấu các bản sao Available bị lấy ra
            var copies = await _context.BookCopies
                .Where(x => x.BookId == book.Id && !x.IsDeleted && x.Status == BookCopyStatus.Available)
                .OrderBy(x => x.Barcode)
                .Take(Math.Abs(request.Quantity))
                .ToListAsync(cancellationToken);

            foreach (var copy in copies)
            {
                copy.Status = BookCopyStatus.Lost;
                copy.Notes = request.Reason;
                copy.UpdatedAt = DateTime.UtcNow;
                copy.UpdatedBy = operatorId.ToString();
            }

            var removed = copies.Count;
            book.TotalQuantity = Math.Max(0, book.TotalQuantity - removed);
            book.StockForSell = Math.Max(0, book.StockForSell - removed);
        }

        book.UpdatedAt = DateTime.UtcNow;
        book.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Điều chỉnh tồn kho", "Inventory", $"Điều chỉnh tồn kho sách {book.Title}: {request.Quantity}. Lý do: {request.Reason}");

        return ServiceResult<bool>.Success(true, "Điều chỉnh tồn kho thành công.");
    }

    public async Task<ServiceResult<bool>> RestockAsync(RestockRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == request.BookId, cancellationToken);
        if (book is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy sách.");
        }

        if (request.Quantity <= 0)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Số lượng nhập phải lớn hơn 0.");
        }

        var existingCount = await _context.BookCopies.CountAsync(x => x.BookId == book.Id, cancellationToken);
        for (var i = 1; i <= request.Quantity; i++)
        {
            _context.BookCopies.Add(new BookCopy
            {
                Id = Guid.NewGuid(),
                BookId = book.Id,
                Barcode = ServiceHelpers.BuildBarcode(book.Id, existingCount + i),
                ConditionStatus = request.ConditionStatus,
                ShelfCode = request.ShelfCode,
                WarehouseId = request.WarehouseId,
                Status = BookCopyStatus.Available,
                AcquiredDate = DateTime.UtcNow,
                Notes = request.Reason,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = operatorId.ToString()
            });
        }

        book.TotalQuantity += request.Quantity;
        if (request.UnitType == UnitType.Rent)
        {
            book.StockForRent += request.Quantity;
        }
        else
        {
            book.StockForSell += request.Quantity;
        }

        book.UpdatedAt = DateTime.UtcNow;
        book.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Nhập tồn kho", "Inventory", $"Nhập {request.Quantity} bản sao cho sách {book.Title}.");

        return ServiceResult<bool>.Success(true, "Nhập tồn kho thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<WarehouseListItemResponse>>> GetWarehousesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.Warehouses
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Code)
            .Select(x => new WarehouseListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Address = x.Address,
                ProvinceId = x.ProvinceId,
                ProvinceName = x.Province!.Name,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                CopyCount = _context.BookCopies.Count(c => c.WarehouseId == x.Id && !c.IsDeleted),
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<WarehouseListItemResponse>>.Success(items, "Lấy danh sách kho hàng thành công.");
    }

    public async Task<ServiceResult<WarehouseListItemResponse>> CreateWarehouseAsync(CreateWarehouseRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        if (await _context.Warehouses.AnyAsync(x => x.Code == request.Code.Trim() && !x.IsDeleted, cancellationToken))
        {
            return ServiceResult<WarehouseListItemResponse>.Failure(HttpStatusCode.Conflict, "Mã kho đã tồn tại.");
        }

        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Address = request.Address,
            ProvinceId = request.ProvinceId,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạo kho hàng", "Inventory", $"Tạo kho {warehouse.Code}.");

        var result = await _context.Warehouses
            .AsNoTracking()
            .Where(x => x.Id == warehouse.Id)
            .Select(x => new WarehouseListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Address = x.Address,
                ProvinceId = x.ProvinceId,
                ProvinceName = x.Province!.Name,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                CopyCount = 0,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return ServiceResult<WarehouseListItemResponse>.Success(result!, "Tạo kho hàng thành công.");
    }

    public async Task<ServiceResult<WarehouseListItemResponse>> UpdateWarehouseAsync(Guid id, UpdateWarehouseRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (warehouse is null)
        {
            return ServiceResult<WarehouseListItemResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy kho hàng.");
        }

        if (request.Name is not null) warehouse.Name = request.Name.Trim();
        if (request.Address is not null) warehouse.Address = request.Address;
        if (request.ProvinceId.HasValue) warehouse.ProvinceId = request.ProvinceId;
        if (request.IsActive.HasValue) warehouse.IsActive = request.IsActive.Value;

        warehouse.UpdatedAt = DateTime.UtcNow;
        warehouse.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Cập nhật kho hàng", "Inventory", $"Cập nhật kho {warehouse.Code}.");

        var result = await _context.Warehouses
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new WarehouseListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Address = x.Address,
                ProvinceId = x.ProvinceId,
                ProvinceName = x.Province!.Name,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                CopyCount = _context.BookCopies.Count(c => c.WarehouseId == x.Id && !c.IsDeleted),
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return ServiceResult<WarehouseListItemResponse>.Success(result!, "Cập nhật kho hàng thành công.");
    }

    private async Task WriteAuditAsync(Guid? operatorId, string action, string module, string note)
    {
        await _auditLogService.WriteAsync(new AuditLog
        {
            ActorUserId = operatorId,
            Action = action,
            Module = module,
            Note = note,
            EntityType = "Inventory",
            IsSuccess = true
        });
    }

    // ------------------------------------------------------------------ Phiếu nhập kho & kiểm kê (FE: /inventory/*)

    public async Task<ServiceResult<DraftPurchaseOrderResponse>> CreateDraftPurchaseOrderAsync(DraftPurchaseOrderRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.WarehouseId && !x.IsDeleted, cancellationToken);
        if (warehouse is null)
        {
            return ServiceResult<DraftPurchaseOrderResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy kho hàng.");
        }

        var details = request.Details.Where(x => x.Quantity > 0).ToList();
        if (details.Count == 0)
        {
            return ServiceResult<DraftPurchaseOrderResponse>.Failure(HttpStatusCode.BadRequest, "Phiếu nhập kho cần ít nhất một dòng hàng.");
        }

        var bookIds = details.Select(x => x.BookId).Distinct().ToList();
        var validBookIds = await _context.Books
            .AsNoTracking()
            .Where(x => bookIds.Contains(x.Id) && !x.IsDeleted)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var invalid = bookIds.Except(validBookIds).ToList();
        if (invalid.Count > 0)
        {
            return ServiceResult<DraftPurchaseOrderResponse>.Failure(HttpStatusCode.BadRequest, "Có sách không tồn tại trong phiếu nhập kho.");
        }

        var sequence = await _context.PurchaseOrders.CountAsync(cancellationToken) + 1;
        var code = $"PO-{DateTime.UtcNow:yyyyMMdd}-{sequence:D4}";

        var purchaseOrder = new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            Code = code,
            WarehouseId = request.WarehouseId,
            LeadTimeDays = request.LeadTimeDays,
            Notes = request.Notes,
            TotalQuantity = details.Sum(x => x.Quantity),
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        foreach (var detail in details)
        {
            purchaseOrder.Details.Add(new PurchaseOrderDetail
            {
                Id = Guid.NewGuid(),
                BookId = detail.BookId,
                Quantity = detail.Quantity,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = operatorId.ToString()
            });
        }

        _context.PurchaseOrders.Add(purchaseOrder);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Lập phiếu nhập kho", "Inventory", $"Lập phiếu nhập kho nháp {code} với {purchaseOrder.TotalQuantity} bản.");

        return ServiceResult<DraftPurchaseOrderResponse>.Success(new DraftPurchaseOrderResponse
        {
            Id = purchaseOrder.Id,
            Code = purchaseOrder.Code,
            WarehouseId = purchaseOrder.WarehouseId,
            LeadTimeDays = purchaseOrder.LeadTimeDays,
            Notes = purchaseOrder.Notes,
            TotalQuantity = purchaseOrder.TotalQuantity,
            CreatedAt = purchaseOrder.CreatedAt
        }, "Lập phiếu nhập kho thành công.");
    }

    public async Task<ServiceResult<int>> RestockAllAsync(RestockAllRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var quantity = request.QuantityPerBook <= 0 ? 5 : request.QuantityPerBook;
        var threshold = DefaultLowStockThreshold;

        var query = _context.Books.AsNoTracking().Where(x => !x.IsDeleted && x.IsActive);

        if (request.WarehouseId.HasValue)
        {
            var warehouseId = request.WarehouseId.Value;
            query = query.Where(x => _context.BookCopies.Any(c => c.BookId == x.Id && c.WarehouseId == warehouseId && !c.IsDeleted));
        }

        var lowStockBooks = await query
            .Where(x => x.StockForSell <= threshold)
            .OrderBy(x => x.StockForSell)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (lowStockBooks.Count == 0)
        {
            return ServiceResult<int>.Success(0, "Không có sách nào dưới ngưỡng tồn kho.");
        }

        var restocked = 0;

        foreach (var bookId in lowStockBooks)
        {
            var result = await RestockAsync(new RestockRequest
            {
                BookId = bookId,
                WarehouseId = request.WarehouseId,
                UnitType = UnitType.Sell,
                Quantity = quantity,
                ConditionStatus = "Mới",
                Reason = request.Reason ?? "Nhập tự động cho mục tồn kho thấp"
            }, operatorId, cancellationToken);

            if (result.IsSuccess)
            {
                restocked++;
            }
        }

        await WriteAuditAsync(operatorId, "Nhập kho tự động", "Inventory", $"Đề xuất nhập kho cho {restocked}/{lowStockBooks.Count} mục tồn kho thấp.");

        return ServiceResult<int>.Success(restocked, $"Đã tạo đề xuất nhập kho cho {restocked} mục.");
    }

    public async Task<ServiceResult<AuditSheetResponse>> GetAuditSheetAsync(CancellationToken cancellationToken = default)
    {
        var books = await _context.Books
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Title)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Isbn,
                x.TotalQuantity
            })
            .ToListAsync(cancellationToken);

        var rows = new List<AuditSheetRow>(books.Count);

        foreach (var book in books)
        {
            var actual = await _context.BookCopies
                .AsNoTracking()
                .CountAsync(x => x.BookId == book.Id && !x.IsDeleted && x.Status != BookCopyStatus.Lost, cancellationToken);

            rows.Add(new AuditSheetRow
            {
                BookId = book.Id,
                Title = book.Title,
                Isbn = book.Isbn,
                SystemQuantity = book.TotalQuantity,
                ActualQuantity = actual,
                Difference = actual - book.TotalQuantity,
                Note = actual != book.TotalQuantity ? "Chênh lệch cần kiểm tra" : null
            });
        }

        var warehouseName = "Tất cả kho";

        return ServiceResult<AuditSheetResponse>.Success(new AuditSheetResponse
        {
            WarehouseName = warehouseName,
            GeneratedAt = DateTime.UtcNow,
            TotalBooks = rows.Count,
            TotalCopies = rows.Sum(x => x.ActualQuantity),
            Rows = rows
        }, "Lấy phiếu kiểm kê tồn kho thành công.");
    }
}
