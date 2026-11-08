using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Account;
using BookVerse.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IPermissionCacheService _permissionCacheService;
    private readonly IAuditLogService _auditLogService;

    public AccountService(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IPermissionCacheService permissionCacheService,
        IAuditLogService auditLogService)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _permissionCacheService = permissionCacheService;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<PagedResult<AccountListItemResponse>>> SearchAsync(AccountSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.Users.AsNoTracking();

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (request.IsDeleted.HasValue)
        {
            query = query.Where(x => x.IsDeleted == request.IsDeleted.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                (x.UserName != null && x.UserName.ToLower().Contains(keyword)) ||
                (x.FullName != null && x.FullName.ToLower().Contains(keyword)) ||
                (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                (x.PhoneNumber != null && x.PhoneNumber.Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.UserName,
                x.FullName,
                x.Email,
                x.PhoneNumber,
                x.IsActive,
                x.IsDeleted,
                x.LockoutEnd,
                x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var roleMap = await BuildUserRoleMapAsync(users.Select(x => x.Id).ToList(), cancellationToken);

        var items = users.Select(x => new AccountListItemResponse
        {
            Id = x.Id,
            UserName = x.UserName ?? string.Empty,
            FullName = x.FullName,
            Email = x.Email,
            PhoneNumber = x.PhoneNumber,
            IsActive = x.IsActive,
            IsDeleted = x.IsDeleted,
            IsLocked = x.LockoutEnd.HasValue && x.LockoutEnd > DateTimeOffset.UtcNow,
            LockoutEnd = x.LockoutEnd,
            Roles = roleMap.TryGetValue(x.Id, out var roles) ? roles : Array.Empty<string>(),
            CreatedAt = x.CreatedAt
        }).ToList();

        return ServiceResult<PagedResult<AccountListItemResponse>>.Success(new PagedResult<AccountListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách tài khoản thành công.");
    }

    public async Task<ServiceResult<AccountDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<AccountDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tài khoản.");
        }

        var roleMap = await BuildUserRoleMapAsync(new List<Guid> { id }, cancellationToken);

        return ServiceResult<AccountDetailResponse>.Success(new AccountDetailResponse
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive,
            IsDeleted = user.IsDeleted,
            EmailConfirmed = user.EmailConfirmed,
            LockoutEnd = user.LockoutEnd,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = roleMap.TryGetValue(id, out var roles) ? roles : Array.Empty<string>()
        }, "Lấy chi tiết tài khoản thành công.");
    }

    public async Task<ServiceResult<AccountDetailResponse>> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        if (!ServiceHelpers.IsValidEmail(request.Email))
        {
            return ServiceResult<AccountDetailResponse>.Failure(HttpStatusCode.BadRequest, "Email không hợp lệ.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber,
            IsActive = request.IsActive,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return ServiceResult<AccountDetailResponse>.Failure(
                HttpStatusCode.BadRequest,
                "Không thể tạo tài khoản.",
                createResult.Errors.Select(x => x.Description).ToList());
        }

        if (request.Roles.Count > 0)
        {
            var validRoles = request.Roles.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
            var existRoles = await _roleManager.Roles.Where(r => validRoles.Contains(r.Name!)).Select(r => r.Name!).ToListAsync(cancellationToken);
            if (existRoles.Count > 0)
            {
                await _userManager.AddToRolesAsync(user, existRoles);
            }
        }

        await WriteAuditAsync(user.Id, user.UserName, "Tạo tài khoản", "Account", $"Tạo tài khoản {user.UserName}.");

        return await GetDetailAsync(user.Id, cancellationToken);
    }

    public async Task<ServiceResult<AccountDetailResponse>> UpdateAsync(Guid id, UpdateAccountRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<AccountDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tài khoản.");
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (!ServiceHelpers.IsValidEmail(request.Email))
            {
                return ServiceResult<AccountDetailResponse>.Failure(HttpStatusCode.BadRequest, "Email không hợp lệ.");
            }

            var emailOwner = await _userManager.Users.AnyAsync(x => x.Id != id && x.NormalizedEmail == request.Email.Trim().ToUpperInvariant(), cancellationToken);
            if (emailOwner)
            {
                return ServiceResult<AccountDetailResponse>.Failure(HttpStatusCode.Conflict, "Email đã được sử dụng.");
            }

            user.Email = request.Email.Trim();
        }

        if (request.PhoneNumber is not null)
        {
            user.PhoneNumber = request.PhoneNumber;
        }

        if (request.AvatarUrl is not null)
        {
            user.AvatarUrl = request.AvatarUrl;
        }

        if (request.IsActive.HasValue && request.IsActive.Value != user.IsActive)
        {
            user.IsActive = request.IsActive.Value;
            await _userManager.UpdateSecurityStampAsync(user);
            _permissionCacheService.InvalidateUserPermissions(user.Id);
        }

        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = operatorId.ToString();

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return ServiceResult<AccountDetailResponse>.Failure(HttpStatusCode.BadRequest, "Cập nhật tài khoản thất bại.");
        }

        await WriteAuditAsync(operatorId, null, "Cập nhật tài khoản", "Account", $"Cập nhật tài khoản {user.UserName}.");

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<AccountDetailResponse>> AssignRolesAsync(Guid id, AssignRolesRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<AccountDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tài khoản.");
        }

        var validRoles = request.Roles.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
        var existRoles = await _roleManager.Roles.Where(r => validRoles.Contains(r.Name!)).Select(r => r.Name!).ToListAsync(cancellationToken);

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeRoles = currentRoles.Except(existRoles).ToList();
        var addRoles = existRoles.Except(currentRoles).ToList();

        if (removeRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, removeRoles);
        }

        if (addRoles.Count > 0)
        {
            await _userManager.AddToRolesAsync(user, addRoles);
        }

        await _userManager.UpdateSecurityStampAsync(user);
        _permissionCacheService.InvalidateUserPermissions(user.Id);

        await WriteAuditAsync(operatorId, null, "Gán vai trò", "Account", $"Gán vai trò cho {user.UserName}.");

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> LockAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tài khoản.");
        }

        await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(100));
        await _userManager.UpdateSecurityStampAsync(user);
        _permissionCacheService.InvalidateUserPermissions(user.Id);

        await WriteAuditAsync(operatorId, null, "Khóa tài khoản", "Account", $"Khóa tài khoản {user.UserName}.");

        return ServiceResult<bool>.Success(true, "Khóa tài khoản thành công.");
    }

    public async Task<ServiceResult<bool>> UnlockAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tài khoản.");
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);
        _permissionCacheService.InvalidateUserPermissions(user.Id);

        await WriteAuditAsync(operatorId, null, "Mở khóa tài khoản", "Account", $"Mở khóa tài khoản {user.UserName}.");

        return ServiceResult<bool>.Success(true, "Mở khóa tài khoản thành công.");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tài khoản.");
        }

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = operatorId.ToString();
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = operatorId.ToString();

        await _userManager.UpdateAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);
        _permissionCacheService.InvalidateUserPermissions(user.Id);

        await WriteAuditAsync(operatorId, null, "Xóa mềm tài khoản", "Account", $"Xóa mềm tài khoản {user.UserName}.");

        return ServiceResult<bool>.Success(true, "Xóa mềm tài khoản thành công.");
    }

    public async Task<ServiceResult<bool>> HardDeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tài khoản.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Xóa vĩnh viễn tài khoản thất bại.");
        }

        _permissionCacheService.InvalidateUserPermissions(user.Id);
        await WriteAuditAsync(operatorId, null, "Xóa vĩnh viễn tài khoản", "Account", $"Xóa vĩnh viễn tài khoản {user.UserName}.");

        return ServiceResult<bool>.Success(true, "Xóa vĩnh viễn tài khoản thành công.");
    }

    public async Task<ServiceResult<bool>> RestoreAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tài khoản.");
        }

        user.IsDeleted = false;
        user.DeletedAt = null;
        user.DeletedBy = null;
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = operatorId.ToString();

        await _userManager.UpdateAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);
        _permissionCacheService.InvalidateUserPermissions(user.Id);

        await WriteAuditAsync(operatorId, null, "Khôi phục tài khoản", "Account", $"Khôi phục tài khoản {user.UserName}.");

        return ServiceResult<bool>.Success(true, "Khôi phục tài khoản thành công.");
    }

    private async Task<Dictionary<Guid, string[]>> BuildUserRoleMapAsync(List<Guid> userIds, CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, string[]>();
        }

        var rows = await (from userRole in _context.UserRoles.AsNoTracking()
                          join role in _context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                          where userIds.Contains(userRole.UserId)
                          select new { userRole.UserId, role.Name })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Name!).Distinct().OrderBy(x => x).ToArray());
    }

    private async Task WriteAuditAsync(Guid? operatorId, string? operatorName, string action, string module, string note)
    {
        await _auditLogService.WriteAsync(new AuditLog
        {
            ActorUserId = operatorId,
            ActorUserName = operatorName,
            Action = action,
            Module = module,
            Note = note,
            EntityType = "Account",
            IsSuccess = true
        });
    }
}
