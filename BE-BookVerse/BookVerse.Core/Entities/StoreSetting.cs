namespace BookVerse.Core.Entities;

/// <summary>Cấu hình chung của cửa hàng (key-value có kiểm soát).</summary>
public class StoreSetting
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Description { get; set; }
    public string GroupKey { get; set; } = "general";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
