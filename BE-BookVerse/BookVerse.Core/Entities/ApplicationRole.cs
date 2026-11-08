using Microsoft.AspNetCore.Identity;

namespace BookVerse.Core.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>Mã vai trò dùng cho FE (ví dụ: admin, warehouse-manager).</summary>
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
