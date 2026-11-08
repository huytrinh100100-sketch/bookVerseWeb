using BookVerse.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BookVerse.Infrastructure.Services;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionCode { get; }

    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionCacheService _permissionCacheService;

    public PermissionAuthorizationHandler(IPermissionCacheService permissionCacheService)
    {
        _permissionCacheService = permissionCacheService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userIdRaw = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            return;
        }

        var result = await _permissionCacheService.GetUserPermissionsAsync(userId);
        if (result.IsSuccess && result.Data is not null && result.Data.Contains(requirement.PermissionCode, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }
    }
}
