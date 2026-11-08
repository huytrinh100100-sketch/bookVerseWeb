using BookVerse.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Options;

namespace BookVerse.WebAPI.Middleware;

/// <summary>
/// Tự sinh policy động theo mã permission: Authorize("book.list") sẽ tự tạo policy
/// yêu cầu claim "permission" có giá trị tương ứng.
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public const string PermissionPrefix = "Permission:";

    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissionCode = policyName[PermissionPrefix.Length..];

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permissionCode))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Policy đã được đăng ký sẵn trong options (ví dụ: AdminOnly)
        var registered = _fallbackPolicyProvider.GetPolicyAsync(policyName);
        if (registered != null)
        {
            return registered;
        }

        // Còn lại: coi như mã permission dạng "book.list", "order.process"...
        if (policyName.Contains('.', StringComparison.Ordinal))
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return registered;
    }
}
