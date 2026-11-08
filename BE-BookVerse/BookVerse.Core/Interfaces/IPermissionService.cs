using BookVerse.Core.Common;
using BookVerse.Core.Models.Permission;

namespace BookVerse.Core.Interfaces;

public interface IPermissionService
{
    Task<ServiceResult<PagedResult<PermissionListItemResponse>>> SearchAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<PermissionListItemResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
}
