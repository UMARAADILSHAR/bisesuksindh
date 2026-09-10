using BiseSukkur.Core.DTOs;
using BiseSukkur.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BiseSukkur.Web.Authentication;

public class CustomAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    private UserSessionDto? _cachedSession;

    public CustomAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory scopeFactory,
        ILogger<CustomAuthenticationStateProvider> logger)
        : base(loggerFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public UserSessionDto? CurrentUserSession
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                return httpContext.User?.Identity?.IsAuthenticated == true
                    ? UserSessionExtensions.FromClaims(httpContext.User)
                    : null;
            }
            return _cachedSession;
        }
    }

    /// <summary>
    /// Revalidate every 2 minutes — check the database to ensure the user
    /// is still active, not locked out, and has the same role as stored in the cookie.
    /// </summary>
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(2);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        var principal = authenticationState.User;
        if (principal.Identity?.IsAuthenticated != true)
        {
            ClearCachedSession();
            return false;
        }

        // Extract user ID and role from the cookie claims
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(userIdClaim, out var userId) || string.IsNullOrEmpty(roleClaim))
        {
            _logger.LogWarning("Revalidation failed: missing NameIdentifier or Role claim.");
            ClearCachedSession();
            return false;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = await db.Users
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(u => u.Id == userId)
                .Select(u => new { u.IsActive, u.Role, u.LockoutEnd })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Revalidation failed: user {UserId} not found in database.", userId);
                ClearCachedSession();
                return false;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Revalidation failed: user {UserId} is deactivated.", userId);
                ClearCachedSession();
                return false;
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                _logger.LogWarning("Revalidation failed: user {UserId} is locked out until {LockoutEnd}.", userId, user.LockoutEnd);
                ClearCachedSession();
                return false;
            }

            // Verify role hasn't changed in the database
            if (user.Role.ToString() != roleClaim)
            {
                _logger.LogWarning("Revalidation failed: user {UserId} role changed from {CookieRole} to {DbRole}.", userId, roleClaim, user.Role);
                ClearCachedSession();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication revalidation for user {UserId}.", userId);
            // On DB errors, allow the session to continue rather than kicking out all users
            return true;
        }
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var user = httpContext.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                _cachedSession = UserSessionExtensions.FromClaims(user);
                return Task.FromResult(new AuthenticationState(user));
            }
            else
            {
                _cachedSession = null;
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }
        }

        if (_cachedSession != null)
        {
            var identity = new ClaimsIdentity(_cachedSession.ToClaims(), CookieAuthenticationDefaults.AuthenticationScheme);
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }

    public void NotifyUserChanged(UserSessionDto? user)
    {
        _cachedSession = user;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void ClearCachedSession()
    {
        _cachedSession = null;
    }
}
