using System.Net;
using BookVerse.Core.Common;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Role;
using BookVerse.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IPermissionCacheService _permissionCacheService;
    private readonly IAuditLogService _auditLogService;

    public RoleService(
        AppDbContext context,
        RoleManager<ApplicationRole> roleManager,
        IPermissionCacheService permissionCacheService,
        IAuditLogService auditLogService)
    {
        _context = context;
        _roleManager = roleManager;
        _permissionCacheService = permissionCacheService;
        _auditLogService = auditLogService;
    }

    public async Task<ServiceResult<PagedResult<RoleListItemResponse>>> SearchAsync(RoleSearchRequest request, CancellationToken cancellationToken = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var query = _context.Roles.AsNoTracking();

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
                (x.Name != null && x.Name.ToLower().Contains(keyword)) ||
                (x.Description != null && x.Description.ToLower().Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var roles = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new RoleListItemResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name ?? string.Empty,
                Description = x.Description,
                IsSystem = x.IsSystem,
                IsDefault = x.IsDefault,
                IsActive = x.IsActive,
                IsDeleted = x.IsDeleted,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        foreach (var role in roles)
        {
            role.UserCount = await _context.UserRoles.CountAsync(x => x.RoleId == role.Id, cancellationToken);
            role.PermissionCount = await _context.RolePermissions.CountAsync(x => x.RoleId == role.Id, cancellationToken);
        }

        return ServiceResult<PagedResult<RoleListItemResponse>>.Success(new PagedResult<RoleListItemResponse>
        {
            Items = roles,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
        }, "Lấy danh sách vai trò thành công.");
    }

    public async Task<ServiceResult<RoleDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null)
        {
            return ServiceResult<RoleDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy vai trò.");
        }

        var permissions = await _context.RolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == id)
            .Join(_context.Permissions.AsNoTracking(), rp => rp.PermissionId, p => p.Id, (rp, p) => p.Code)
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        return ServiceResult<RoleDetailResponse>.Success(new RoleDetailResponse
        {
            Id = role.Id,
            Code = role.Code,
            Name = role.Name ?? string.Empty,
            Description = role.Description,
            IsSystem = role.IsSystem,
            IsDefault = role.IsDefault,
            IsActive = role.IsActive,
            IsDeleted = role.IsDeleted,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            PermissionCodes = permissions
        }, "Lấy chi tiết vai trò thành công.");
    }

    public async Task<ServiceResult<RoleDetailResponse>> CreateAsync(CreateRoleRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var code = string.IsNullOrWhiteSpace(request.Code) ? request.Name.Trim().ToLowerInvariant().Replace(" ", "-") : request.Code.Trim();

        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            NormalizedName = request.Name.Trim().ToUpperInvariant(),
            Code = code,
            Description = request.Description,
            IsDefault = request.IsDefault,
            IsActive = request.IsActive,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            return ServiceResult<RoleDetailResponse>.Failure(
                HttpStatusCode.BadRequest,
                "Không thể tạo vai trò.",
                result.Errors.Select(x => x.Description).ToList());
        }

        await WriteAuditAsync(operatorId, "Tạo vai trò", "Role", $"Tạo vai trò {role.Name}.");

        return await GetDetailAsync(role.Id, cancellationToken);
    }

    public async Task<ServiceResult<RoleDetailResponse>> UpdateAsync(Guid id, UpdateRoleRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null)
        {
            return ServiceResult<RoleDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy vai trò.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            role.Name = request.Name.Trim();
            role.NormalizedName = request.Name.Trim().ToUpperInvariant();
        }

        if (request.Description is not null)
        {
            role.Description = request.Description;
        }

        if (request.IsDefault.HasValue)
        {
            role.IsDefault = request.IsDefault.Value;
        }

        if (request.IsActive.HasValue)
        {
            role.IsActive = request.IsActive.Value;
        }

        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = operatorId.ToString();

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            return ServiceResult<RoleDetailResponse>.Failure(HttpStatusCode.BadRequest, "Cập nhật vai trò thất bại.");
        }

        await WriteAuditAsync(operatorId, "Cập nhật vai trò", "Role", $"Cập nhật vai trò {role.Name}.");

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<RoleDetailResponse>> AssignPermissionsAsync(Guid id, AssignPermissionsRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null)
        {
            return ServiceResult<RoleDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy vai trò.");
        }

        var codes = request.Permissions.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
        var targetPermissionIds = await _context.Permissions
            .Where(p => codes.Contains(p.Code))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var existingPermissionIds = await _context.RolePermissions
            .Where(x => x.RoleId == id)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken);

        var toAdd = targetPermissionIds.Except(existingPermissionIds).ToList();
        var toRemove = existingPermissionIds.Except(targetPermissionIds).ToList();

        foreach (var permissionId in toAdd)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = id,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = operatorId.ToString()
            });
        }

        var removeEntities = await _context.RolePermissions
            .Where(x => x.RoleId == id && toRemove.Contains(x.PermissionId))
            .ToListAsync(cancellationToken);

        _context.RolePermissions.RemoveRange(removeEntities);
        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate cache của các user thuộc vai trò này
        var userIds = await _context.UserRoles.Where(x => x.RoleId == id).Select(x => x.UserId).ToListAsync(cancellationToken);
        foreach (var userId in userIds)
        {
            _permissionCacheService.InvalidateUserPermissions(userId);
        }

        await WriteAuditAsync(operatorId, "Gán quyền", "Role", $"Gán quyền cho vai trò {role.Name}.");

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy vai trò.");
        }

        if (role.IsSystem)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Không thể xóa vai trò hệ thống.");
        }

        role.IsDeleted = true;
        role.IsActive = false;
        role.DeletedAt = DateTime.UtcNow;
        role.DeletedBy = operatorId.ToString();
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = operatorId.ToString();

        await _roleManager.UpdateAsync(role);

        await WriteAuditAsync(operatorId, "Xóa mềm vai trò", "Role", $"Xóa mềm vai trò {role.Name}.");

        return ServiceResult<bool>.Success(true, "Xóa mềm vai trò thành công.");
    }

    public async Task<ServiceResult<bool>> HardDeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy vai trò.");
        }

        if (role.IsSystem)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Không thể xóa vai trò hệ thống.");
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Xóa vĩnh viễn vai trò thất bại.");
        }

        await WriteAuditAsync(operatorId, "Xóa vĩnh viễn vai trò", "Role", $"Xóa vĩnh viễn vai trò {role.Name}.");

        return ServiceResult<bool>.Success(true, "Xóa vĩnh viễn vai trò thành công.");
    }

    public async Task<ServiceResult<bool>> RestoreAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.Roles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy vai trò.");
        }

        role.IsDeleted = false;
        role.DeletedAt = null;
        role.DeletedBy = null;
        role.IsActive = true;
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = operatorId.ToString();

        await _roleManager.UpdateAsync(role);

        await WriteAuditAsync(operatorId, "Khôi phục vai trò", "Role", $"Khôi phục vai trò {role.Name}.");

        return ServiceResult<bool>.Success(true, "Khôi phục vai trò thành công.");
    }

    private async Task WriteAuditAsync(Guid? operatorId, string action, string module, string note)
    {
        await _auditLogService.WriteAsync(new AuditLog
        {
            ActorUserId = operatorId,
            Action = action,
            Module = module,
            Note = note,
            EntityType = "Role",
            IsSuccess = true
        });
    }

    // ------------------------------------------------------------------ FE: clone / assign / staff

    /// <summary>Nhân bản vai trò (FE: POST /roles/{id}/clone).</summary>
    public async Task<ServiceResult<RoleDetailResponse>> CloneAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var source = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (source is null)
        {
            return ServiceResult<RoleDetailResponse>.Failure(HttpStatusCode.NotFound, "Không tìm thấy vai trò nguồn.");
        }

        var permissionIds = await _context.RolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == id)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken);

        var cloneName = $"{source.Name} (bản sao)";
        var suffix = 1;
        while (await _context.Roles.AnyAsync(x => x.Name == cloneName, cancellationToken))
        {
            suffix++;
            cloneName = $"{source.Name} (bản sao {suffix})";
        }

        var clone = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = cloneName,
            NormalizedName = cloneName.ToUpperInvariant(),
            Code = string.IsNullOrWhiteSpace(source.Code) ? $"role-{Guid.NewGuid().ToString("N")[..8]}" : $"{source.Code}-copy{suffix}",
            Description = source.Description,
            IsSystem = false,
            IsDefault = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId.ToString()
        };

        _context.Roles.Add(clone);

        foreach (var permissionId in permissionIds)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = clone.Id,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = operatorId.ToString()
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        await WriteAuditAsync(operatorId, "Nhân bản vai trò", "Role", $"Nhân bản vai trò {source.Name} thành {clone.Name}.");

        return await GetDetailAsync(clone.Id, cancellationToken);
    }

    /// <summary>Gán vai trò cho một tài khoản (FE: POST /roles/assign).</summary>
    public async Task<ServiceResult<bool>> AssignRoleAsync(AssignRoleRequest request, Guid operatorId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.UserId && !x.IsDeleted, cancellationToken);
        if (user is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy tài khoản.");
        }

        var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == request.RoleId && !x.IsDeleted, cancellationToken);
        if (role is null)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.NotFound, "Không tìm thấy vai trò.");
        }

        var exists = await _context.UserRoles.AnyAsync(x => x.UserId == request.UserId && x.RoleId == request.RoleId, cancellationToken);
        if (!exists)
        {
            _context.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>
            {
                UserId = request.UserId,
                RoleId = request.RoleId
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        _permissionCacheService.InvalidateUserPermissions(request.UserId);

        await WriteAuditAsync(operatorId, "Gán vai trò", "Role", $"Gán vai trò {role.Name} cho tài khoản {user.FullName}.");

        return ServiceResult<bool>.Success(true, "Gán vai trò thành công.");
    }

    /// <summary>Danh sách nhân viên (tài khoản có vai trò) (FE: GET /users/staff).</summary>
    public async Task<ServiceResult<IReadOnlyCollection<StaffMemberResponse>>> GetStaffAsync(CancellationToken cancellationToken = default)
    {
        var roleRows = await _context.UserRoles
            .AsNoTracking()
            .Join(_context.Roles.AsNoTracking(), ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Code })
            .ToListAsync(cancellationToken);

        var roleMap = roleRows
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Code).Distinct().ToList());

        var staff = await _context.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

        var items = staff
            .Where(x => roleMap.ContainsKey(x.Id))
            .Select(x => new StaffMemberResponse
            {
                Id = x.Id,
                UserName = x.UserName ?? string.Empty,
                Email = x.Email,
                FullName = x.FullName,
                RoleCodes = roleMap[x.Id],
                TwoFactorEnabled = x.TwoFactorEnabled,
                LastLoginAt = x.LastLoginAt,
                LastLoginIp = x.LastLoginIp,
                LastLoginLocation = x.LastLoginLocation,
                Status = !x.IsActive ? "Inactive" : x.LockoutEnd.HasValue && x.LockoutEnd > DateTime.UtcNow ? "Locked" : "Active"
            })
            .OrderBy(x => x.FullName)
            .ToList();

        return ServiceResult<IReadOnlyCollection<StaffMemberResponse>>.Success(items, "Lấy danh sách nhân viên thành công.");
    }
}
