using BookVerse.Core.Common;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Permission;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<IReadOnlyCollection<PermissionListItemResponse>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.Permissions
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.GroupCode)
            .ThenBy(x => x.SortOrder)
            .Select(x => new PermissionListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                GroupCode = x.GroupCode,
                GroupName = x.GroupName,
                Description = x.Description,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<PermissionListItemResponse>>.Success(items, "Lấy danh sách quyền thành công.");
    }

    public async Task<ServiceResult<PagedResult<PermissionListItemResponse>>> SearchAsync(CancellationToken cancellationToken = default)
    {
        var items = await GetAllAsync(cancellationToken);
        var all = items.Data ?? (IReadOnlyCollection<PermissionListItemResponse>)[];

        return ServiceResult<PagedResult<PermissionListItemResponse>>.Success(new PagedResult<PermissionListItemResponse>
        {
            Items = all.ToList(),
            Page = 1,
            PageSize = all.Count,
            TotalItems = all.Count,
            TotalPages = all.Count == 0 ? 0 : 1
        }, "Lấy danh sách quyền thành công.");
    }
}
