namespace BookVerse.Core.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "BookVerse.WebAPI";
    public string Audience { get; set; } = "BookVerse.Frontend";
    public string SecretKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
}
