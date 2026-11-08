using BookVerse.Core.Common;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.HomeProfile;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class HomeService : IHomeService
{
    private readonly AppDbContext _context;

    public HomeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<HomeResponse>> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return ServiceResult<HomeResponse>.Failure(System.Net.HttpStatusCode.NotFound, "Không tìm thấy người dùng.");
        }

        var roles = await (from userRole in _context.UserRoles.AsNoTracking()
                           join role in _context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                           where userRole.UserId == userId && role.IsActive && !role.IsDeleted
                           select role.Name!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var permissionCount = await (from userRole in _context.UserRoles.AsNoTracking()
                                     join role in _context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                                     join rolePermission in _context.RolePermissions.AsNoTracking() on role.Id equals rolePermission.RoleId
                                     join permission in _context.Permissions.AsNoTracking() on rolePermission.PermissionId equals permission.Id
                                     where userRole.UserId == userId && permission.IsActive
                                     select permission.Id)
            .Distinct()
            .CountAsync(cancellationToken);

        return ServiceResult<HomeResponse>.Success(new HomeResponse
        {
            UserName = user.UserName ?? string.Empty,
            FullName = user.FullName,
            Roles = roles,
            PermissionCount = permissionCount,
            LastLoginAt = user.LastLoginAt
        }, "Lấy thông tin trang chủ thành công.");
    }
}
