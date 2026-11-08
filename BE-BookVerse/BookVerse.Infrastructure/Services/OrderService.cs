using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Constants;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Order;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public OrderService(AppDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<PagedResult<OrderListItemResponse>>> SearchAsync(OrderSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = from order in _context.Orders.AsNoTracking()
                    join customer in _context.Users.AsNoTracking() on order.CustomerId equals customer.Id
                    select new OrderListItemResponse
                    {
                        Id = order.Id,
                        Code = order.Code,
                        CustomerId = order.CustomerId,
                        CustomerName = customer.FullName,
                        CustomerPhone = customer.PhoneNumber ?? string.Empty,
                        OrderDate = order.OrderDate,
                        OrderStatus = order.OrderStatus,
                        PaymentMethod = order.PaymentMethod,
                        TotalAmount = order.TotalAmount,
                        DiscountAmount = order.DiscountAmount,
                        FinalAmount = order.FinalAmount,
                        IsPaid = order.IsPaid,
                        CouponCode = order.CouponCode,
                        TotalItems = order.Details.Count(d => !d.IsDeleted)
                    };

        query = query.Where(x => !_context.Orders.Any(o => o.Id == x.Id && o.IsDeleted));

        if (request.CustomerId.HasValue) query = query.Where(x => x.CustomerId == request.CustomerId.Value);
        if (request.StaffId.HasValue) query = query.Where(x => _context.Orders.Any(o => o.Id == x.Id && o.StaffId == request.StaffId.Value));
        if (request.OrderStatus.HasValue) query = query.Where(x => x.OrderStatus == request.OrderStatus.Value);
        if (request.PaymentMethod.HasValue) query = query.Where(x => x.PaymentMethod == request.PaymentMethod.Value);
        if (request.IsPaid.HasValue) query = query.Where(x => x.IsPaid == request.IsPaid.Value);
        if (request.FromDate.HasValue) query = query.Where(x => x.OrderDate >= request.FromDate.Value);
        if (request.ToDate.HasValue) query = query.Where(x => x.OrderDate <= request.ToDate.Value);
        if (request.MinAmount.HasValue) query = query.Where(x => x.FinalAmount >= request.MinAmount.Value);
        if (request.MaxAmount.HasValue) query = query.Where(x => x.FinalAmount <= request.MaxAmount.Value);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(keyword) ||
                x.CustomerName.ToLower().Contains(keyword) ||
                (x.CustomerPhone != null && x.CustomerPhone.Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        query = (request.SortBy?.ToLowerInvariant()) switch
        {
            "amount" => request.SortDescending ? query.OrderByDescending(x => x.FinalAmount) : query.OrderBy(x => x.FinalAmount),
            "customer" => request.SortDescending ? query.OrderByDescending(x => x.CustomerName) : query.OrderBy(x => x.CustomerName),
            "status" => request.SortDescending ? query.OrderByDescending(x => x.OrderStatus) : query.OrderBy(x => x.OrderStatus),
            _ => request.SortDescending ? query.OrderByDescending(x => x.OrderDate) : query.OrderBy(x => x.OrderDate)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<OrderListItemResponse>>.Success(new PagedResult<OrderListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách đơn hàng thành công.");
    }

    public async Task<ServiceResult<OrderDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null)
        {
            return ServiceResult<OrderDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đơn hàng.");
        }

        var customer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == order.CustomerId, cancellationToken);
        var staff = order.StaffId.HasValue
            ? await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == order.StaffId.Value, cancellationToken)
            : null;
        var province = order.ProvinceId.HasValue
            ? await _context.Provinces.AsNoTracking().FirstOrDefaultAsync(x => x.Id == order.ProvinceId.Value, cancellationToken)
            : null;
        var commune = order.CommuneId.HasValue
            ? await _context.Communes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == order.CommuneId.Value, cancellationToken)
            : null;

        var details = await _context.OrderDetails
            .AsNoTracking()
            .Where(x => x.OrderId == id && !x.IsDeleted)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new OrderDetailItemResponse
            {
                Id = x.Id,
                BookId = x.BookId,
                BookTitle = x.BookTitle ?? x.Book.Title,
                BookSlug = x.Book.Slug,
                CoverImageUrl = x.CoverImageUrl,
                AuthorName = x.AuthorName,
                UnitType = x.UnitType,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                DiscountAmount = x.DiscountAmount,
                LineTotal = x.LineTotal,
                DueDate = x.DueDate,
                ReturnedDate = x.ReturnedDate
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<OrderDetailResponse>.Success(new OrderDetailResponse
        {
            Id = order.Id,
            Code = order.Code,
            CustomerId = order.CustomerId,
            CustomerName = customer?.FullName ?? string.Empty,
            CustomerEmail = customer?.Email ?? string.Empty,
            CustomerPhone = customer?.PhoneNumber ?? string.Empty,
            StaffId = order.StaffId,
            StaffName = staff?.FullName,
            CouponCode = order.CouponCode,
            OrderDate = order.OrderDate,
            OrderStatus = order.OrderStatus,
            PaymentMethod = order.PaymentMethod,
            TotalAmount = order.TotalAmount,
            ShippingAmount = order.ShippingAmount,
            DiscountAmount = order.DiscountAmount,
            FinalAmount = order.FinalAmount,
            IsPaid = order.IsPaid,
            PaidAt = order.PaidAt,
            CancelledAt = order.CancelledAt,
            CancelReason = order.CancelReason,
            ShippingAddress = order.ShippingAddress,
            ProvinceName = province?.Name,
            CommuneName = commune?.Name,
            TrackingCode = order.TrackingCode,
            CustomerNote = order.CustomerNote,
            StaffNote = order.StaffNote,
            IsDeleted = order.IsDeleted,
            CreatedAt = order.CreatedAt,
            RowVersion = order.RowVersion,
            Details = details
        }, "Lấy chi tiết đơn hàng thành công.");
    }

    public async Task<ServiceResult<OrderDetailResponse>> CreateAsync(CreateOrderRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        if (request.Details.Count == 0)
        {
            return ServiceResult<OrderDetailResponse>.Failure(HttpStatusCode.BadRequest, "Đơn hàng phải có ít nhất một sản phẩm.");
        }

        var customer = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.CustomerId && !x.IsDeleted, cancellationToken);
        if (customer is null)
        {
            return ServiceResult<OrderDetailResponse>.Failure(HttpStatusCode.BadRequest, "Khách hàng không tồn tại.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        // Lấy sách và kiểm tra tồn kho
        var bookIds = request.Details.Select(x => x.BookId).Distinct().ToList();
        var books = await _context.Books.Where(x => bookIds.Contains(x.Id)).ToListAsync(cancellationToken);

        if (books.Count != bookIds.Count)
        {
            return ServiceResult<OrderDetailResponse>.Failure(HttpStatusCode.BadRequest, "Có sản phẩm không tồn tại trong hệ thống.");
        }

        var now = DateTime.UtcNow;
        var sequence = await _context.Orders.CountAsync(cancellationToken) + 1;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Code = ServiceHelpers.BuildOrderCode(now, sequence),
            CustomerId = request.CustomerId,
            StaffId = request.StaffId,
            OrderDate = now,
            OrderStatus = OrderStatus.Pending,
            PaymentMethod = request.PaymentMethod,
            ShippingAddress = request.ShippingAddress,
            ProvinceId = request.ProvinceId,
            CommuneId = request.CommuneId,
            CustomerNote = request.CustomerNote,
            StaffNote = request.StaffNote,
            CreatedAt = now,
            CreatedBy = operatorId.ToString()
        };

        decimal totalAmount = 0;

        foreach (var line in request.Details)
        {
            var book = books.First(x => x.Id == line.BookId);
            var unitPrice = line.UnitType == UnitType.Rent ? book.PriceForRent : book.PriceForSell;

            if (line.Quantity <= 0)
            {
                return ServiceResult<OrderDetailResponse>.Failure(HttpStatusCode.BadRequest, "Số lượng phải lớn hơn 0.");
            }

            var available = line.UnitType == UnitType.Rent ? book.StockForRent : book.StockForSell;
            if (available < line.Quantity)
            {
                return ServiceResult<OrderDetailResponse>.Failure(
                    HttpStatusCode.BadRequest,
                    $"Sách '{book.Title}' không đủ tồn kho (còn {available}, yêu cầu {line.Quantity}).");
            }

            var lineTotal = ServiceHelpers.RoundMoney(unitPrice * line.Quantity);
            totalAmount += lineTotal;

            _context.OrderDetails.Add(new OrderDetail
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                BookId = book.Id,
                UnitType = line.UnitType,
                Quantity = line.Quantity,
                UnitPrice = unitPrice,
                DiscountAmount = 0,
                LineTotal = lineTotal,
                BookTitle = book.Title,
                IsbnSnapshot = book.Isbn,
                AuthorName = book.Author?.Name,
                PublisherName = book.Publisher?.Name,
                CoverImageUrl = book.CoverImageUrl,
                DueDate = line.UnitType == UnitType.Rent ? (line.DueDate ?? now.AddDays(30)) : null,
                CreatedAt = now,
                CreatedBy = operatorId.ToString()
            });

            // Trừ tồn kho
            if (line.UnitType == UnitType.Rent)
            {
                book.StockForRent -= line.Quantity;
            }
            else
            {
                book.StockForSell -= line.Quantity;
            }

            book.SoldCount += line.Quantity;
            book.UpdatedAt = now;
            book.UpdatedBy = operatorId.ToString();

            // Gán bản sao vật lý cho đơn mượn
            if (line.UnitType == UnitType.Rent)
            {
                var copies = await _context.BookCopies
                    .Where(x => x.BookId == book.Id && !x.IsDeleted && x.Status == BookCopyStatus.Available)
                    .OrderBy(x => x.Barcode)
                    .Take(line.Quantity)
                    .ToListAsync(cancellationToken);

                foreach (var copy in copies)
                {
                    copy.Status = BookCopyStatus.Lent;
                    copy.CurrentOrderId = order.Id;
                    copy.DueDate = line.DueDate ?? now.AddDays(30);
                    copy.UpdatedAt = now;
                    copy.UpdatedBy = operatorId.ToString();
                }
            }
        }

        totalAmount = ServiceHelpers.RoundMoney(totalAmount);

        // Áp dụng voucher
        var discountAmount = 0m;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(x => x.Code == request.CouponCode.Trim() && !x.IsDeleted, cancellationToken);
            if (coupon is not null && coupon.Status == CouponStatus.Active && coupon.StartDate <= now && coupon.EndDate >= now && coupon.UsedCount < coupon.UsageLimit)
            {
                discountAmount = coupon.DiscountType == DiscountType.Percent
                    ? ServiceHelpers.RoundMoney(totalAmount * coupon.DiscountValue / 100m)
                    : ServiceHelpers.RoundMoney(coupon.DiscountValue);

                if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
                {
                    discountAmount = coupon.MaxDiscountAmount.Value;
                }

                discountAmount = Math.Min(discountAmount, totalAmount);

                coupon.UsedCount++;
                order.CouponId = coupon.Id;
                order.CouponCode = coupon.Code;
            }
        }

        var shippingAmount = totalAmount - discountAmount >= 500000m ? 0m : 30000m;

        order.TotalAmount = totalAmount;
        order.DiscountAmount = discountAmount;
        order.ShippingAmount = shippingAmount;
        order.FinalAmount = ServiceHelpers.RoundMoney(totalAmount - discountAmount + shippingAmount);

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        // Lịch sử trạng thái
        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            FromStatus = OrderStatus.Pending,
            ToStatus = OrderStatus.Pending,
            Note = "Tạo đơn hàng.",
            ChangedBy = operatorId,
            ChangedAt = now
        });

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạo đơn hàng", "Order", $"Tạo đơn hàng {order.Code} - {order.FinalAmount:N0} đ.");

        return await GetDetailAsync(order.Id, cancellationToken);
    }

    public async Task<ServiceResult<OrderDetailResponse>> UpdateAsync(Guid id, UpdateOrderRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null)
        {
            return ServiceResult<OrderDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đơn hàng.");
        }

        if (request.RowVersion is not null && order.RowVersion is not null)
        {
            _context.Entry(order).Property(x => x.RowVersion).OriginalValue = request.RowVersion;
        }

        if (request.StaffId.HasValue) order.StaffId = request.StaffId;
        if (request.PaymentMethod.HasValue) order.PaymentMethod = request.PaymentMethod.Value;
        if (request.ShippingAddress is not null) order.ShippingAddress = request.ShippingAddress;
        if (request.ProvinceId.HasValue) order.ProvinceId = request.ProvinceId;
        if (request.CommuneId.HasValue) order.CommuneId = request.CommuneId;
        if (request.TrackingCode is not null) order.TrackingCode = request.TrackingCode;
        if (request.CustomerNote is not null) order.CustomerNote = request.CustomerNote;
        if (request.StaffNote is not null) order.StaffNote = request.StaffNote;

        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = operatorId.ToString();

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<OrderDetailResponse>.Failure(HttpStatusCode.Conflict, "Đơn hàng đã bị thay đổi bởi người khác. Vui lòng tải lại trang.");
        }

        await WriteAuditAsync(operatorId, "Cập nhật đơn hàng", "Order", $"Cập nhật đơn hàng {order.Code}.");

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<OrderDetailResponse>> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null)
        {
            return ServiceResult<OrderDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đơn hàng.");
        }

        if (request.RowVersion is not null && order.RowVersion is not null)
        {
            _context.Entry(order).Property(x => x.RowVersion).OriginalValue = request.RowVersion;
        }

        if (order.OrderStatus == request.OrderStatus)
        {
            return ServiceResult<OrderDetailResponse>.Failure(HttpStatusCode.BadRequest, "Đơn hàng đã ở trạng thái này.");
        }

        if (!IsValidTransition(order.OrderStatus, request.OrderStatus))
        {
            return ServiceResult<OrderDetailResponse>.Failure(
                HttpStatusCode.BadRequest,
                $"Không thể chuyển trạng thái từ {order.OrderStatus} sang {request.OrderStatus}.");
        }

        var fromStatus = order.OrderStatus;
        order.OrderStatus = request.OrderStatus;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = operatorId.ToString();

        // Trả lại tồn kho khi chuyển sang Cancelled/Refunded
        if (request.OrderStatus is OrderStatus.Cancelled or OrderStatus.Refunded)
        {
            await RestoreStockAsync(order.Id, operatorId, cancellationToken);
        }

        // Hoàn tất đơn: cập nhật hạng VIP, điểm thưởng
        if (request.OrderStatus == OrderStatus.Completed)
        {
            await ApplyCustomerRewardAsync(order, cancellationToken);
        }

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            FromStatus = fromStatus,
            ToStatus = request.OrderStatus,
            Note = request.Reason,
            ChangedBy = operatorId,
            ChangedAt = DateTime.UtcNow
        });

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<OrderDetailResponse>.Failure(HttpStatusCode.Conflict, "Đơn hàng đã bị thay đổi bởi người khác. Vui lòng tải lại trang.");
        }

        await WriteAuditAsync(operatorId, "Đổi trạng thái đơn hàng", "Order", $"Đơn {order.Code}: {fromStatus} -> {request.OrderStatus}.");

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> CancelAsync(Guid id, CancelOrderRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đơn hàng.");
        }

        if (order.OrderStatus is OrderStatus.Completed or OrderStatus.Cancelled)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Không thể hủy đơn hàng đã hoàn tất hoặc đã hủy.");
        }

        var fromStatus = order.OrderStatus;
        order.OrderStatus = OrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancelledBy = operatorId;
        order.CancelReason = request.Reason;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = operatorId.ToString();

        await RestoreStockAsync(id, operatorId, cancellationToken);

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            FromStatus = fromStatus,
            ToStatus = OrderStatus.Cancelled,
            Note = request.Reason,
            ChangedBy = operatorId,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Hủy đơn hàng", "Order", $"Hủy đơn {order.Code}. Lý do: {request.Reason}");

        return ServiceResult<bool>.Success(true, "Hủy đơn hàng thành công.");
    }

    public async Task<ServiceResult<bool>> RefundAsync(Guid id, RefundOrderRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đơn hàng.");
        }

        if (!order.IsPaid)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Đơn hàng chưa thanh toán, không thể hoàn tiền.");
        }

        var fromStatus = order.OrderStatus;
        order.OrderStatus = OrderStatus.Refunded;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = operatorId.ToString();

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            FromStatus = fromStatus,
            ToStatus = OrderStatus.Refunded,
            Note = request.Reason,
            ChangedBy = operatorId,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Hoàn tiền", "Order", $"Hoàn tiền đơn {order.Code} - {request.Amount:N0} đ. Lý do: {request.Reason}");

        return ServiceResult<bool>.Success(true, "Hoàn tiền thành công.");
    }

    public async Task<ServiceResult<bool>> MarkPaidAsync(Guid id, MarkPaidRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy đơn hàng.");
        }

        if (order.IsPaid)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Đơn hàng đã được thanh toán.");
        }

        order.IsPaid = true;
        order.PaidAt = DateTime.UtcNow;
        order.PaymentMethod = request.PaymentMethod;
        if (order.OrderStatus == OrderStatus.Pending)
        {
            order.OrderStatus = OrderStatus.Paid;
        }

        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = operatorId.ToString();

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            FromStatus = order.OrderStatus,
            ToStatus = order.OrderStatus,
            Note = $"Đánh dấu đã thanh toán. Mã giao dịch: {request.TransactionCode}",
            ChangedBy = operatorId,
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Thanh toán đơn hàng", "Order", $"Đơn {order.Code} đã thanh toán.");

        return ServiceResult<bool>.Success(true, "Đánh dấu đã thanh toán thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<OrderStatusHistoryResponse>>> GetHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var items = await _context.OrderStatusHistories
            .AsNoTracking()
            .Where(x => x.OrderId == id)
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

        return ServiceResult<IReadOnlyCollection<OrderStatusHistoryResponse>>.Success(items, "Lấy lịch sử đơn hàng thành công.");
    }

    // ------------------------------------------------------------------ Helpers

    private static bool IsValidTransition(OrderStatus from, OrderStatus to)
    {
        return (from, to) switch
        {
            (OrderStatus.Pending, OrderStatus.Paid) => true,
            (OrderStatus.Pending, OrderStatus.Processing) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Paid, OrderStatus.Processing) => true,
            (OrderStatus.Paid, OrderStatus.Shipped) => true,
            (OrderStatus.Paid, OrderStatus.Cancelled) => true,
            (OrderStatus.Paid, OrderStatus.Refunded) => true,
            (OrderStatus.Processing, OrderStatus.Shipped) => true,
            (OrderStatus.Processing, OrderStatus.Cancelled) => true,
            (OrderStatus.Shipped, OrderStatus.Completed) => true,
            (OrderStatus.Shipped, OrderStatus.Refunded) => true,
            (OrderStatus.Completed, OrderStatus.Refunded) => true,
            _ => false
        };
    }

    private async Task RestoreStockAsync(Guid orderId, Guid operatorId, CancellationToken cancellationToken)
    {
        var details = await _context.OrderDetails.Where(x => x.OrderId == orderId && !x.IsDeleted).ToListAsync(cancellationToken);
        var bookIds = details.Select(x => x.BookId).Distinct().ToList();
        var books = await _context.Books.Where(x => bookIds.Contains(x.Id)).ToListAsync(cancellationToken);

        foreach (var detail in details)
        {
            var book = books.FirstOrDefault(x => x.Id == detail.BookId);
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
            book.UpdatedAt = DateTime.UtcNow;
            book.UpdatedBy = operatorId.ToString();
        }

        // Trả bản sao vật lý
        var copies = await _context.BookCopies.Where(x => x.CurrentOrderId == orderId).ToListAsync(cancellationToken);
        foreach (var copy in copies)
        {
            copy.Status = BookCopyStatus.Available;
            copy.CurrentOrderId = null;
            copy.ReturnedDate = DateTime.UtcNow;
            copy.UpdatedAt = DateTime.UtcNow;
            copy.UpdatedBy = operatorId.ToString();
        }
    }

    private async Task ApplyCustomerRewardAsync(Order order, CancellationToken cancellationToken)
    {
        var customer = await _context.Users.FirstOrDefaultAsync(x => x.Id == order.CustomerId, cancellationToken);
        if (customer is null)
        {
            return;
        }

        customer.LifetimeSpend += order.FinalAmount;
        customer.RewardPoints += (int)(order.FinalAmount / 10000m);

        customer.VipTier = customer.LifetimeSpend switch
        {
            >= 5000000m => VipTier.Platinum,
            >= 2000000m => VipTier.Gold,
            >= 500000m => VipTier.Silver,
            _ => VipTier.Standard
        };
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

    // ------------------------------------------------------------------ Xuất danh sách đơn hàng (FE: GET /orders/export)

    public async Task<ServiceResult<OrderExportResult>> ExportAsync(OrderSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = from order in _context.Orders.AsNoTracking()
                    join customer in _context.Users.AsNoTracking() on order.CustomerId equals customer.Id
                    where !order.IsDeleted
                    select new
                    {
                        order.Id,
                        order.Code,
                        order.OrderDate,
                        CustomerName = customer.FullName,
                        CustomerPhone = customer.PhoneNumber,
                        order.OrderStatus,
                        order.PaymentMethod,
                        order.IsPaid,
                        order.TotalAmount,
                        order.DiscountAmount,
                        order.ShippingAmount,
                        order.FinalAmount,
                        order.CouponCode,
                        TotalItems = order.Details.Count(d => !d.IsDeleted)
                    };

        if (request.Keyword is not null)
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Code.ToLower().Contains(keyword) ||
                x.CustomerName.ToLower().Contains(keyword) ||
                (x.CustomerPhone != null && x.CustomerPhone.Contains(keyword)));
        }

        if (request.OrderStatus.HasValue) query = query.Where(x => x.OrderStatus == request.OrderStatus.Value);
        if (request.PaymentMethod.HasValue) query = query.Where(x => x.PaymentMethod == request.PaymentMethod.Value);
        if (request.IsPaid.HasValue) query = query.Where(x => x.IsPaid == request.IsPaid.Value);
        if (request.CustomerId.HasValue) query = query.Where(x => x.Id != null && _context.Orders.Any(o => o.Id == x.Id && o.CustomerId == request.CustomerId.Value));
        if (request.FromDate.HasValue) query = query.Where(x => x.OrderDate >= request.FromDate.Value);
        if (request.ToDate.HasValue) query = query.Where(x => x.OrderDate <= request.ToDate.Value);
        if (request.MinAmount.HasValue) query = query.Where(x => x.FinalAmount >= request.MinAmount.Value);
        if (request.MaxAmount.HasValue) query = query.Where(x => x.FinalAmount <= request.MaxAmount.Value);

        var rows = await query
            .OrderByDescending(x => x.OrderDate)
            .Take(5000)
            .ToListAsync(cancellationToken);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Ma don,Ngay dat hang,Khach hang,So dien thoai,Trang thai,Phuong thuc thanh toan,Da thanh toan,Tong tien,Giam gia,Phi van chuyen,Thanh toan,Voucher,So san pham");

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",",
                EscapeCsv(row.Code),
                row.OrderDate.ToString("yyyy-MM-dd HH:mm"),
                EscapeCsv(row.CustomerName),
                EscapeCsv(row.CustomerPhone ?? string.Empty),
                row.OrderStatus.ToString(),
                row.PaymentMethod.ToString(),
                row.IsPaid ? "Co" : "Khong",
                row.TotalAmount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                row.DiscountAmount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                row.ShippingAmount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                row.FinalAmount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                EscapeCsv(row.CouponCode ?? string.Empty),
                row.TotalItems.ToString()
            ));
        }

        var fileName = $"don-hang-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return ServiceResult<OrderExportResult>.Success(new OrderExportResult
        {
            FileName = fileName,
            ContentType = "text/csv; charset=utf-8",
            Content = sb.ToString(),
            TotalRows = rows.Count,
            ExportedAt = DateTime.UtcNow
        }, "Xuất danh sách đơn hàng thành công.");
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var quote = (char)34;
        var sanitized = value.Replace(quote.ToString(), string.Concat(quote, quote));

        var needsQuote = sanitized.Contains(',')
            || sanitized.Contains(quote)
            || sanitized.Contains('\n')
            || sanitized.Contains('\r');

        return needsQuote
            ? string.Concat(quote, sanitized, quote)
            : sanitized;
    }
}
