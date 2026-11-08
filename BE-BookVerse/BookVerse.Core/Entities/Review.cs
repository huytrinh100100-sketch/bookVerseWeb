using BookVerse.Core.Constants;

namespace BookVerse.Core.Entities;

public class Review
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Guid CustomerId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    public Sentiment Sentiment { get; set; } = Sentiment.Neutral;
    public string MediaUrls { get; set; } = string.Empty;
    public bool IsVerifiedPurchase { get; set; }
    public int HelpfulCount { get; set; }
    public string? ReplyContent { get; set; }
    public Guid? RepliedBy { get; set; }
    public DateTime? RepliedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Book Book { get; set; } = null!;
    public ApplicationUser Customer { get; set; } = null!;
}
