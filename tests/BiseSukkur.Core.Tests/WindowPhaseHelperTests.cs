using BiseSukkur.Core.DTOs;
using BiseSukkur.Core.Helpers;
using FluentAssertions;
using Xunit;

namespace BiseSukkur.Core.Tests;

public class WindowPhaseHelperTests
{
    private static readonly TimelineDto Timeline = new()
    {
        PortalOpen = true,
        NormalStart = new DateTime(2026, 1, 1),
        NormalEnd = new DateTime(2026, 1, 31, 23, 59, 59),
        GraceEnd = new DateTime(2026, 2, 7, 23, 59, 59),
        GraceEnabled = true
    };

    [Fact]
    public void ResolvePhase_UsesNormalWithinNormalWindow()
    {
        WindowPhaseHelper.ResolvePhase(new DateTime(2026, 1, 15), Timeline).Should().Be("normal");
    }

    [Fact]
    public void ResolvePhase_UsesGraceAfterNormalWindow()
    {
        WindowPhaseHelper.ResolvePhase(new DateTime(2026, 2, 1), Timeline).Should().Be("grace");
    }

    [Fact]
    public void ResolvePhase_ClosesAfterGraceWindow()
    {
        WindowPhaseHelper.ResolvePhase(new DateTime(2026, 2, 8), Timeline).Should().Be("closed");
    }

    [Fact]
    public void ResolvePhase_ClosesWhenPortalIsDisabled()
    {
        var closedTimeline = new TimelineDto { PortalOpen = false };

        WindowPhaseHelper.ResolvePhase(new DateTime(2026, 1, 15), closedTimeline).Should().Be("closed");
    }

    [Theory]
    [InlineData("normal", "Open (Normal Fee)")]
    [InlineData("grace", "Grace Period (Late Fee Applies)")]
    [InlineData("anything-else", "Closed")]
    public void ResolvePhaseLabel_ReturnsUserFacingText(string phase, string expected)
    {
        WindowPhaseHelper.ResolvePhaseLabel(phase).Should().Be(expected);
    }
}
