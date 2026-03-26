using System.Text.Json;
using System.Text.RegularExpressions;

namespace MafiaPickem.Api.Overlay;

public class TournamentOverlaySettings
{
    public bool HideBlocksByPhase { get; set; } = true;
    public OverlayThemeSettings Theme { get; set; } = new();
    public OverlayStackPanelLayout LeftPanel { get; set; } = new() { EdgeOffset = 15, TopOffset = 138 };
    public OverlayStackPanelLayout RightPanel { get; set; } = new() { EdgeOffset = 15, TopOffset = 394 };
    public OverlayBlockPlacement SummaryBlock { get; set; } = new() { Panel = OverlayPanelSide.Left };
    public OverlayBlockPlacement FirstVoteBlock { get; set; } = new() { Panel = OverlayPanelSide.Right };
    public OverlayBlockPlacement LastRoundBlock { get; set; } = new() { Panel = OverlayPanelSide.Left };
    public OverlayBlockPlacement FooterBlock { get; set; } = new() { Panel = OverlayPanelSide.Left };

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
            LeftPanel = OverlayStackPanelLayout.Normalize(settings?.LeftPanel, 15, 138, settings?.SummaryBlock, settings?.LastRoundBlock, settings?.FooterBlock, settings?.FirstVoteBlock),
            RightPanel = OverlayStackPanelLayout.Normalize(settings?.RightPanel, 15, 394, settings?.FirstVoteBlock, settings?.SummaryBlock, settings?.LastRoundBlock, settings?.FooterBlock),
            SummaryBlock = OverlayBlockPlacement.Normalize(settings?.SummaryBlock, OverlayPanelSide.Left),
            FirstVoteBlock = OverlayBlockPlacement.Normalize(settings?.FirstVoteBlock, OverlayPanelSide.Right),
            LastRoundBlock = OverlayBlockPlacement.Normalize(settings?.LastRoundBlock, OverlayPanelSide.Left),
            FooterBlock = OverlayBlockPlacement.Normalize(settings?.FooterBlock, OverlayPanelSide.Left)
        };
    }
}

public class OverlayStackPanelLayout
{
    public int EdgeOffset { get; set; }
    public int TopOffset { get; set; }

    public static OverlayStackPanelLayout Normalize(OverlayStackPanelLayout? panel, int defaultEdgeOffset, int defaultTopOffset, params OverlayBlockPlacement?[] legacyBlocks)
    {
        var legacyPosition = GetLegacyPosition(legacyBlocks);

        return new OverlayStackPanelLayout
        {
            EdgeOffset = NormalizeOffset(panel?.EdgeOffset ?? legacyPosition.EdgeOffset ?? defaultEdgeOffset),
            TopOffset = NormalizeOffset(panel?.TopOffset ?? legacyPosition.TopOffset ?? defaultTopOffset)
        };
    }

    private static (int? EdgeOffset, int? TopOffset) GetLegacyPosition(IEnumerable<OverlayBlockPlacement?> legacyBlocks)
    {
        foreach (var block in legacyBlocks)
        {
            if (block?.LegacyEdgeOffset is int edgeOffset || block?.LegacyTopOffset is int topOffset)
            {
                return (block?.LegacyEdgeOffset, block?.LegacyTopOffset);
            }
        }

        return (null, null);
    }

    private static int NormalizeOffset(int value)
    {
        return Math.Clamp(value, 0, 4000);
    }
}

public class OverlayBlockPlacement
{
    public string Panel { get; set; } = OverlayPanelSide.Left;

    // Legacy properties kept for backward compatibility with already stored JSON.
    public string? Side { get; set; }
    public int? EdgeOffset { get; set; }
    public int? TopOffset { get; set; }
    public bool? PlaceBelowPrevious { get; set; }

    public int? LegacyEdgeOffset => EdgeOffset;
    public int? LegacyTopOffset => TopOffset;

    public static OverlayBlockPlacement Normalize(OverlayBlockPlacement? block, string defaultPanel)
    {
        return new OverlayBlockPlacement
        {
            Panel = OverlayPanelSide.Normalize(block?.Panel, OverlayPanelSide.Normalize(block?.Side, defaultPanel))
        };
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

public static class OverlayPanelSide
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