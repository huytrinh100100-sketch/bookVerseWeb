using BookVerse.Core.Entities;
using BookVerse.Core.Models.Auth;

namespace BookVerse.Core.Interfaces;

public interface IRefreshTokenService
{
    Task<RefreshTokenIssueResult> CreateAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByRawTokenAsync(string rawToken, CancellationToken cancellationToken = default);
    Task RevokeAsync(RefreshToken token, string? ipAddress, string reason, string? replacedByTokenHash = null, CancellationToken cancellationToken = default);
    Task RevokeAllActiveTokensAsync(Guid userId, string? ipAddress, string reason, CancellationToken cancellationToken = default);
}
