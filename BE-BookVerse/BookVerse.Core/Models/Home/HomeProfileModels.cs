namespace BookVerse.Core.Models.HomeProfile;

public class HomeResponse
{
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; set; } = [];
    public int PermissionCount { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
