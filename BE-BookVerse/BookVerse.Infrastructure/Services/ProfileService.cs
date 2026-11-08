using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Account;
using BookVerse.Core.Models.Profile;
using BookVerse.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPermissionCacheService _permissionCacheService;

    public ProfileService(AppDbContext context, UserManager<ApplicationUser> userManager, IPermissionCacheService permissionCacheService)
    {
        _context = context;
        _userManager = userManager;
        _permissionCacheService = permissionCacheService;
    }

    public async Task<ServiceResult<ProfileResponse>> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return ServiceResult<ProfileResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy người dùng.");
        }

        var province = user.ProvinceId.HasValue
            ? await _context.Provinces.AsNoTracking().FirstOrDefaultAsync(x => x.Id == user.ProvinceId.Value, cancellationToken)
            : null;
        var commune = user.CommuneId.HasValue
            ? await _context.Communes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == user.CommuneId.Value, cancellationToken)
            : null;

        var roles = await (from userRole in _context.UserRoles.AsNoTracking()
                           join role in _context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                           where userRole.UserId == userId && role.IsActive && !role.IsDeleted
                           select role.Name!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        return ServiceResult<ProfileResponse>.Success(new ProfileResponse
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Address = user.Address,
            ProvinceId = user.ProvinceId,
            ProvinceName = province?.Name,
            CommuneId = user.CommuneId,
            CommuneName = commune?.Name,
            Birthday = user.Birthday,
            VipTier = user.VipTier.ToString(),
            RewardPoints = user.RewardPoints,
            LifetimeSpend = user.LifetimeSpend,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles
        }, "Lấy hồ sơ cá nhân thành công.");
    }

    public async Task<ServiceResult<ProfileResponse>> UpdateAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return ServiceResult<ProfileResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy người dùng.");
        }

        if (!string.IsNullOrWhiteSpace(request.FullName)) user.FullName = request.FullName.Trim();
        if (request.PhoneNumber is not null) user.PhoneNumber = request.PhoneNumber;
        if (request.AvatarUrl is not null) user.AvatarUrl = request.AvatarUrl;
        if (request.Address is not null) user.Address = request.Address;
        if (request.ProvinceId.HasValue) user.ProvinceId = request.ProvinceId;
        if (request.CommuneId.HasValue) user.CommuneId = request.CommuneId;
        if (request.Birthday.HasValue) user.Birthday = request.Birthday;

        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return await GetAsync(userId, cancellationToken);
    }

    public async Task<ServiceResult<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy người dùng.");
        }

        var changeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changeResult.Succeeded)
        {
            return ServiceResult<bool>.Failure(
                HttpStatusCode.BadRequest,
                "Đổi mật khẩu thất bại.",
                changeResult.Errors.Select(x => x.Description).ToList());
        }

        await _userManager.UpdateSecurityStampAsync(user);
        _permissionCacheService.InvalidateUserPermissions(userId);

        return ServiceResult<bool>.Success(true, "Đổi mật khẩu thành công.");
    }
}
