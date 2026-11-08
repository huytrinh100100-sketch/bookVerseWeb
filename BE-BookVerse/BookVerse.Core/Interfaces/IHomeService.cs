using BookVerse.Core.Common;
using BookVerse.Core.Models.HomeProfile;

namespace BookVerse.Core.Interfaces;

public interface IHomeService
{
    Task<ServiceResult<HomeResponse>> GetAsync(Guid userId, CancellationToken cancellationToken = default);
}
