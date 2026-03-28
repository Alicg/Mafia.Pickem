using FluentAssertions;
using MafiaPickem.Api.Overlay;

namespace MafiaPickem.Api.Tests.Overlay;

public class ObsViewerSympathyHtmlRendererTests
{
    [Fact]
    public void Render_ShouldSubstituteTemplatePlaceholders()
    {
        var html = ObsViewerSympathyHtmlRenderer.Render(123);

        html.Should().Contain("const tournamentId = 123;");
        html.Should().Contain("Зрительские симпатии");
        html.Should().NotContain("{{tournamentId}}");
        html.Should().NotContain("{{fontBase64}}");
        html.Should().Contain("data:font/otf;base64,");
    }
}