namespace BookVerse.Core.Models.AuditLog;

public class AuditLogSearchRequest
{
    public string? Keyword { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? Module { get; set; }
    public string? EntityType { get; set; }
    public bool? IsSuccess { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AuditLogListItemResponse
{
    public Guid Id { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? ActorUserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Note { get; set; }
    public bool IsSuccess { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuditLogDetailResponse
{
    public Guid Id { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? ActorUserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Note { get; set; }
    public bool IsSuccess { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
