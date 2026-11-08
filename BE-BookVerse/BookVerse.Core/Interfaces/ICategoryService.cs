using BookVerse.Core.Common;
using BookVerse.Core.Models.Category;

namespace BookVerse.Core.Interfaces;

public interface ICategoryService
{
    Task<ServiceResult<PagedResult<CategoryListItemResponse>>> SearchAsync(CategorySearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<CategoryDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<CategoryTreeNodeResponse>>> GetTreeAsync(string? rootCode = null, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<CategoryRootResponse>>> GetRootsAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<CategoryListItemResponse>>> GetItemsByRootCodeAsync(string rootCode, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<ServiceResult<CategoryDetailResponse>> CreateAsync(CreateCategoryRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<CategoryDetailResponse>> UpdateAsync(Guid id, UpdateCategoryRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<CategoryDetailResponse>> MoveAsync(Guid id, MoveCategoryRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> UpdateStatusAsync(Guid id, UpdateCategoryStatusRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> HardDeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> RestoreAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> ReorderAsync(ReorderCategoriesRequest request, Guid operatorId, CancellationToken cancellationToken = default);
}
