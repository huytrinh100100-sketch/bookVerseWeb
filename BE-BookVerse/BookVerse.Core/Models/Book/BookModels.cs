using BookVerse.Core.Constants;
using BookVerse.Core.Models.Content;

namespace BookVerse.Core.Models.Book;

public class BookSearchRequest
{
    public string? Keyword { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? AuthorId { get; set; }
    public Guid? PublisherId { get; set; }
    public BookStatus? Status { get; set; }
    public UnitType? UnitType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public string? StockLevel { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class BookListItemResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public Guid? PublisherId { get; set; }
    public string? PublisherName { get; set; }
    public string? CoverImageUrl { get; set; }
    public decimal PriceForSell { get; set; }
    public decimal PriceForRent { get; set; }
    public int PageCount { get; set; }
    public int TotalQuantity { get; set; }
    public int StockForSell { get; set; }
    public int StockForRent { get; set; }
    public BookStatus Status { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public decimal? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int SoldCount { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class BookDetailResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public Guid? PublisherId { get; set; }
    public string? PublisherName { get; set; }
    public decimal PriceForSell { get; set; }
    public decimal PriceForRent { get; set; }
    public int PageCount { get; set; }
    public int? PublicationYear { get; set; }
    public string? Language { get; set; }
    public string? Format { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public int TotalQuantity { get; set; }
    public int StockForSell { get; set; }
    public int StockForRent { get; set; }
    public BookStatus Status { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public decimal? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int SoldCount { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public byte[]? RowVersion { get; set; }
    public IReadOnlyCollection<BookCopyListItemResponse> Copies { get; set; } = [];
    public IReadOnlyCollection<ReviewListItemResponse> Reviews { get; set; } = [];
}

public class CreateBookRequest
{
    public string? Code { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Slug { get; set; }
    public string? Isbn { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? AuthorId { get; set; }
    public Guid? PublisherId { get; set; }
    public decimal PriceForSell { get; set; }
    public decimal PriceForRent { get; set; }
    public int PageCount { get; set; }
    public int? PublicationYear { get; set; }
    public string? Language { get; set; }
    public string? Format { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public int Quantity { get; set; }
    public BookStatus Status { get; set; } = BookStatus.Draft;
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateBookRequest
{
    public string? Code { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Slug { get; set; }
    public string? Isbn { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? AuthorId { get; set; }
    public Guid? PublisherId { get; set; }
    public decimal? PriceForSell { get; set; }
    public decimal? PriceForRent { get; set; }
    public int? PageCount { get; set; }
    public int? PublicationYear { get; set; }
    public string? Language { get; set; }
    public string? Format { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public BookStatus? Status { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsActive { get; set; }
    public byte[]? RowVersion { get; set; }
}

public class UpdateBookStatusRequest
{
    public bool IsActive { get; set; }
    public BookStatus? Status { get; set; }
}

/// <summary>Điều chỉnh tồn kho sách (tương đương FE BookStockAdjustRequest).</summary>
public class UpdateBookStockRequest
{
    public int StockForSell { get; set; }
    public int StockForRent { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class BookCopyListItemResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string ConditionStatus { get; set; } = string.Empty;
    public string? ShelfCode { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public DateTime? AcquiredDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public BookCopyStatus Status { get; set; }
    public Guid? CurrentOrderId { get; set; }
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; }
}

public class BookCopySearchRequest
{
    public string? Keyword { get; set; }
    public Guid? BookId { get; set; }
    public Guid? WarehouseId { get; set; }
    public BookCopyStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CreateBookCopyRequest
{
    public Guid BookId { get; set; }
    public string? Barcode { get; set; }
    public string ConditionStatus { get; set; } = "Moi";
    public string? ShelfCode { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime? AcquiredDate { get; set; }
    public string? Notes { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateBookCopyRequest
{
    public string? ConditionStatus { get; set; }
    public string? ShelfCode { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime? AcquiredDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public BookCopyStatus? Status { get; set; }
    public string? Notes { get; set; }
}

public class AdjustStockRequest
{
    public Guid BookId { get; set; }
    public UnitType UnitType { get; set; } = UnitType.Sell;
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? WarehouseId { get; set; }
}
