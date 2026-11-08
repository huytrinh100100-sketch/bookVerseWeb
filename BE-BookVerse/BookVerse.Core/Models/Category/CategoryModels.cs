namespace BookVerse.Core.Models.Category;

public class CategorySearchRequest
{
    public string? Keyword { get; set; }
    public string? RootCode { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CategoryListItemResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public Guid RootId { get; set; }
    public string RootCode { get; set; } = string.Empty;
    public string RootName { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? TypeLabel { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsMegaMenu { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public class CategoryDetailResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public Guid RootId { get; set; }
    public string? RootCode { get; set; }
    public string? RootName { get; set; }
    public Guid? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsMegaMenu { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public class CategoryTreeNodeResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public Guid RootId { get; set; }
    public string RootCode { get; set; } = string.Empty;
    public string RootName { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? TypeLabel { get; set; }
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsMegaMenu { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int BookCount { get; set; }
    public IReadOnlyCollection<CategoryTreeNodeResponse> Children { get; set; } = [];
}

public class CategoryRootResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class CreateCategoryRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public Guid? ParentId { get; set; }
    public string? RootCode { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsMegaMenu { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCategoryRequest
{
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public int? SortOrder { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool? IsMegaMenu { get; set; }
    public bool? IsActive { get; set; }
}

public class MoveCategoryRequest
{
    public Guid? ParentId { get; set; }
    public int? SortOrder { get; set; }
}

public class UpdateCategoryStatusRequest
{
    public bool IsActive { get; set; }
}

public class ReorderCategoriesRequest
{
    public List<CategoryOrderItem> Items { get; set; } = [];
}

public class CategoryOrderItem
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
}
