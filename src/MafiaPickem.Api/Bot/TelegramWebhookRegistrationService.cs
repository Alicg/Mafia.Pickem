using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MafiaPickem.Api.Bot;

public sealed class TelegramWebhookRegistrationService : BackgroundService
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ILogger<TelegramWebhookRegistrationService> _logger;

    public TelegramWebhookRegistrationService(
        ITelegramBotClient telegramBotClient,
        ILogger<TelegramWebhookRegistrationService> logger)
    {
        _telegramBotClient = telegramBotClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_telegramBotClient.Options.CanRegisterWebhook)
        {
            _logger.LogInformation(
                "Skipping Telegram webhook registration because TelegramBotToken, TelegramWebhookSecretToken, or TelegramWebhookUrl is missing.");
        }
        else
        {
            await _telegramBotClient.RegisterWebhookAsync(stoppingToken);
        }

        if (!_telegramBotClient.Options.CanShowMiniApp)
        {
            _logger.LogInformation("Skipping Telegram mini app menu button configuration because TelegramMiniAppUrl is missing.");
            return;
        }

        await _telegramBotClient.ConfigureMiniAppMenuButtonAsync(stoppingToken);
    }
}
