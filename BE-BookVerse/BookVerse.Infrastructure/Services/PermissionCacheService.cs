using BookVerse.Core.Common;
using BookVerse.Core.Interfaces;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BookVerse.Infrastructure.Services;

public class PermissionCacheService : IPermissionCacheService
{
    private const string CacheKeyPrefix = "perm:user:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public PermissionCacheService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ServiceResult<IReadOnlyCollection<string>>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{userId}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyCollection<string>? cached) && cached is not null)
        {
            return ServiceResult<IReadOnlyCollection<string>>.Success(cached, "Lấy quyền người dùng thành công (cache).");
        }

        var permissions = await (from userRole in _context.UserRoles.AsNoTracking()
                                 join role in _context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                                 join rolePermission in _context.RolePermissions.AsNoTracking() on role.Id equals rolePermission.RoleId
                                 join permission in _context.Permissions.AsNoTracking() on rolePermission.PermissionId equals permission.Id
                                 where userRole.UserId == userId
                                       && role.IsActive && !role.IsDeleted
                                       && permission.IsActive
                                 select permission.Code)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        _cache.Set(cacheKey, (IReadOnlyCollection<string>)permissions, CacheDuration);

        return ServiceResult<IReadOnlyCollection<string>>.Success(permissions, "Lấy quyền người dùng thành công.");
    }

    public void InvalidateUserPermissions(Guid userId)
    {
        _cache.Remove($"{CacheKeyPrefix}{userId}");
    }

    public void InvalidateAll()
    {
        // MemoryCache không hỗ trợ duyệt xoá theo prefix -> dùng compaction khi cần reset toàn bộ.
        // Gọi InvalidateUserPermissions cho từng user khi thay đổi vai trò/quyền.
    }
}
