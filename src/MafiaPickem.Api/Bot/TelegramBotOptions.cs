using Microsoft.Extensions.Configuration;

namespace MafiaPickem.Api.Bot;

public sealed class TelegramBotOptions
{
    public const string DefaultMiniAppButtonText = "Угадайка";
    public const string DefaultMiniAppPromptText = "Нажмите кнопку ниже, чтобы открыть приложение.";

    public string? BotToken { get; init; }
    public string? WebhookUrl { get; init; }
    public string? WebhookSecretToken { get; init; }
    public string? MiniAppUrl { get; init; }
    public string MiniAppButtonText { get; init; } = DefaultMiniAppButtonText;
    public string MiniAppPromptText { get; init; } = DefaultMiniAppPromptText;

    public bool CanRegisterWebhook =>
        !string.IsNullOrWhiteSpace(BotToken) &&
        !string.IsNullOrWhiteSpace(WebhookUrl) &&
        !string.IsNullOrWhiteSpace(WebhookSecretToken);

    public bool CanShowMiniApp =>
        !string.IsNullOrWhiteSpace(BotToken) &&
        !string.IsNullOrWhiteSpace(MiniAppUrl);

    public static TelegramBotOptions Create(IConfiguration configuration)
    {
        var webhookUrl = Normalize(configuration["TelegramWebhookUrl"]);

        return new TelegramBotOptions
        {
            BotToken = Normalize(configuration["TelegramBotToken"]),
            WebhookUrl = webhookUrl,
            WebhookSecretToken = Normalize(configuration["TelegramWebhookSecretToken"]),
            MiniAppUrl = ResolveMiniAppUrl(configuration, webhookUrl),
            MiniAppButtonText = Normalize(configuration["TelegramMiniAppButtonText"]) ?? DefaultMiniAppButtonText,
            MiniAppPromptText = Normalize(configuration["TelegramMiniAppPromptText"]) ?? DefaultMiniAppPromptText,
        };
    }

    private static string? ResolveMiniAppUrl(IConfiguration configuration, string? webhookUrl)
    {
        var configuredUrl = Normalize(configuration["TelegramMiniAppUrl"]);
        if (IsAbsoluteHttpUrl(configuredUrl))
        {
            return configuredUrl;
        }

        if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out var webhookUri))
        {
            return null;
        }

        return webhookUri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }

    private static bool IsAbsoluteHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}