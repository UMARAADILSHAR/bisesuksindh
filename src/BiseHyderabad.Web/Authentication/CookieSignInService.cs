using BiseHyderabad.Core.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace BiseHyderabad.Web.Authentication;

public interface ICookieSignInService
{
    Task SignInAsync(UserSessionDto user, bool rememberMe = false);
    Task SignOutAsync();
    Task RefreshAsync(UserSessionDto user);
}

public class CookieSignInService : ICookieSignInService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    public CookieSignInService(
        IHttpContextAccessor httpContextAccessor,
        CustomAuthenticationStateProvider authStateProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _authStateProvider = authStateProvider;
    }

    public async Task SignInAsync(UserSessionDto user, bool rememberMe = false)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && !httpContext.Response.HasStarted)
            {
                var identity = new ClaimsIdentity(user.ToClaims(), CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    AllowRefresh = true,
                    ExpiresUtc = rememberMe
                        ? DateTimeOffset.UtcNow.AddDays(14)
                        : DateTimeOffset.UtcNow.AddHours(8)
                };

                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProperties);
            }
        }
        catch
        {
            // Fallback for interactive server circuits where response has already started
        }

        _authStateProvider.NotifyUserChanged(user);
    }

    public async Task SignOutAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && !httpContext.Response.HasStarted)
            {
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
        catch
        {
            // Circuit cleanup
        }

        _authStateProvider.NotifyUserChanged(null);
    }

    public async Task RefreshAsync(UserSessionDto user)
    {
        await SignInAsync(user, rememberMe: false);
    }
}
