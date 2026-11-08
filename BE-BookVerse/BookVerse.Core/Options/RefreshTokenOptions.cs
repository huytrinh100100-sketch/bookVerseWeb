namespace BookVerse.Core.Options;

public class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    public int ExpiryDays { get; set; } = 7;
    public int MaxActiveTokens { get; set; } = 5;
    public int TokenSizeBytes { get; set; } = 64;
}
