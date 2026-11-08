namespace BookVerse.Core.Models.Profile;

public class ProfileResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Address { get; set; }
    public Guid? ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
    public Guid? CommuneId { get; set; }
    public string? CommuneName { get; set; }
    public DateTime? Birthday { get; set; }
    public string VipTier { get; set; } = "Standard";
    public int RewardPoints { get; set; }
    public decimal LifetimeSpend { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = [];
}
