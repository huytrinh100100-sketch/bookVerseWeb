using BookVerse.Core.Constants;

namespace BookVerse.Core.Entities;

public class Article
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? CoverImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? AuthorId { get; set; }
    public Guid? EditorId { get; set; }
    public string LinkedBookIds { get; set; } = string.Empty;
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public int ViewCount { get; set; }
    public int ConversionCount { get; set; }
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    public DateTime? PublishedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public CommonCategory? Category { get; set; }
    public Author? Author { get; set; }
}
