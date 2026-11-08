using System.Security.Claims;
using BookVerse.Core.Common;
using BookVerse.Core.Models.Auth;

namespace BookVerse.Core.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> LogoutAsync(Guid currentUserId, LogoutRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<MeResponse>> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
