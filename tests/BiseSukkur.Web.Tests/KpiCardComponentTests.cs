using BiseSukkur.Web.Components.Shared.UI;
using Bunit;
using FluentAssertions;
using Xunit;

namespace BiseSukkur.Web.Tests;

public class KpiCardComponentTests : TestContext
{
    [Fact]
    public void KpiCard_RendersTitleAndValueProperly()
    {
        var cut = RenderComponent<KpiCard>(parameters => parameters
            .Add(p => p.Title, "Active Candidates")
            .Add(p => p.Value, "1,250")
            .Add(p => p.Subtitle, "Verified by Board")
            .Add(p => p.Variant, "emerald")
        );

        cut.Find(".kpi-label").TextContent.Should().Be("Active Candidates");
        cut.Find(".kpi-value").TextContent.Trim().Should().Be("1,250");
        cut.Find(".kpi-subtext").TextContent.Should().Be("Verified by Board");
        cut.Find(".kpi-card").ClassList.Should().Contain("kpi-card-emerald");
    }

    [Fact]
    public void PageHeader_RendersTitleAndActionButtons()
    {
        var cut = RenderComponent<PageHeader>(parameters => parameters
            .Add(p => p.Title, "School Portal")
            .Add(p => p.Description, "Manage students and fees")
            .Add(p => p.Actions, "<button id='btn-test'>Add Candidate</button>")
        );

        cut.Find(".page-title").TextContent.Should().Be("School Portal");
        cut.Find(".page-description").TextContent.Should().Be("Manage students and fees");
        cut.Find("#btn-test").TextContent.Should().Be("Add Candidate");
    }
}
