using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MafiaPickem.Api.Bot;

public sealed class TelegramWebhookRegistrationService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramWebhookRegistrationService> _logger;

    public TelegramWebhookRegistrationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<TelegramWebhookRegistrationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var botToken = _configuration["TelegramBotToken"];
        var secretToken = _configuration["TelegramWebhookSecretToken"];
        var webhookUrl = _configuration["TelegramWebhookUrl"];

        if (string.IsNullOrWhiteSpace(botToken) ||
            string.IsNullOrWhiteSpace(secretToken) ||
            string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogInformation(
                "Skipping Telegram webhook registration because TelegramBotToken, TelegramWebhookSecretToken, or TelegramWebhookUrl is missing.");
            return;
        }

        if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out var webhookUri) ||
            !string.Equals(webhookUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Skipping Telegram webhook registration because TelegramWebhookUrl is not a valid HTTPS URL: {WebhookUrl}", webhookUrl);
            return;
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient(nameof(TelegramWebhookRegistrationService));
            using var response = await httpClient.PostAsJsonAsync(
                $"https://api.telegram.org/bot{botToken}/setWebhook",
                new SetWebhookRequest(webhookUrl, secretToken, false),
                JsonSerializerOptions,
                stoppingToken);

            var responseBody = await response.Content.ReadAsStringAsync(stoppingToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Telegram webhook registration failed with status {StatusCode}. Response: {ResponseBody}",
                    response.StatusCode,
                    responseBody);
                return;
            }

            var telegramResponse = JsonSerializer.Deserialize<TelegramApiResponse>(responseBody, JsonSerializerOptions);
            if (telegramResponse?.Ok == true)
            {
                _logger.LogInformation("Telegram webhook registered successfully for {WebhookUrl}", webhookUrl);
                return;
            }

            _logger.LogError("Telegram webhook registration returned an unexpected response: {ResponseBody}", responseBody);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Telegram webhook registration canceled during shutdown.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while registering Telegram webhook.");
        }
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private sealed record SetWebhookRequest(
        string Url,
        string SecretToken,
        bool DropPendingUpdates);

    private sealed record TelegramApiResponse(
        bool Ok,
        string? Description);
}
