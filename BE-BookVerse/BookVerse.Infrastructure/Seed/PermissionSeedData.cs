using BookVerse.Core.Constants;

namespace BookVerse.Infrastructure.Seed;

public static class PermissionSeedData
{
    public static IReadOnlyList<PermissionDefinition> GetAll()
    {
        return PermissionConstants.GetAll();
    }
}
