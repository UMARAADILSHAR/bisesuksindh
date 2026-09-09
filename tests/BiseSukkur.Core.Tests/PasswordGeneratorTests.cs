using BiseSukkur.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace BiseSukkur.Core.Tests;

public class PasswordGeneratorTests
{
    [Fact]
    public void GenerateTemporaryPassword_EnforcesMinimumLengthAndCharacterClasses()
    {
        var password = PasswordGenerator.GenerateTemporaryPassword(8);

        password.Should().HaveLength(12);
        password.Any(char.IsUpper).Should().BeTrue();
        password.Any(char.IsLower).Should().BeTrue();
        password.Any(char.IsDigit).Should().BeTrue();
        password.Any(c => "!@#$%&*".Contains(c)).Should().BeTrue();
    }

    [Fact]
    public void GenerateTemporaryPassword_HonoursLongerRequestedLength()
    {
        PasswordGenerator.GenerateTemporaryPassword(18).Should().HaveLength(18);
    }
}
