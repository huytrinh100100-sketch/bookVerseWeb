using BookVerse.Core.Common;
using BookVerse.Core.Entities;
using BookVerse.Core.Models.AuditLog;

namespace BookVerse.Core.Interfaces;

public interface IAuditLogService
{
    Task WriteAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<ServiceResult<PagedResult<AuditLogListItemResponse>>> SearchAsync(AuditLogSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<AuditLogDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
}
