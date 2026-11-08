namespace BookVerse.Core.Entities;

public class WishlistItem
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid BookId { get; set; }
    public DateTime CreatedAt { get; set; }

    public ApplicationUser Customer { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
