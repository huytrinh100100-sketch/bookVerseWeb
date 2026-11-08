using BookVerse.Core.Common;
using BookVerse.Core.Models.Account;

namespace BookVerse.Core.Interfaces;

public interface IAccountService
{
    Task<ServiceResult<PagedResult<AccountListItemResponse>>> SearchAsync(AccountSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<AccountDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<AccountDetailResponse>> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<AccountDetailResponse>> UpdateAsync(Guid id, UpdateAccountRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<AccountDetailResponse>> AssignRolesAsync(Guid id, AssignRolesRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> LockAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> UnlockAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> HardDeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> RestoreAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
}
