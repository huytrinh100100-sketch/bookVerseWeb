namespace BookVerse.Core.Entities;

/// <summary>Sổ địa chỉ nhận hàng của khách.</summary>
public class CustomerAddress
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Guid ProvinceId { get; set; }
    public Guid CommuneId { get; set; }
    public bool IsDefault { get; set; }
    public string? Note { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public ApplicationUser Customer { get; set; } = null!;
    public Province? Province { get; set; }
    public Commune? Commune { get; set; }
}
