namespace MafiaPickem.Api.Bot;

public interface ITelegramWebhookValidator
{
    bool ValidateSecretToken(string? headerValue);
}
