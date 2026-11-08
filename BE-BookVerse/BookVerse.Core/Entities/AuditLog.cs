namespace BookVerse.Core.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? ActorUserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Note { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
