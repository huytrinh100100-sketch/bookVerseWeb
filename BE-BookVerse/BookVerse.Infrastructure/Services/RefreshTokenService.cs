using System.Security.Cryptography;
using System.Text;
using BookVerse.Core.Entities;
using BookVerse.Core.Interfaces;
using BookVerse.Core.Models.Auth;
using BookVerse.Core.Options;
using BookVerse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BookVerse.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext _context;
    private readonly RefreshTokenOptions _options;

    public RefreshTokenService(AppDbContext context, IOptions<RefreshTokenOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task<RefreshTokenIssueResult> CreateAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var rawToken = GenerateRawToken();
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_options.ExpiryDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIpAddress = ipAddress
        };

        await _context.RefreshTokens.AddAsync(token, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new RefreshTokenIssueResult
        {
            RawToken = rawToken,
            TokenHash = token.TokenHash,
            ExpiresAt = token.ExpiresAt
        };
    }

    public async Task<RefreshToken?> GetByRawTokenAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return null;
        }

        var hash = HashToken(rawToken);
        return await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
    }

    public async Task RevokeAsync(RefreshToken token, string? ipAddress, string reason, string? replacedByTokenHash = null, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Id == token.Id, cancellationToken);
        if (entity is null || entity.RevokedAt.HasValue)
        {
            return;
        }

        entity.RevokedAt = DateTime.UtcNow;
        entity.RevokedReason = reason;
        entity.RevokedByIpAddress = ipAddress;
        entity.ReplacedByTokenHash = replacedByTokenHash;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllActiveTokensAsync(Guid userId, string? ipAddress, string reason, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var token in tokens)
        {
            token.RevokedAt = now;
            token.RevokedReason = reason;
            token.RevokedByIpAddress = ipAddress;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>Chỉ lưu SHA-256 hash của refresh token, không lưu bản gốc.</summary>
    public static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
