using BookVerse.Core.Models.Auth;

namespace BookVerse.Core.Interfaces;

public interface IJwtTokenService
{
    AccessTokenResult CreateAccessToken(AuthUserDto user, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions, string? securityStamp);
}
