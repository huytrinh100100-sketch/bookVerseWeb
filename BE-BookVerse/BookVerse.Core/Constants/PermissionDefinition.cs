namespace BookVerse.Core.Constants;

public sealed record PermissionDefinition(
    string Code,
    string Name,
    string GroupCode,
    string GroupName,
    int SortOrder,
    string? Description = null);
