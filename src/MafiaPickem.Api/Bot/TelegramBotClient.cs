using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace MafiaPickem.Api.Bot;

public sealed class TelegramBotClient : ITelegramBotClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TelegramBotClient> _logger;

    public TelegramBotClient(
        IHttpClientFactory httpClientFactory,
        TelegramBotOptions options,
        ILogger<TelegramBotClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        Options = options;
    }

    public TelegramBotOptions Options { get; }

    public Task<bool> RegisterWebhookAsync(CancellationToken cancellationToken)
    {
        if (!Options.CanRegisterWebhook)
        {
            _logger.LogInformation("Skipping Telegram webhook registration because bot token, webhook URL, or secret token is missing.");
            return Task.FromResult(false);
        }

        return PostTelegramMethodAsync(
            "setWebhook",
            new SetWebhookRequest(Options.WebhookUrl!, Options.WebhookSecretToken!, false),
            "register Telegram webhook",
            cancellationToken);
    }

    public Task<bool> ConfigureMiniAppMenuButtonAsync(CancellationToken cancellationToken)
    {
        if (!Options.CanShowMiniApp)
        {
            _logger.LogInformation("Skipping Telegram mini app menu button configuration because bot token or mini app URL is missing.");
            return Task.FromResult(false);
        }

        return PostTelegramMethodAsync(
            "setChatMenuButton",
            new SetChatMenuButtonRequest(new MenuButtonWebApp("web_app", Options.MiniAppButtonText, new WebAppInfo(Options.MiniAppUrl!))),
            "configure Telegram mini app menu button",
            cancellationToken);
    }

    public Task<bool> SendMiniAppPromptAsync(long chatId, CancellationToken cancellationToken)
    {
        if (!Options.CanShowMiniApp)
        {
            _logger.LogInformation("Skipping Telegram mini app prompt because bot token or mini app URL is missing.");
            return Task.FromResult(false);
        }

        return PostTelegramMethodAsync(
            "sendMessage",
            new SendMessageRequest(
                chatId,
                Options.MiniAppPromptText,
                new InlineKeyboardMarkup(
                    [
                        [
                            new InlineKeyboardButton(
                                Options.MiniAppButtonText,
                                new WebAppInfo(Options.MiniAppUrl!))
                        ]
                    ])),
            $"send mini app prompt to chat {chatId}",
            cancellationToken);
    }

    private async Task<bool> PostTelegramMethodAsync(string methodName, object payload, string actionDescription, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Options.BotToken))
        {
            _logger.LogInformation("Skipping Telegram API call {MethodName} because bot token is missing.", methodName);
            return false;
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient(nameof(TelegramBotClient));
            using var response = await httpClient.PostAsJsonAsync(
                $"https://api.telegram.org/bot{Options.BotToken}/{methodName}",
                payload,
                JsonSerializerOptions,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Failed to {ActionDescription}. Status: {StatusCode}. Response: {ResponseBody}",
                    actionDescription,
                    response.StatusCode,
                    responseBody);
                return false;
            }

            var telegramResponse = JsonSerializer.Deserialize<TelegramApiResponse>(responseBody, JsonSerializerOptions);
            if (telegramResponse?.Ok == true)
            {
                _logger.LogInformation("Successfully completed Telegram API call: {ActionDescription}", actionDescription);
                return true;
            }

            _logger.LogError(
                "Telegram API returned an unexpected response while trying to {ActionDescription}: {ResponseBody}",
                actionDescription,
                responseBody);
            return false;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Telegram API call canceled while trying to {ActionDescription}.", actionDescription);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while trying to {ActionDescription}.", actionDescription);
            return false;
        }
    }

    private sealed record SetWebhookRequest(
        string Url,
        string SecretToken,
        bool DropPendingUpdates);

    private sealed record SetChatMenuButtonRequest(
        MenuButtonWebApp MenuButton);

    private sealed record SendMessageRequest(
        long ChatId,
        string Text,
        InlineKeyboardMarkup ReplyMarkup);

    private sealed record InlineKeyboardMarkup(
        IReadOnlyList<IReadOnlyList<InlineKeyboardButton>> InlineKeyboard);

    private sealed record InlineKeyboardButton(
        string Text,
        WebAppInfo WebApp);

    private sealed record MenuButtonWebApp(
        string Type,
        string Text,
        WebAppInfo WebApp);

    private sealed record WebAppInfo(string Url);

    private sealed record TelegramApiResponse(
        bool Ok,
        string? Description);
}