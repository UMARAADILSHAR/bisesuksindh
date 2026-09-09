using BiseSukkur.Core.Entities;
using BiseSukkur.Core.Enums;
using BiseSukkur.Core.Interfaces;
using BiseSukkur.Infrastructure.Data;
using BiseSukkur.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;

namespace BiseSukkur.Infrastructure.Tests;

public class Phase1SecurityTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task PermissionService_AllowsSchoolPermissionOnlyForAssignedSchool()
    {
        await using var context = CreateDbContext();
        context.Users.Add(new User { Id = 7, Username = "school", Role = UserRole.SchoolAdmin, SchoolId = 12, IsActive = true });
        await context.SaveChangesAsync();
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.UserId).Returns(7);
        var service = new PermissionService(context, currentUser.Object);

        (await service.HasPermissionAsync(Permissions.EnrollmentManage, 12)).Should().BeTrue();
        (await service.HasPermissionAsync(Permissions.EnrollmentManage, 99)).Should().BeFalse();
    }

    [Fact]
    public async Task PermissionService_UsesExplicitDenyGrant()
    {
        await using var context = CreateDbContext();
        context.Users.Add(new User { Id = 7, Username = "admin", Role = UserRole.SuperAdmin, IsActive = true });
        context.PermissionGrants.Add(new PermissionGrant
        {
            UserId = 7,
            Permission = Permissions.UsersManage,
            ScopeType = PermissionScopeType.Board,
            IsAllowed = false
        });
        await context.SaveChangesAsync();
        var service = new PermissionService(context, Mock.Of<ICurrentUserService>());

        (await service.HasPermissionAsync(7, Permissions.UsersManage)).Should().BeFalse();
    }

    [Fact]
    public void MfaService_RejectsMalformedCodes()
    {
        var service = new MfaService();
        var secret = service.GenerateSecret();

        service.VerifyCode(secret, "12").Should().BeFalse();
        service.VerifyCode(secret, "abcdef").Should().BeFalse();
        service.BuildOtpAuthUri("admin@example.com", secret).Should().StartWith("otpauth://totp/");
    }

    [Fact]
    public async Task ApprovalService_PreventsSubmitterFromApprovingOwnRequest()
    {
        await using var context = CreateDbContext();
        var audit = new Mock<ISecurityAuditService>();
        var service = new ApprovalService(context, audit.Object);
        var request = await service.SubmitAsync("result", "Result", "20", "{}", 4, "entry-user");

        (await service.ReviewAsync(request.Id, ApprovalStatus.Approved, 4, "entry-user")).Should().BeFalse();
        (await context.ApprovalRequests.SingleAsync()).Status.Should().Be(ApprovalStatus.Pending);
    }
}
