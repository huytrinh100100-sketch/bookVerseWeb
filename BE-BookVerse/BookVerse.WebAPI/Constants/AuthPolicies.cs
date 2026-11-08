namespace BookVerse.WebAPI.Constants;

public static class AuthPolicies
{
    /// <summary>Policy động: yêu cầu permission cụ thể (dùng qua AuthorizeAttribute).</summary>
    public const string Permission = "Permission";

    /// <summary>Chỉ vai trò Admin.</summary>
    public const string AdminOnly = "AdminOnly";
}
