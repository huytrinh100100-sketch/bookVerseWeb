using BookVerse.Core.Common;
using BookVerse.Core.Models.Book;

namespace BookVerse.Core.Interfaces;

public interface IBookCopyService
{
    Task<ServiceResult<PagedResult<BookCopyListItemResponse>>> SearchAsync(BookSearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>Tìm kiếm bản sao theo bộ lọc riêng của bản sao (FE: GET /book-copies).</summary>
    Task<ServiceResult<PagedResult<BookCopyListItemResponse>>> SearchCopiesAsync(BookCopySearchRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<BookCopyListItemResponse>>> GetByBookAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyCollection<BookCopyListItemResponse>>> CreateAsync(CreateBookCopyRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<BookCopyListItemResponse>> UpdateAsync(Guid id, UpdateBookCopyRequest request, Guid operatorId, CancellationToken cancellationToken = default);
    Task<ServiceResult<bool>> DeleteAsync(Guid id, Guid operatorId, CancellationToken cancellationToken = default);
}
