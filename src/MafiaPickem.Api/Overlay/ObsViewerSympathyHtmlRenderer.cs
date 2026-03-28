using System.Globalization;

namespace MafiaPickem.Api.Overlay;

public static class ObsViewerSympathyHtmlRenderer
{
    private static readonly Lazy<string> FontBase64 = new(() =>
    {
        using var stream = typeof(ObsViewerSympathyHtmlRenderer).Assembly
            .GetManifestResourceStream("Actay-Regular.otf")!;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return Convert.ToBase64String(ms.ToArray());
    });

    private static readonly Lazy<string> Template = new(() => ReadResource("ObsViewerSympathyTemplate.html"));

    public static string Render(int tournamentId)
    {
        return Template.Value
            .Replace("{{fontBase64}}", FontBase64.Value, StringComparison.Ordinal)
            .Replace("{{tournamentId}}", tournamentId.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    private static string ReadResource(string logicalName)
    {
        using var stream = typeof(ObsViewerSympathyHtmlRenderer).Assembly
            .GetManifestResourceStream(logicalName)
            ?? throw new InvalidOperationException($"Embedded resource '{logicalName}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}