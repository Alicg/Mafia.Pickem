using System.Text.Json;
using System.Text.RegularExpressions;

namespace MafiaPickem.Api.Overlay;

public class TournamentOverlaySettings
{
    public string OverlayType { get; set; } = ObsOverlayType.Classic;
    public bool HideBlocksByPhase { get; set; } = true;
    public OverlayThemeSettings Theme { get; set; } = new();
    public OverlayStackPanelLayout LeftPanel { get; set; } = new() { EdgeOffset = 15, TopOffset = 138 };
    public OverlayStackPanelLayout RightPanel { get; set; } = new() { EdgeOffset = 15, TopOffset = 394 };
    public OverlayBlockPlacement SummaryBlock { get; set; } = new() { Panel = OverlayPanelSide.Left };
    public OverlayBlockPlacement FirstVoteBlock { get; set; } = new() { Panel = OverlayPanelSide.Right };
    public OverlayBlockPlacement LastRoundBlock { get; set; } = new() { Panel = OverlayPanelSide.Left };
    public OverlayBlockPlacement FooterBlock { get; set; } = new() { Panel = OverlayPanelSide.Left };
    public ViewerSympathyOverlayBlockSettings ViewerSympathyBlock { get; set; } = new();

    public static TournamentOverlaySettings CreateDefault()
    {
        return Normalize(null);
    }

    public static TournamentOverlaySettings Normalize(TournamentOverlaySettings? settings)
    {
        return new TournamentOverlaySettings
        {
            OverlayType = ObsOverlayType.Normalize(settings?.OverlayType),
            HideBlocksByPhase = settings?.HideBlocksByPhase ?? true,
            Theme = OverlayThemeSettings.Normalize(settings?.Theme),
            LeftPanel = OverlayStackPanelLayout.Normalize(settings?.LeftPanel, 15, 138, settings?.SummaryBlock, settings?.LastRoundBlock, settings?.FooterBlock, settings?.FirstVoteBlock),
            RightPanel = OverlayStackPanelLayout.Normalize(settings?.RightPanel, 15, 394, settings?.FirstVoteBlock, settings?.SummaryBlock, settings?.LastRoundBlock, settings?.FooterBlock),
            SummaryBlock = OverlayBlockPlacement.Normalize(settings?.SummaryBlock, OverlayPanelSide.Left),
            FirstVoteBlock = OverlayBlockPlacement.Normalize(settings?.FirstVoteBlock, OverlayPanelSide.Right),
            LastRoundBlock = OverlayBlockPlacement.Normalize(settings?.LastRoundBlock, OverlayPanelSide.Left),
            FooterBlock = OverlayBlockPlacement.Normalize(settings?.FooterBlock, OverlayPanelSide.Left),
            ViewerSympathyBlock = ViewerSympathyOverlayBlockSettings.Normalize(settings?.ViewerSympathyBlock)
        };
    }
}

public class ViewerSympathyOverlayBlockSettings
{
    public int HorizontalOffset { get; set; }
    public int VerticalOffset { get; set; } = 24;
    public int Scale { get; set; } = 10;

    public static ViewerSympathyOverlayBlockSettings Normalize(ViewerSympathyOverlayBlockSettings? settings)
    {
        return new ViewerSympathyOverlayBlockSettings
        {
            HorizontalOffset = NormalizeOffset(settings?.HorizontalOffset ?? 0),
            VerticalOffset = NormalizeOffset(settings?.VerticalOffset ?? 24),
            Scale = Math.Clamp(settings?.Scale ?? 10, 1, 10)
        };
    }

    private static int NormalizeOffset(int value)
    {
        return Math.Clamp(value, -4000, 4000);
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
    public bool IsVisible { get; set; } = true;
    public OverlayDynamicDisplaySettings DynamicDisplay { get; set; } = new();

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
            Panel = OverlayPanelSide.Normalize(block?.Panel, OverlayPanelSide.Normalize(block?.Side, defaultPanel)),
            IsVisible = block?.IsVisible ?? true,
            DynamicDisplay = OverlayDynamicDisplaySettings.Normalize(block?.DynamicDisplay)
        };
    }
}

public class OverlayDynamicDisplaySettings
{
    public bool Enabled { get; set; }
    public int IntervalSeconds { get; set; } = 30;
    public int VisibleDurationSeconds { get; set; } = 8;

    public static OverlayDynamicDisplaySettings Normalize(OverlayDynamicDisplaySettings? settings)
    {
        var intervalSeconds = Math.Clamp(settings?.IntervalSeconds ?? 30, 1, 3600);
        var visibleDurationSeconds = Math.Clamp(settings?.VisibleDurationSeconds ?? 8, 1, 3600);

        return new OverlayDynamicDisplaySettings
        {
            Enabled = settings?.Enabled ?? false,
            IntervalSeconds = intervalSeconds,
            VisibleDurationSeconds = Math.Min(visibleDurationSeconds, intervalSeconds)
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

public static class ObsOverlayType
{
    public const string Classic = "classic";
    public const string ViewerSympathy = "viewer-sympathy";

    public static string Normalize(string? value)
    {
        return string.Equals(value, ViewerSympathy, StringComparison.OrdinalIgnoreCase)
            ? ViewerSympathy
            : Classic;
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