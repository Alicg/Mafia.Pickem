using Microsoft.Extensions.Configuration;

namespace MafiaPickem.Api.Overlay;

public sealed class ObsOverlayOptions
{
    public string? PublicBaseUrl { get; init; }

    public static ObsOverlayOptions Create(IConfiguration configuration)
    {
        var publicBaseUrl = Normalize(configuration["ObsOverlayPublicBaseUrl"])
            ?? Normalize(configuration["TelegramMiniAppUrl"]);

        return new ObsOverlayOptions
        {
            PublicBaseUrl = IsAbsoluteHttpUrl(publicBaseUrl) ? publicBaseUrl : null
        };
    }

    public string? BuildTournamentOverlayUrl(int tournamentId)
    {
        if (!Uri.TryCreate(PublicBaseUrl, UriKind.Absolute, out var baseUri))
        {
            return null;
        }

        return new Uri(baseUri, $"/api/overlay/tournaments/{tournamentId}").ToString();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().TrimEnd('/');
    }

    private static bool IsAbsoluteHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}