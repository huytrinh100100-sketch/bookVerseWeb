namespace BookVerse.Core.Models.Role;

public class RoleSearchRequest
{
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class RoleListItemResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int UserCount { get; set; }
    public int PermissionCount { get; set; }
    public IReadOnlyCollection<string> PermissionCodes { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class RoleDetailResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int UserCount { get; set; }
    public int PermissionCount { get; set; }
    public IReadOnlyCollection<string> PermissionCodes { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateRoleRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> PermissionCodes { get; set; } = [];
}

public class UpdateRoleRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDefault { get; set; }
    public bool? IsActive { get; set; }
}

public class AssignPermissionsRequest
{
    public List<string> Permissions { get; set; } = [];
    public List<string> PermissionCodes { get; set; } = [];
}

/// <summary>FE: POST /roles/assign - gan vai tro cho mot tai khoan.</summary>
public class AssignRoleRequest
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}

/// <summary>FE: GET /users/staff.</summary>
public class StaffMemberResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string FullName { get; set; } = string.Empty;
    public IReadOnlyCollection<string> RoleCodes { get; set; } = [];
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public string? LastLoginLocation { get; set; }
    public string Status { get; set; } = "Active";
}
