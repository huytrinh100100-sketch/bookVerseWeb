using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Geography;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class ProvinceService : IProvinceService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public ProvinceService(AppDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<PagedResult<ProvinceListItemResponse>>> SearchAsync(ProvinceSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 50 : Math.Min(request.PageSize, 200);

        var query = _context.Provinces.AsNoTracking();

        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.IsDeleted.HasValue) query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(keyword) || x.Code.ToLower().Contains(keyword));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProvinceListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Kind = x.Kind,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                CommuneCount = x.Communes.Count(c => !c.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<ProvinceListItemResponse>>.Success(new PagedResult<ProvinceListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách tỉnh/thành phố thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<ProvinceListItemResponse>>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Provinces.AsNoTracking();
        if (activeOnly)
        {
            query = query.Where(x => x.IsActive && !x.IsDeleted);
        }

        var items = await query
            .OrderBy(x => x.Name)
            .Select(x => new ProvinceListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Kind = x.Kind,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                CommuneCount = x.Communes.Count(c => !c.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<ProvinceListItemResponse>>.Success(items, "Lấy danh mục tỉnh/thành phố thành công.");
    }

    public async Task<ServiceResult<ProvinceDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var province = await _context.Provinces.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (province is null)
        {
            return ServiceResult<ProvinceDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tỉnh/thành phố.");
        }

        return ServiceResult<ProvinceDetailResponse>.Success(new ProvinceDetailResponse
        {
            Id = province.Id,
            Code = province.Code,
            Name = province.Name,
            Kind = province.Kind,
            IsActive = province.IsActive,
            IsDeleted = province.IsDeleted,
            CreatedAt = province.CreatedAt
        }, "Lấy chi tiết tỉnh/thành phố thành công.");
    }

    public async Task<ServiceResult<ProvinceDetailResponse>> CreateAsync(CreateProvinceRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        if (await _context.Provinces.AnyAsync(x => x.Code == request.Code.Trim(), cancellationToken))
        {
            return ServiceResult<ProvinceDetailResponse>.Failure(HttpStatusCode.Conflict, "Mã tỉnh/thành phố đã tồn tại.");
        }

        var province = new Province
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Kind = request.Kind,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        _context.Provinces.Add(province);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạo tỉnh/thành phố", "Province", $"Tạo tỉnh/thành phố {province.Name}.");

        return await GetDetailAsync(province.Id, cancellationToken);
    }

    public async Task<ServiceResult<ProvinceDetailResponse>> UpdateAsync(Guid id, UpdateProvinceRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var province = await _context.Provinces.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (province is null)
        {
            return ServiceResult<ProvinceDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tỉnh/thành phố.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name)) province.Name = request.Name.Trim();
        if (request.Kind is not null) province.Kind = request.Kind;
        if (request.IsActive.HasValue) province.IsActive = request.IsActive.Value;

        province.UpdatedAt = DateTime.UtcNow;
        province.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Cập nhật tỉnh/thành phố", "Province", $"Cập nhật tỉnh/thành phố {province.Name}.");

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var province = await _context.Provinces.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (province is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tỉnh/thành phố.");
        }

        province.IsDeleted = true;
        province.IsActive = false;
        province.DeletedAt = DateTime.UtcNow;
        province.DeletedBy = operatorId.ToString();
        province.UpdatedAt = DateTime.UtcNow;
        province.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa mềm tỉnh/thành phố", "Province", $"Xóa mềm tỉnh/thành phố {province.Name}.");

        return ServiceResult<bool>.Success(true, "Xóa mềm tỉnh/thành phố thành công.");
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
}
