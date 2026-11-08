using BookVerse.Core.Constants;

namespace BookVerse.Core.Entities;

public class Author
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Biography { get; set; }
    public string? AvatarUrl { get; set; }
    public AgreementStatus AgreementStatus { get; set; } = AgreementStatus.Draft;
    public DateTime? AgreementExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public ICollection<Book> Books { get; set; } = [];
}
