using System.Text.Json;
using System.Text.RegularExpressions;

namespace MafiaPickem.Api.Overlay;

public class TournamentOverlaySettings
{
    public bool HideBlocksByPhase { get; set; } = true;
    public OverlayThemeSettings Theme { get; set; } = new();
    public OverlayBlockLayout SummaryBlock { get; set; } = new() { Side = OverlayBlockSide.Left, EdgeOffset = 15, TopOffset = 138 };
    public OverlayBlockLayout FirstVoteBlock { get; set; } = new() { Side = OverlayBlockSide.Right, EdgeOffset = 15, TopOffset = 394 };
    public OverlayBlockLayout LastRoundBlock { get; set; } = new() { Side = OverlayBlockSide.Left, EdgeOffset = 15, TopOffset = 394 };
    public OverlayBlockLayout FooterBlock { get; set; } = new() { Side = OverlayBlockSide.Left, EdgeOffset = 15, TopOffset = 736 };

    public static TournamentOverlaySettings CreateDefault()
    {
        return Normalize(null);
    }

    public static TournamentOverlaySettings Normalize(TournamentOverlaySettings? settings)
    {
        return new TournamentOverlaySettings
        {
            HideBlocksByPhase = settings?.HideBlocksByPhase ?? true,
            Theme = OverlayThemeSettings.Normalize(settings?.Theme),
            SummaryBlock = OverlayBlockLayout.Normalize(settings?.SummaryBlock, OverlayBlockSide.Left, 15, 138),
            FirstVoteBlock = OverlayBlockLayout.Normalize(settings?.FirstVoteBlock, OverlayBlockSide.Right, 15, 394),
            LastRoundBlock = OverlayBlockLayout.Normalize(settings?.LastRoundBlock, OverlayBlockSide.Left, 15, 394),
            FooterBlock = OverlayBlockLayout.Normalize(settings?.FooterBlock, OverlayBlockSide.Left, 15, 736)
        };
    }
}

public class OverlayBlockLayout
{
    public string Side { get; set; } = OverlayBlockSide.Left;
    public int EdgeOffset { get; set; }
    public int TopOffset { get; set; }

    public static OverlayBlockLayout Normalize(OverlayBlockLayout? block, string defaultSide, int defaultEdgeOffset, int defaultTopOffset)
    {
        return new OverlayBlockLayout
        {
            Side = OverlayBlockSide.Normalize(block?.Side, defaultSide),
            EdgeOffset = NormalizeOffset(block?.EdgeOffset ?? defaultEdgeOffset),
            TopOffset = NormalizeOffset(block?.TopOffset ?? defaultTopOffset)
        };
    }

    private static int NormalizeOffset(int value)
    {
        return Math.Clamp(value, 0, 4000);
    }
}

public class OverlayThemeSettings
{
    private static readonly Regex HexColorRegex = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    public string FillColorStart { get; set; } = "#163A61";
    public string FillColorEnd { get; set; } = "#0B1F3A";
    public int FillOpacity { get; set; } = 92;
    public bool UseGradient { get; set; } = true;

    public static OverlayThemeSettings Normalize(OverlayThemeSettings? theme)
    {
        return new OverlayThemeSettings
        {
            FillColorStart = NormalizeColor(theme?.FillColorStart, "#163A61"),
            FillColorEnd = NormalizeColor(theme?.FillColorEnd, "#0B1F3A"),
            FillOpacity = Math.Clamp(theme?.FillOpacity ?? 92, 0, 100),
            UseGradient = theme?.UseGradient ?? true
        };
    }

    private static string NormalizeColor(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var trimmed = value.Trim();
        return HexColorRegex.IsMatch(trimmed) ? trimmed.ToUpperInvariant() : fallback;
    }
}

public static class OverlayBlockSide
{
    public const string Left = "left";
    public const string Right = "right";

    public static string Normalize(string? value, string fallback)
    {
        return string.Equals(value, Right, StringComparison.OrdinalIgnoreCase)
            ? Right
            : string.Equals(value, Left, StringComparison.OrdinalIgnoreCase)
                ? Left
                : fallback;
    }
}

public static class TournamentOverlaySettingsSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static TournamentOverlaySettings Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return TournamentOverlaySettings.CreateDefault();
        }

        try
        {
            return TournamentOverlaySettings.Normalize(JsonSerializer.Deserialize<TournamentOverlaySettings>(json, SerializerOptions));
        }
        catch (JsonException)
        {
            return TournamentOverlaySettings.CreateDefault();
        }
    }

    public static string Serialize(TournamentOverlaySettings? settings)
    {
        return JsonSerializer.Serialize(TournamentOverlaySettings.Normalize(settings), SerializerOptions);
    }
}