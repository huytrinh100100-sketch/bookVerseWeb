using System.Net;
using System.Security.Claims;
using BookVerse.Core.Common;
using BookVerse.Core.Constants;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Auth;
using BookVerse.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        AppDbContext context,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IAuditLogService auditLogService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _context = context;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await FindUserAsync(request.UserNameOrEmail);
        if (user is null)
        {
            await WriteAuditAsync(null, request.UserNameOrEmail, "Đăng nhập thất bại", "Auth", "Không tìm thấy tài khoản.");
            return ServiceResult<AuthResponse>.Failure(HttpStatusCode.Unauthorized, "Tên đăng nhập, email hoặc mật khẩu không đúng.");
        }

        if (!user.IsActive || user.IsDeleted)
        {
            await WriteAuditAsync(user.Id, user.UserName, "Đăng nhập thất bại", "Auth", "Tài khoản đã bị vô hiệu hóa hoặc xóa mềm.");
            return ServiceResult<AuthResponse>.Failure(HttpStatusCode.Forbidden, "Tài khoản không hoạt động.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            await WriteAuditAsync(user.Id, user.UserName, "Đăng nhập thất bại", "Auth", "Tài khoản đang bị khóa.");
            return ServiceResult<AuthResponse>.Failure(HttpStatusCode.Forbidden, "Tài khoản đang bị khóa.");
        }

        var passwordResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!passwordResult.Succeeded)
        {
            var message = passwordResult.IsLockedOut
                ? "Tài khoản đang bị khóa do đăng nhập sai quá số lần cho phép."
                : "Tên đăng nhập, email hoặc mật khẩu không đúng.";

            await WriteAuditAsync(user.Id, user.UserName, "Đăng nhập thất bại", "Auth", message);
            return ServiceResult<AuthResponse>.Failure(passwordResult.IsLockedOut ? HttpStatusCode.Forbidden : HttpStatusCode.Unauthorized, message);
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var authResponse = await BuildAuthResponseAsync(user, cancellationToken);
        await WriteAuditAsync(user.Id, user.UserName, "Đăng nhập thành công", "Auth", "Đăng nhập hệ thống thành công.");

        return ServiceResult<AuthResponse>.Success(authResponse, "Đăng nhập thành công.");
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (!ServiceHelpers.IsValidEmail(request.Email))
        {
            return ServiceResult<AuthResponse>.Failure(HttpStatusCode.BadRequest, "Email không hợp lệ.");
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && !ServiceHelpers.IsValidPhone(request.PhoneNumber))
        {
            return ServiceResult<AuthResponse>.Failure(HttpStatusCode.BadRequest, "Số điện thoại không hợp lệ.");
        }

        var exists = await _userManager.Users
            .AnyAsync(x => x.NormalizedEmail == request.Email.Trim().ToUpperInvariant()
                        || x.NormalizedUserName == request.UserName.Trim().ToUpperInvariant(), cancellationToken);

        if (exists)
        {
            return ServiceResult<AuthResponse>.Failure(HttpStatusCode.Conflict, "Tên đăng nhập hoặc email đã tồn tại.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            ProvinceId = request.ProvinceId,
            CommuneId = request.CommuneId,
            CustomerStatus = CustomerStatus.Active,
            VipTier = VipTier.Standard,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "register"
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return ServiceResult<AuthResponse>.Failure(
                HttpStatusCode.BadRequest,
                "Không thể tạo tài khoản.",
                createResult.Errors.Select(x => x.Description).ToList());
        }

        // Gán vai trò Customer mặc định
        var customerRole = await _roleManager.FindByNameAsync("Customer");
        if (customerRole is not null)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
            if (!roleResult.Succeeded)
            {
                return ServiceResult<AuthResponse>.Failure(
                    HttpStatusCode.BadRequest,
                    "Không thể gán vai trò Customer.",
                    roleResult.Errors.Select(x => x.Description).ToList());
            }
        }
        await WriteAuditAsync(user.Id, user.UserName, "Đăng ký tài khoản", "Auth", "Khách hàng đăng ký tài khoản mới.");

        var authResponse = await BuildAuthResponseAsync(user, cancellationToken);
        return ServiceResult<AuthResponse>.Success(authResponse, "Đăng ký tài khoản thành công.");
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var existingToken = await _refreshTokenService.GetByRawTokenAsync(request.RefreshToken, cancellationToken);
        if (existingToken is null)
        {
            return ServiceResult<AuthResponse>.Failure(HttpStatusCode.Unauthorized, "Refresh token không hợp lệ.");
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == existingToken.UserId, cancellationToken);
        if (user is null || !user.IsActive || user.IsDeleted)
        {
            return ServiceResult<AuthResponse>.Failure(HttpStatusCode.Unauthorized, "Tài khoản không còn hợp lệ.");
        }

        var ipAddress = ResolveClientIpAddress();

        if (existingToken.RevokedAt != null)
        {
            await _refreshTokenService.RevokeAllActiveTokensAsync(user.Id, ipAddress, "Phát hiện tái sử dụng refresh token đã bị thu hồi.", cancellationToken);
            await WriteAuditAsync(user.Id, user.UserName, "Phát hiện tái sử dụng refresh token", "Auth", "Hệ thống đã thu hồi toàn bộ refresh token hoạt động.");
            return ServiceResult<AuthResponse>.Failure(HttpStatusCode.Unauthorized, "Refresh token đã bị thu hồi.");
        }

        if (existingToken.ExpiresAt <= DateTime.UtcNow)
        {
            return ServiceResult<AuthResponse>.Failure(HttpStatusCode.Unauthorized, "Refresh token đã hết hạn.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var newRefreshToken = await _refreshTokenService.CreateAsync(user.Id, ipAddress, cancellationToken);
        await _refreshTokenService.RevokeAsync(
            existingToken,
            ipAddress,
            "Làm mới refresh token thành công.",
            newRefreshToken.TokenHash,
            cancellationToken);

        var authResponse = await BuildAuthResponseAsync(user, cancellationToken, newRefreshToken);

        await transaction.CommitAsync(cancellationToken);

        await WriteAuditAsync(user.Id, user.UserName, "Làm mới token thành công", "Auth", "Làm mới access token và refresh token thành công.");

        return ServiceResult<AuthResponse>.Success(authResponse, "Làm mới token thành công.");
    }

    public async Task<ServiceResult<bool>> LogoutAsync(Guid currentUserId, LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var existingToken = await _refreshTokenService.GetByRawTokenAsync(request.RefreshToken, cancellationToken);
        if (existingToken is null || existingToken.UserId != currentUserId)
        {
            return ServiceResult<bool>.Failure(HttpStatusCode.BadRequest, "Refresh token không hợp lệ.");
        }

        await _refreshTokenService.RevokeAsync(existingToken, ResolveClientIpAddress(), "Đăng xuất khỏi hệ thống.", null, cancellationToken);

        var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == currentUserId, cancellationToken);
        await WriteAuditAsync(currentUserId, user?.UserName, "Đăng xuất", "Auth", "Đăng xuất hệ thống thành công.");

        return ServiceResult<bool>.Success(true, "Đăng xuất thành công.");
    }

    public async Task<ServiceResult<MeResponse>> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var userIdRaw = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            return ServiceResult<MeResponse>.Failure(HttpStatusCode.Unauthorized, "Phiên đăng nhập không hợp lệ.");
        }

        var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return ServiceResult<MeResponse>.Failure(HttpStatusCode.Unauthorized, "Phiên đăng nhập không hợp lệ.");
        }

        var roles = await GetRoleNamesAsync(userId, cancellationToken);
        var permissions = await GetPermissionCodesAsync(userId, cancellationToken);

        var response = new MeResponse
        {
            User = new AuthUserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                VipTier = user.VipTier.ToString(),
                RewardPoints = user.RewardPoints
            },
            Roles = roles,
            Permissions = permissions
        };

        return ServiceResult<MeResponse>.Success(response, "Lấy thông tin người dùng thành công.");
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(
        ApplicationUser user,
        CancellationToken cancellationToken,
        RefreshTokenIssueResult? refreshTokenIssueResult = null)
    {
        var roles = await GetRoleNamesAsync(user.Id, cancellationToken);
        var permissions = await GetPermissionCodesAsync(user.Id, cancellationToken);
        var userDto = new AuthUserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            VipTier = user.VipTier.ToString(),
            RewardPoints = user.RewardPoints
        };

        var accessTokenResult = _jwtTokenService.CreateAccessToken(userDto, roles, permissions, user.SecurityStamp);
        var refreshResult = refreshTokenIssueResult ?? await _refreshTokenService.CreateAsync(user.Id, ResolveClientIpAddress(), cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessTokenResult.AccessToken,
            RefreshToken = refreshResult.RawToken,
            ExpiresAt = accessTokenResult.ExpiresAt,
            User = userDto,
            Roles = roles,
            Permissions = permissions
        };
    }

    private async Task<IReadOnlyCollection<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await (from userRole in _context.UserRoles.AsNoTracking()
                      join role in _context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                      where userRole.UserId == userId && role.IsActive && !role.IsDeleted
                      orderby role.Name
                      select role.Name!)
            .Distinct()
            .ToArrayAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<string>> GetPermissionCodesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await (from userRole in _context.UserRoles.AsNoTracking()
                      join role in _context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                      join rolePermission in _context.RolePermissions.AsNoTracking() on role.Id equals rolePermission.RoleId
                      join permission in _context.Permissions.AsNoTracking() on rolePermission.PermissionId equals permission.Id
                      where userRole.UserId == userId
                            && role.IsActive
                            && !role.IsDeleted
                            && permission.IsActive
                      orderby permission.Code
                      select permission.Code)
            .Distinct()
            .ToArrayAsync(cancellationToken);
    }

    private async Task<ApplicationUser?> FindUserAsync(string userNameOrEmail)
    {
        var normalizedInput = userNameOrEmail.Trim().ToUpperInvariant();

        return await _userManager.Users
            .FirstOrDefaultAsync(x =>
                x.NormalizedUserName == normalizedInput ||
                x.NormalizedEmail == normalizedInput);
    }

    private async Task WriteAuditAsync(Guid? userId, string? userName, string action, string module, string note)
    {
        await _auditLogService.WriteAsync(new AuditLog
        {
            ActorUserId = userId,
            ActorUserName = userName,
            Action = action,
            Module = module,
            Note = note,
            EntityType = "Auth",
            IsSuccess = !action.Contains("thất bại", StringComparison.OrdinalIgnoreCase)
        });
    }

    private string? ResolveClientIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }
}
