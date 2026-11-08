using BookVerse.Core.Common;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.AuditLog;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _context;

    public AuditLogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task WriteAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        auditLog.CreatedAt = auditLog.CreatedAt == default ? DateTime.UtcNow : auditLog.CreatedAt;
        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ServiceResult<PagedResult<AuditLogListItemResponse>>> SearchAsync(AuditLogSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(x =>
                (x.Action != null && x.Action.Contains(keyword)) ||
                (x.ActorUserName != null && x.ActorUserName.Contains(keyword)) ||
                (x.Note != null && x.Note.Contains(keyword)));
        }

        if (request.ActorUserId.HasValue)
        {
            query = query.Where(x => x.ActorUserId == request.ActorUserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Module))
        {
            query = query.Where(x => x.Module == request.Module);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(x => x.EntityType == request.EntityType);
        }

        if (request.IsSuccess.HasValue)
        {
            query = query.Where(x => x.IsSuccess == request.IsSuccess.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= request.ToDate.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ServiceResult<PagedResult<AuditLogListItemResponse>>.Success(new PagedResult<AuditLogListItemResponse>
        {
            Items = items.Select(x => new AuditLogListItemResponse
            {
                Id = x.Id,
                ActorUserId = x.ActorUserId,
                ActorUserName = x.ActorUserName,
                Action = x.Action,
                Module = x.Module,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                Note = x.Note,
                IsSuccess = x.IsSuccess,
                IpAddress = x.IpAddress,
                CreatedAt = x.CreatedAt
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách nhật ký hệ thống thành công.");
    }

    public async Task<ServiceResult<AuditLogDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _context.AuditLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return ServiceResult<AuditLogDetailResponse>.Failure(System.Net.HttpStatusCode.NotFound, "Không tìm thấy bản ghi nhật ký.");
        }

        return ServiceResult<AuditLogDetailResponse>.Success(new AuditLogDetailResponse
        {
            Id = item.Id,
            ActorUserId = item.ActorUserId,
            ActorUserName = item.ActorUserName,
            Action = item.Action,
            Module = item.Module,
            EntityType = item.EntityType,
            EntityId = item.EntityId,
            Note = item.Note,
            IsSuccess = item.IsSuccess,
            IpAddress = item.IpAddress,
            CreatedAt = item.CreatedAt
        }, "Lấy chi tiết nhật ký hệ thống thành công.");
    }
}
