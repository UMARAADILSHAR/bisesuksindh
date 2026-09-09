using BiseSukkur.Core.Interfaces;

namespace BiseSukkur.Web.Services;

public sealed class TenantContext : ITenantContext
{
    private readonly ICurrentUserService _currentUser;

    public TenantContext(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public int? TenantId => _currentUser.TenantId;
    public bool IsPlatformAdministrator => _currentUser.IsPlatformAdministrator;
    public bool HasTenant => TenantId.HasValue;
}
