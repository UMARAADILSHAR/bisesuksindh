namespace BiseSukkur.Core.Interfaces;

public interface IIpRateLimitService
{
    /// <summary>
    /// Returns true when the caller should be rejected (rate limit exceeded).
    /// </summary>
    bool IsRateLimited(string policyName, int permitLimit, TimeSpan window);
}
