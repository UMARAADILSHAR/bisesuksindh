using BiseHyderabad.Core.Enums;

namespace BiseHyderabad.Core.Entities;

public class PermissionGrant
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string Permission { get; set; } = string.Empty;
    public PermissionScopeType ScopeType { get; set; } = PermissionScopeType.Board;
    public int? ScopeId { get; set; }
    public bool IsAllowed { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
}

public class ApprovalRequest
{
    public long Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public int SubmittedByUserId { get; set; }
    public string SubmittedByUsername { get; set; } = string.Empty;
    public int? ReviewedByUserId { get; set; }
    public string? ReviewedByUsername { get; set; }
    public string? ReviewComment { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}

public class SecurityAuditEvent
{
    public long Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
    public string? DetailsJson { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
