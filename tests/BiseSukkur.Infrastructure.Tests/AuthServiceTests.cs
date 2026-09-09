using BiseSukkur.Core.DTOs;
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

public class AuthServiceTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task LoginAsync_LocksAccountInDatabase_AfterFiveConsecutiveFailures()
    {
        using var context = CreateDbContext();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var mockActivityLog = new Mock<IActivityLogService>();
        var mockIpRateLimit = new Mock<IIpRateLimitService>();
        mockIpRateLimit.Setup(r => r.IsRateLimited(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>())).Returns(false);

        var authService = new AuthService(context, mockActivityLog.Object, memoryCache, mockIpRateLimit.Object);

        var passwordHash = authService.HashPassword("CorrectPassword123!");
        var user = new User
        {
            Username = "testschooladmin",
            PasswordHash = passwordHash,
            Name = "Test Admin",
            Role = UserRole.SchoolAdmin,
            IsActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // 5 consecutive failed attempts with wrong password
        for (int i = 1; i <= 5; i++)
        {
            var response = await authService.LoginAsync(new LoginRequest { Username = "testschooladmin", Password = "WrongPassword!" });
            response.Success.Should().BeFalse();
        }

        // Verify database state: user should have 5 failed attempts and active LockoutEnd
        var dbUser = await context.Users.FirstAsync(u => u.Username == "testschooladmin");
        dbUser.FailedLoginAttempts.Should().Be(5);
        dbUser.LockoutEnd.Should().NotBeNull();
        dbUser.LockoutEnd!.Value.Should().BeAfter(DateTime.UtcNow);

        // Next login attempt (even with correct password) should be locked out
        var lockedResponse = await authService.LoginAsync(new LoginRequest { Username = "testschooladmin", Password = "CorrectPassword123!" });
        lockedResponse.Success.Should().BeFalse();
        lockedResponse.ErrorMessage.Should().Contain("locked");
    }

    [Fact]
    public async Task LoginAsync_ResetsFailedAttemptsOnSuccessfulLogin()
    {
        using var context = CreateDbContext();
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var mockActivityLog = new Mock<IActivityLogService>();
        var mockIpRateLimit = new Mock<IIpRateLimitService>();
        mockIpRateLimit.Setup(r => r.IsRateLimited(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>())).Returns(false);

        var authService = new AuthService(context, mockActivityLog.Object, memoryCache, mockIpRateLimit.Object);

        var passwordHash = authService.HashPassword("CorrectPassword123!");
        var user = new User
        {
            Username = "testuser",
            PasswordHash = passwordHash,
            Name = "Test User",
            Role = UserRole.SchoolAdmin,
            IsActive = true,
            FailedLoginAttempts = 2
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var response = await authService.LoginAsync(new LoginRequest { Username = "testuser", Password = "CorrectPassword123!" });

        response.Success.Should().BeTrue();
        var dbUser = await context.Users.FirstAsync(u => u.Username == "testuser");
        dbUser.FailedLoginAttempts.Should().Be(0);
        dbUser.LockoutEnd.Should().BeNull();
        dbUser.LastLoginAt.Should().NotBeNull();
    }
}
