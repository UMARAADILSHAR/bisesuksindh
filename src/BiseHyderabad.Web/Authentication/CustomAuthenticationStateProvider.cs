using BiseHyderabad.Core.DTOs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

namespace BiseHyderabad.Web.Authentication;

public class CustomAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private UserSessionDto? _cachedSession;

    public CustomAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(loggerFactory)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserSessionDto? CurrentUserSession =>
        _cachedSession ?? UserSessionExtensions.FromClaims(_httpContextAccessor.HttpContext?.User);

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(authenticationState.User.Identity?.IsAuthenticated == true);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_cachedSession != null)
        {
            var identity = new ClaimsIdentity(_cachedSession.ToClaims(), CookieAuthenticationDefaults.AuthenticationScheme);
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }

        var httpContext = _httpContextAccessor.HttpContext;
        var user = httpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
        if (user.Identity?.IsAuthenticated == true)
        {
            _cachedSession = UserSessionExtensions.FromClaims(user);
        }
        return Task.FromResult(new AuthenticationState(user));
    }

    public void NotifyUserChanged(UserSessionDto? user)
    {
        _cachedSession = user;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
