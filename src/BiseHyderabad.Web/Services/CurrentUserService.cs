using BiseHyderabad.Core.DTOs;
using BiseHyderabad.Core.Enums;
using BiseHyderabad.Core.Interfaces;
using BiseHyderabad.Web.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BiseHyderabad.Web.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CustomAuthenticationStateProvider _authenticationStateProvider;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        CustomAuthenticationStateProvider authenticationStateProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _authenticationStateProvider = authenticationStateProvider;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;
    // HttpContext is not guaranteed to be available during an interactive
    // Blazor Server event. The authentication-state provider retains the
    // claims-backed session for that circuit after cookie sign-in.
    private UserSessionDto? Session => UserSessionExtensions.FromClaims(Principal)
        ?? _authenticationStateProvider.CurrentUserSession;

    public int? UserId => Session?.Id;
    public string? Username => Session?.Username;
    public int? TenantId => Session?.TenantId > 0 ? Session.TenantId : null;
    public UserRole? Role => Session?.Role;
    public int? SchoolId => Session?.SchoolId;
    public int? DistrictId => Session?.DistrictId;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true || Session != null;
    public bool IsSuperAdmin => Role == UserRole.SuperAdmin;
    public bool IsPlatformAdministrator => IsSuperAdmin;
    public bool IsDistrictAdmin => Role == UserRole.DistrictAdmin;
    public bool IsSchoolAdmin => Role == UserRole.SchoolAdmin;
}
