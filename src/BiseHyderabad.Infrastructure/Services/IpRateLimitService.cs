using BiseHyderabad.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace BiseHyderabad.Infrastructure.Services;

public class IpRateLimitService : IIpRateLimitService
{
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IpRateLimitService(IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
    {
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsRateLimited(string policyName, int permitLimit, TimeSpan window)
    {
        var clientKey = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var cacheKey = $"rate:{policyName}:{clientKey}";

        var count = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = window;
            return 0;
        });

        count++;
        _cache.Set(cacheKey, count, window);

        return count > permitLimit;
    }
}
