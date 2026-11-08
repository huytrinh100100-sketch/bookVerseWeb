using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Constants;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Coupon;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class CouponService : ICouponService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public CouponService(AppDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<PagedResult<CouponListItemResponse>>> SearchAsync(CouponSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.Coupons.AsNoTracking();

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (request.DiscountType.HasValue) query = query.Where(x => x.DiscountType == request.DiscountType.Value);
        if (request.Audience.HasValue) query = query.Where(x => x.Audience == request.Audience.Value);
        if (request.IsDeleted.HasValue) query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(keyword) || x.Name.ToLower().Contains(keyword));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return ServiceResult<PagedResult<CouponListItemResponse>>.Success(new PagedResult<CouponListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách voucher thành công.");
    }

    public async Task<ServiceResult<CouponDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (coupon is null)
        {
            return ServiceResult<CouponDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy voucher.");
        }

        var campaign = coupon.CampaignId.HasValue
            ? await _context.Campaigns.AsNoTracking().FirstOrDefaultAsync(x => x.Id == coupon.CampaignId.Value, cancellationToken)
            : null;

        return ServiceResult<CouponDetailResponse>.Success(new CouponDetailResponse
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Name = coupon.Name,
            Description = coupon.Description,
            DiscountType = coupon.DiscountType,
            DiscountValue = coupon.DiscountValue,
            MinOrderValue = coupon.MinOrderValue,
            MaxDiscountAmount = coupon.MaxDiscountAmount,
            UsageLimit = coupon.UsageLimit,
            UsedCount = coupon.UsedCount,
            PerUserUsageLimit = coupon.PerUserUsageLimit,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            Audience = coupon.Audience,
            Status = coupon.Status,
            IsDeleted = coupon.IsDeleted,
            CreatedAt = coupon.CreatedAt,
            UpdatedAt = coupon.UpdatedAt,
            RowVersion = coupon.RowVersion,
            CampaignId = coupon.CampaignId,
            CampaignName = campaign?.Name
        }, "Lấy chi tiết voucher thành công.");
    }

    public async Task<ServiceResult<CouponDetailResponse>> CreateAsync(CreateCouponRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        if (await _context.Coupons.AnyAsync(x => x.Code == request.Code.Trim() && !x.IsDeleted, cancellationToken))
        {
            return ServiceResult<CouponDetailResponse>.Failure(HttpStatusCode.Conflict, "Mã voucher đã tồn tại.");
        }

        if (request.EndDate <= request.StartDate)
        {
            return ServiceResult<CouponDetailResponse>.Failure(HttpStatusCode.BadRequest, "Ngày kết thúc phải sau ngày bắt đầu.");
        }

        if (request.DiscountType == DiscountType.Percent && (request.DiscountValue <= 0 || request.DiscountValue > 100))
        {
            return ServiceResult<CouponDetailResponse>.Failure(HttpStatusCode.BadRequest, "Phần trăm giảm giá phải trong khoảng 0-100.");
        }

        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinOrderValue = request.MinOrderValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            UsageLimit = request.UsageLimit,
            UsedCount = 0,
            PerUserUsageLimit = request.PerUserUsageLimit,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Audience = request.Audience,
            Status = request.Status,
            CampaignId = request.CampaignId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạo voucher", "Coupon", $"Tạo voucher {coupon.Code}.");

        return await GetDetailAsync(coupon.Id, cancellationToken);
    }

    public async Task<ServiceResult<CouponDetailResponse>> UpdateAsync(Guid id, UpdateCouponRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (coupon is null)
        {
            return ServiceResult<CouponDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy voucher.");
        }

        if (request.RowVersion is not null && coupon.RowVersion is not null)
        {
            _context.Entry(coupon).Property(x => x.RowVersion).OriginalValue = request.RowVersion;
        }

        if (!string.IsNullOrWhiteSpace(request.Name)) coupon.Name = request.Name.Trim();
        if (request.Description is not null) coupon.Description = request.Description;
        if (request.DiscountType.HasValue) coupon.DiscountType = request.DiscountType.Value;
        if (request.DiscountValue.HasValue) coupon.DiscountValue = request.DiscountValue.Value;
        if (request.MinOrderValue.HasValue) coupon.MinOrderValue = request.MinOrderValue.Value;
        if (request.MaxDiscountAmount.HasValue) coupon.MaxDiscountAmount = request.MaxDiscountAmount.Value;
        if (request.UsageLimit.HasValue) coupon.UsageLimit = request.UsageLimit.Value;
        if (request.PerUserUsageLimit.HasValue) coupon.PerUserUsageLimit = request.PerUserUsageLimit.Value;
        if (request.StartDate.HasValue) coupon.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) coupon.EndDate = request.EndDate.Value;
        if (request.Audience.HasValue) coupon.Audience = request.Audience.Value;
        if (request.Status.HasValue) coupon.Status = request.Status.Value;
        if (request.CampaignId.HasValue) coupon.CampaignId = request.CampaignId;

        coupon.UpdatedAt = DateTime.UtcNow;
        coupon.UpdatedBy = operatorId.ToString();

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<CouponDetailResponse>.Failure(HttpStatusCode.Conflict, "Voucher đã bị thay đổi bởi người khác. Vui lòng tải lại trang.");
        }

        await WriteAuditAsync(operatorId, "Cập nhật voucher", "Coupon", $"Cập nhật voucher {coupon.Code}.");

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (coupon is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy voucher.");
        }

        coupon.IsDeleted = true;
        coupon.Status = CouponStatus.Paused;
        coupon.DeletedAt = DateTime.UtcNow;
        coupon.DeletedBy = operatorId.ToString();
        coupon.UpdatedAt = DateTime.UtcNow;
        coupon.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa mềm voucher", "Coupon", $"Xóa mềm voucher {coupon.Code}.");

        return ServiceResult<bool>.Success(true, "Xóa mềm voucher thành công.");
    }

    public async Task<ServiceResult<bool>> HardDeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (coupon is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy voucher.");
        }

        var used = await _context.Orders.AnyAsync(x => x.CouponId == id, cancellationToken);
        if (used)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Voucher đã được sử dụng, không thể xóa vĩnh viễn.");
        }

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa vĩnh viễn voucher", "Coupon", $"Xóa vĩnh viễn voucher {coupon.Code}.");

        return ServiceResult<bool>.Success(true, "Xóa vĩnh viễn voucher thành công.");
    }

    public async Task<ServiceResult<ValidateCouponResponse>> ValidateAsync(ValidateCouponRequest request, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == request.Code.Trim().ToUpperInvariant() && !x.IsDeleted, cancellationToken);

        if (coupon is null)
        {
            return ServiceResult<ValidateCouponResponse>.Success(new ValidateCouponResponse
            {
                IsValid = false,
                Message = "Mã voucher không tồn tại."
            }, "Mã voucher không hợp lệ.");
        }

        var now = DateTime.UtcNow;

        if (coupon.Status != CouponStatus.Active)
        {
            return ServiceResult<ValidateCouponResponse>.Success(new ValidateCouponResponse { IsValid = false, Message = "Voucher không còn hiệu lực." }, "Voucher không còn hiệu lực.");
        }

        if (now < coupon.StartDate)
        {
            return ServiceResult<ValidateCouponResponse>.Success(new ValidateCouponResponse { IsValid = false, Message = "Voucher chưa tới thời gian sử dụng." }, "Voucher chưa tới thời gian sử dụng.");
        }

        if (now > coupon.EndDate)
        {
            return ServiceResult<ValidateCouponResponse>.Success(new ValidateCouponResponse { IsValid = false, Message = "Voucher đã hết hạn." }, "Voucher đã hết hạn.");
        }

        if (coupon.UsageLimit > 0 && coupon.UsedCount >= coupon.UsageLimit)
        {
            return ServiceResult<ValidateCouponResponse>.Success(new ValidateCouponResponse { IsValid = false, Message = "Voucher đã hết lượt sử dụng." }, "Voucher đã hết lượt sử dụng.");
        }

        if (request.OrderAmount < coupon.MinOrderValue)
        {
            return ServiceResult<ValidateCouponResponse>.Success(new ValidateCouponResponse
            {
                IsValid = false,
                Message = $"Đơn hàng tối thiểu {coupon.MinOrderValue:N0} đ để dùng voucher.",
                CouponId = coupon.Id,
                CouponCode = coupon.Code,
                MinOrderValue = coupon.MinOrderValue
            }, "Chưa đạt giá trị đơn hàng tối thiểu.");
        }

        // Giới hạn sử dụng theo khách hàng
        if (coupon.PerUserUsageLimit > 0)
        {
            var usedByCustomer = await _context.Orders.CountAsync(x => x.CustomerId == request.CustomerId && x.CouponId == coupon.Id && x.OrderStatus != OrderStatus.Cancelled, cancellationToken);
            if (usedByCustomer >= coupon.PerUserUsageLimit)
            {
                return ServiceResult<ValidateCouponResponse>.Success(new ValidateCouponResponse { IsValid = false, Message = "Bạn đã sử dụng voucher này tối đa số lần cho phép." }, "Đã vượt giới hạn sử dụng.");
            }
        }

        var discount = coupon.DiscountType == DiscountType.Percent
            ? ServiceHelpers.RoundMoney(request.OrderAmount * coupon.DiscountValue / 100m)
            : ServiceHelpers.RoundMoney(coupon.DiscountValue);

        if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
        {
            discount = coupon.MaxDiscountAmount.Value;
        }

        discount = Math.Min(discount, request.OrderAmount);

        return ServiceResult<ValidateCouponResponse>.Success(new ValidateCouponResponse
        {
            IsValid = true,
            Message = "Voucher hợp lệ.",
            CouponId = coupon.Id,
            CouponCode = coupon.Code,
            DiscountAmount = discount,
            FinalAmount = ServiceHelpers.RoundMoney(request.OrderAmount - discount),
            MinOrderValue = coupon.MinOrderValue
        }, "Voucher hợp lệ.");
    }

    // ------------------------------------------------------------------ Chiến dịch

    public async Task<ServiceResult<PagedResult<CampaignListItemResponse>>> SearchCampaignsAsync(CampaignSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.Campaigns.AsNoTracking();

        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.IsDeleted.HasValue) query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x => x.Code.ToLower().Contains(keyword) || x.Name.ToLower().Contains(keyword));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CampaignListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                CouponCount = x.Coupons.Count(c => !c.IsDeleted),
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<CampaignListItemResponse>>.Success(new PagedResult<CampaignListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách chiến dịch thành công.");
    }

    public async Task<ServiceResult<CampaignDetailResponse>> GetCampaignAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.Campaigns.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (campaign is null)
        {
            return ServiceResult<CampaignDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy chiến dịch.");
        }

        var coupons = await _context.Coupons
            .AsNoTracking()
            .Where(x => x.CampaignId == id && !x.IsDeleted)
            .OrderBy(x => x.Code)
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

        return ServiceResult<CampaignDetailResponse>.Success(new CampaignDetailResponse
        {
            Id = campaign.Id,
            Code = campaign.Code,
            Name = campaign.Name,
            Description = campaign.Description,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            IsActive = campaign.IsActive,
            IsDeleted = campaign.IsDeleted,
            CreatedAt = campaign.CreatedAt,
            Coupons = coupons
        }, "Lấy chi tiết chiến dịch thành công.");
    }

    public async Task<ServiceResult<CampaignDetailResponse>> CreateCampaignAsync(CreateCampaignRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        if (await _context.Campaigns.AnyAsync(x => x.Code == request.Code.Trim() && !x.IsDeleted, cancellationToken))
        {
            return ServiceResult<CampaignDetailResponse>.Failure(HttpStatusCode.Conflict, "Mã chiến dịch đã tồn tại.");
        }

        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạo chiến dịch", "Campaign", $"Tạo chiến dịch {campaign.Name}.");

        return await GetCampaignAsync(campaign.Id, cancellationToken);
    }

    public async Task<ServiceResult<CampaignDetailResponse>> UpdateCampaignAsync(Guid id, UpdateCampaignRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (campaign is null)
        {
            return ServiceResult<CampaignDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy chiến dịch.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name)) campaign.Name = request.Name.Trim();
        if (request.Description is not null) campaign.Description = request.Description;
        if (request.StartDate.HasValue) campaign.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) campaign.EndDate = request.EndDate.Value;
        if (request.IsActive.HasValue) campaign.IsActive = request.IsActive.Value;

        campaign.UpdatedAt = DateTime.UtcNow;
        campaign.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Cập nhật chiến dịch", "Campaign", $"Cập nhật chiến dịch {campaign.Name}.");

        return await GetCampaignAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> DeleteCampaignAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var campaign = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (campaign is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy chiến dịch.");
        }

        campaign.IsDeleted = true;
        campaign.IsActive = false;
        campaign.DeletedAt = DateTime.UtcNow;
        campaign.DeletedBy = operatorId.ToString();
        campaign.UpdatedAt = DateTime.UtcNow;
        campaign.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa mềm chiến dịch", "Campaign", $"Xóa mềm chiến dịch {campaign.Name}.");

        return ServiceResult<bool>.Success(true, "Xóa mềm chiến dịch thành công.");
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

    // ------------------------------------------------------------------ Tra cứu & tạm dừng (FE: /coupons/*)

    public async Task<ServiceResult<CouponDetailResponse>> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return ServiceResult<CouponDetailResponse>.Failure(HttpStatusCode.BadRequest, "Mã voucher không được để trống.");
        }

        var normalized = code.Trim().ToUpperInvariant();
        var coupon = await _context.Coupons.AsNoTracking().FirstOrDefaultAsync(x => x.Code == normalized, cancellationToken);

        if (coupon is null)
        {
            return ServiceResult<CouponDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy voucher.");
        }

        var campaign = coupon.CampaignId.HasValue
            ? await _context.Campaigns.AsNoTracking().FirstOrDefaultAsync(x => x.Id == coupon.CampaignId.Value, cancellationToken)
            : null;

        return ServiceResult<CouponDetailResponse>.Success(new CouponDetailResponse
        {
            Id = coupon.Id,
            Code = coupon.Code,
            Name = coupon.Name,
            Description = coupon.Description,
            DiscountType = coupon.DiscountType,
            DiscountValue = coupon.DiscountValue,
            MinOrderValue = coupon.MinOrderValue,
            MaxDiscountAmount = coupon.MaxDiscountAmount,
            UsageLimit = coupon.UsageLimit,
            UsedCount = coupon.UsedCount,
            PerUserUsageLimit = coupon.PerUserUsageLimit,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            Audience = coupon.Audience,
            Status = coupon.Status,
            IsDeleted = coupon.IsDeleted,
            CreatedAt = coupon.CreatedAt,
            UpdatedAt = coupon.UpdatedAt,
            RowVersion = coupon.RowVersion,
            CampaignId = coupon.CampaignId,
            CampaignName = campaign?.Name
        }, "Lấy voucher theo mã thành công.");
    }

    public async Task<ServiceResult<CouponDetailResponse>> PauseAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var coupon = await _context.Coupons.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (coupon is null)
        {
            return ServiceResult<CouponDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy voucher.");
        }

        coupon.Status = CouponStatus.Paused;
        coupon.UpdatedAt = DateTime.UtcNow;
        coupon.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạm dừng voucher", "Coupon", $"Tạm dừng voucher {coupon.Code}.");

        return await GetDetailAsync(id, cancellationToken);
    }
}
