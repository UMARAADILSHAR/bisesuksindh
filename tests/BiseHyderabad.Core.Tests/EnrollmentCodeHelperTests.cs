using BiseHyderabad.Core.Entities;
using BiseHyderabad.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace BiseHyderabad.Core.Tests;

public class EnrollmentCodeHelperTests
{
    [Theory]
    [InlineData("Science", "S")]
    [InlineData("Pre Engineering", "P")]
    [InlineData("pre_medical", "M")]
    [InlineData("General Private", "G")]
    [InlineData("Commerce", "C")]
    public void ResolveGroupCode_ReturnsConfiguredCode(string group, string expected)
    {
        EnrollmentCodeHelper.ResolveGroupCode(group).Should().Be(expected);
    }

    [Fact]
    public void ResolveGroupCode_UsesFirstLetterForAnUnknownGroup()
    {
        EnrollmentCodeHelper.ResolveGroupCode("Vocational").Should().Be("V");
    }

    [Fact]
    public void ResolveGroupCode_UsesSafeDefaultForEmptyGroup()
    {
        EnrollmentCodeHelper.ResolveGroupCode(" ").Should().Be("S");
    }

    [Fact]
    public void ResolveDistrictCode_UsesConfiguredShortCode()
    {
        var district = new District { ShortCode = "kp" };

        EnrollmentCodeHelper.ResolveDistrictCode(district).Should().Be("KP");
    }

    [Fact]
    public void ResolveDistrictCode_UsesHyderabadForMissingDistrict()
    {
        EnrollmentCodeHelper.ResolveDistrictCode(null).Should().Be("HY");
    }

    [Fact]
    public void FormatEnrollmentNumber_UsesStableBoardFormat()
    {
        var result = EnrollmentCodeHelper.FormatEnrollmentNumber("26", "S", "HY", 1, "7", 42);

        result.Should().Be("E26SHY1-007-0042");
    }
}
