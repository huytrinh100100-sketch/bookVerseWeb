namespace BookVerse.Core.Entities;

/// <summary>
/// Danh mục động dùng chung. Mỗi cây danh mục có một node gốc (ParentId = null),
/// RootId của node gốc trỏ về chính nó, node con thừa kế RootId của cha.
/// Mã duy nhất theo (RootId, Code).
/// </summary>
public class CommonCategory
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public Guid? ParentId { get; set; }
    public Guid RootId { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsMegaMenu { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public CommonCategory? Parent { get; set; }
    public CommonCategory? Root { get; set; }
    public ICollection<CommonCategory> Children { get; set; } = [];
    public ICollection<CommonCategory> RootItems { get; set; } = [];
}
