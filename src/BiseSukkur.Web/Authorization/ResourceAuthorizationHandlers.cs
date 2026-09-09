using BiseSukkur.Core.Entities;
using BiseSukkur.Core.Enums;
using BiseSukkur.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BiseSukkur.Web.Authorization;

public class ResourceAuthorizationRequirement : IAuthorizationRequirement
{
    public string OperationName { get; }

    public ResourceAuthorizationRequirement(string operationName)
    {
        OperationName = operationName;
    }
}

public class EnrollmentAuthorizationHandler : AuthorizationHandler<ResourceAuthorizationRequirement, Enrollment>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceAuthorizationRequirement requirement,
        Enrollment resource)
    {
        if (context.User.IsInRole("SuperAdmin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (!int.TryParse(context.User.FindFirst("TenantId")?.Value, out var tenantId)
            || resource.TenantId != tenantId)
        {
            return Task.CompletedTask;
        }

        var schoolIdClaim = context.User.FindFirst("SchoolId")?.Value;
        if (int.TryParse(schoolIdClaim, out var schoolId) && resource.SchoolId == schoolId)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var districtIdClaim = context.User.FindFirst("DistrictId")?.Value;
        if (int.TryParse(districtIdClaim, out var districtId) && resource.School?.DistrictId == districtId)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

public class InvoiceAuthorizationHandler : AuthorizationHandler<ResourceAuthorizationRequirement, Invoice>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceAuthorizationRequirement requirement,
        Invoice resource)
    {
        if (context.User.IsInRole("SuperAdmin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (!int.TryParse(context.User.FindFirst("TenantId")?.Value, out var tenantId)
            || resource.TenantId != tenantId)
        {
            return Task.CompletedTask;
        }

        var schoolIdClaim = context.User.FindFirst("SchoolId")?.Value;
        if (int.TryParse(schoolIdClaim, out var schoolId) && resource.SchoolId == schoolId)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var districtIdClaim = context.User.FindFirst("DistrictId")?.Value;
        if (int.TryParse(districtIdClaim, out var districtId) && resource.School?.DistrictId == districtId)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
