using BookVerse.Core.Common;
using BookVerse.Core.Models.Account;
using BookVerse.Core.Models.Profile;

namespace BookVerse.Core.Interfaces;

public interface IProfileService
{
    Task<ServiceResult<ProfileResponse>> GetAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ServiceResult<ProfileResponse>> UpdateAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}
