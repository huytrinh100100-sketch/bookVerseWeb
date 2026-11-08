using BookVerse.Core.Constants;

namespace BookVerse.Core.Entities;

public class Book
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
    public Guid? AuthorId { get; set; }
    public Guid? PublisherId { get; set; }

    public decimal PriceForSell { get; set; }
    public decimal PriceForRent { get; set; }
    public int PageCount { get; set; }
    public int? PublicationYear { get; set; }
    public string? Language { get; set; }
    public string? Format { get; set; }
    public string? Weight { get; set; }
    public string? Dimensions { get; set; }

    /// <summary>Tổng số bản sao đang tồn tại trong hệ thống (denormalized).</summary>
    public int TotalQuantity { get; set; }

    /// <summary>Tồn kho để bán (denormalized, cập nhật trong cùng transaction).</summary>
    public int StockForSell { get; set; }

    /// <summary>Tồn kho để mượn (denormalized).</summary>
    public int StockForRent { get; set; }

    public BookStatus Status { get; set; } = BookStatus.Draft;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public decimal? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int SoldCount { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    /// <summary>Concurrency token - chống cập nhật trùng khi 2 nhân viên cùng xử lý.</summary>
    public byte[]? RowVersion { get; set; }

    public CommonCategory? Category { get; set; }
    public Author? Author { get; set; }
    public Publisher? Publisher { get; set; }
    public ICollection<BookCopy> Copies { get; set; } = [];
    public ICollection<OrderDetail> OrderDetails { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<WishlistItem> WishlistItems { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
}
