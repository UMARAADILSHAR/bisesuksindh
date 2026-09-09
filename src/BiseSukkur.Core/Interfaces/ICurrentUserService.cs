using BiseSukkur.Core.Enums;

namespace BiseSukkur.Core.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    int? TenantId { get; }
    UserRole? Role { get; }
    int? SchoolId { get; }
    int? DistrictId { get; }
    bool IsAuthenticated { get; }
    bool IsSuperAdmin { get; }
    bool IsPlatformAdministrator { get; }
    bool IsDistrictAdmin { get; }
    bool IsSchoolAdmin { get; }
}
