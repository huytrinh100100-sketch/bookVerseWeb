using BookVerse.Core.Common;
using BookVerse.Core.Models.Book;

namespace BookVerse.Core.Interfaces;

public interface IBookService
{
    Task<ServiceResult<PagedResult<BookListItemResponse>>> SearchAsync(BookSearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<BookDetailResponse>> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceResult<BookDetailResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<ServiceResult<BookDetailResponse>> CreateAsync(CreateBookRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<BookDetailResponse>> UpdateAsync(Guid id, UpdateBookRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<BookDetailResponse>> UpdateStatusAsync(Guid id, UpdateBookStatusRequest request, Guid operatorId, CancellationToken cancellationToken = default);

    /// <summary>Điều chỉnh tồn kho sách (FE: PUT /books/{id}/stock).</summary>
    Task<ServiceResult<BookDetailResponse>> UpdateStockAsync(Guid id, UpdateBookStockRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> HardDeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> RestoreAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<BookListItemResponse>>> GetFeaturedAsync(int take = 8, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<BookListItemResponse>>> GetNewestAsync(int take = 8, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<BookListItemResponse>>> GetBestSellersAsync(int take = 8, CancellationToken cancellationToken = default);
}
