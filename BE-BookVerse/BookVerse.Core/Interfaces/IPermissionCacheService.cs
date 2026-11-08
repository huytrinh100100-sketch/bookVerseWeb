using BookVerse.Core.Common;

namespace BookVerse.Core.Interfaces;

public interface IPermissionCacheService
{
    Task<ServiceResult<IReadOnlyCollection<string>>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    void InvalidateUserPermissions(Guid userId);
    void InvalidateAll();
}
