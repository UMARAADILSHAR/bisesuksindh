using System.Security.Claims;

namespace BiseSukkur.Core.DTOs;

public static class UserSessionExtensions
{
    public static List<Claim> ToClaims(this UserSessionDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("TenantId", user.TenantId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.GivenName, user.Name),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("RoleName", user.Role.ToString()),
            new("MustChangePassword", user.MustChangePassword.ToString())
            ,new("MfaEnabled", user.MfaEnabled.ToString())
        };

        if (user.Email != null) claims.Add(new Claim(ClaimTypes.Email, user.Email));
        if (user.SchoolId.HasValue) claims.Add(new Claim("SchoolId", user.SchoolId.Value.ToString()));
        if (user.SchoolName != null) claims.Add(new Claim("SchoolName", user.SchoolName));
        if (user.DistrictId.HasValue) claims.Add(new Claim("DistrictId", user.DistrictId.Value.ToString()));
        if (user.DistrictName != null) claims.Add(new Claim("DistrictName", user.DistrictName));

        return claims;
    }

    public static UserSessionDto? FromClaims(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var userId))
        {
            return null;
        }

        var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value;
        if (!Enum.TryParse<Enums.UserRole>(roleClaim, out var role))
        {
            return null;
        }

        int? schoolId = int.TryParse(principal.FindFirst("SchoolId")?.Value, out var sid) ? sid : null;
        int? districtId = int.TryParse(principal.FindFirst("DistrictId")?.Value, out var did) ? did : null;
        int.TryParse(principal.FindFirst("TenantId")?.Value, out var tenantId);
        bool mustChange = bool.TryParse(principal.FindFirst("MustChangePassword")?.Value, out var mcp) && mcp;
        bool mfaEnabled = bool.TryParse(principal.FindFirst("MfaEnabled")?.Value, out var mfa) && mfa;

        return new UserSessionDto
        {
            Id = userId,
            TenantId = tenantId,
            Username = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
            Name = principal.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty,
            Email = principal.FindFirst(ClaimTypes.Email)?.Value,
            Role = role,
            SchoolId = schoolId,
            SchoolName = principal.FindFirst("SchoolName")?.Value,
            DistrictId = districtId,
            DistrictName = principal.FindFirst("DistrictName")?.Value,
            MustChangePassword = mustChange
            ,MfaEnabled = mfaEnabled
        };
    }
}
