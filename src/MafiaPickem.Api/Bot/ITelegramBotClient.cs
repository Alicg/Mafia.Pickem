namespace MafiaPickem.Api.Bot;

public interface ITelegramBotClient
{
    TelegramBotOptions Options { get; }

    Task<bool> RegisterWebhookAsync(CancellationToken cancellationToken);

    Task<bool> ConfigureMiniAppMenuButtonAsync(CancellationToken cancellationToken);

    Task<bool> SendMiniAppPromptAsync(long chatId, CancellationToken cancellationToken);
}