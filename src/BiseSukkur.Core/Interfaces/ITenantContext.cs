namespace BiseSukkur.Core.Interfaces;

public interface ITenantContext
{
    int? TenantId { get; }
    bool IsPlatformAdministrator { get; }
    bool HasTenant { get; }
}
