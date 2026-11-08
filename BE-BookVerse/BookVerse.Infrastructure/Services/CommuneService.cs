using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Geography;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class CommuneService : ICommuneService
{
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public CommuneService(AppDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<PagedResult<CommuneListItemResponse>>> SearchAsync(CommuneSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 50 : Math.Min(request.PageSize, 200);

        var query = from commune in _context.Communes.AsNoTracking()
                    join province in _context.Provinces.AsNoTracking() on commune.ProvinceId equals province.Id
                    select new CommuneListItemResponse
                    {
                        Id = commune.Id,
                        ProvinceId = commune.ProvinceId,
                        ProvinceName = province.Name,
                        Code = commune.Code,
                        Name = commune.Name,
                        Kind = commune.Kind,
                        IsActive = commune.IsActive,
                        IsDeleted = commune.IsDeleted
                    };

        if (request.ProvinceId.HasValue) query = query.Where(x => x.ProvinceId == request.ProvinceId.Value);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.IsDeleted.HasValue) query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(keyword) || x.Code.ToLower().Contains(keyword));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.ProvinceName)
            .ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<CommuneListItemResponse>>.Success(new PagedResult<CommuneListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách xã/phường thành công.");
    }

    public async Task<ServiceResult<IReadOnlyCollection<CommuneListItemResponse>>> GetByProvinceAsync(Guid provinceId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = from commune in _context.Communes.AsNoTracking()
                    join province in _context.Provinces.AsNoTracking() on commune.ProvinceId equals province.Id
                    where commune.ProvinceId == provinceId
                    select new CommuneListItemResponse
                    {
                        Id = commune.Id,
                        ProvinceId = commune.ProvinceId,
                        ProvinceName = province.Name,
                        Code = commune.Code,
                        Name = commune.Name,
                        Kind = commune.Kind,
                        IsActive = commune.IsActive,
                        IsDeleted = commune.IsDeleted
                    };

        if (activeOnly)
        {
            query = query.Where(x => x.IsActive && !x.IsDeleted);
        }

        var items = await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<CommuneListItemResponse>>.Success(items, "Lấy danh sách xã/phường thành công.");
    }

    public async Task<ServiceResult<CommuneDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var commune = await _context.Communes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (commune is null)
        {
            return ServiceResult<CommuneDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy xã/phường.");
        }

        var province = await _context.Provinces.AsNoTracking().FirstOrDefaultAsync(x => x.Id == commune.ProvinceId, cancellationToken);

        return ServiceResult<CommuneDetailResponse>.Success(new CommuneDetailResponse
        {
            Id = commune.Id,
            ProvinceId = commune.ProvinceId,
            ProvinceName = province?.Name,
            Code = commune.Code,
            Name = commune.Name,
            Kind = commune.Kind,
            IsActive = commune.IsActive,
            IsDeleted = commune.IsDeleted,
            CreatedAt = commune.CreatedAt
        }, "Lấy chi tiết xã/phường thành công.");
    }

    public async Task<ServiceResult<CommuneDetailResponse>> CreateAsync(CreateCommuneRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        if (!await _context.Provinces.AnyAsync(x => x.Id == request.ProvinceId, cancellationToken))
        {
            return ServiceResult<CommuneDetailResponse>.Failure(HttpStatusCode.BadRequest, "Tỉnh/thành phố không tồn tại.");
        }

        if (await _context.Communes.AnyAsync(x => x.Code == request.Code.Trim(), cancellationToken))
        {
            return ServiceResult<CommuneDetailResponse>.Failure(HttpStatusCode.Conflict, "Mã xã/phường đã tồn tại.");
        }

        var commune = new Commune
        {
            Id = Guid.NewGuid(),
            ProvinceId = request.ProvinceId,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Kind = request.Kind,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        _context.Communes.Add(commune);
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Tạo xã/phường", "Commune", $"Tạo xã/phường {commune.Name}.");

        return await GetDetailAsync(commune.Id, cancellationToken);
    }

    public async Task<ServiceResult<CommuneDetailResponse>> UpdateAsync(Guid id, UpdateCommuneRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var commune = await _context.Communes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (commune is null)
        {
            return ServiceResult<CommuneDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy xã/phường.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name)) commune.Name = request.Name.Trim();
        if (request.Kind is not null) commune.Kind = request.Kind;
        if (request.IsActive.HasValue) commune.IsActive = request.IsActive.Value;

        commune.UpdatedAt = DateTime.UtcNow;
        commune.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Cập nhật xã/phường", "Commune", $"Cập nhật xã/phường {commune.Name}.");

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var commune = await _context.Communes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (commune is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy xã/phường.");
        }

        commune.IsDeleted = true;
        commune.IsActive = false;
        commune.DeletedAt = DateTime.UtcNow;
        commune.DeletedBy = operatorId.ToString();
        commune.UpdatedAt = DateTime.UtcNow;
        commune.UpdatedBy = operatorId.ToString();
        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Xóa mềm xã/phường", "Commune", $"Xóa mềm xã/phường {commune.Name}.");

        return ServiceResult<bool>.Success(true, "Xóa mềm xã/phường thành công.");
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
