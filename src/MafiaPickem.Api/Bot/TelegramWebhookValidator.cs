using Microsoft.Extensions.Configuration;

namespace MafiaPickem.Api.Bot;

public class TelegramWebhookValidator : ITelegramWebhookValidator
{
    private readonly string? _expectedSecretToken;

    public TelegramWebhookValidator(IConfiguration configuration)
    {
        _expectedSecretToken = configuration["TelegramWebhookSecretToken"];
    }

    public bool ValidateSecretToken(string? headerValue)
    {
        // Fail safe: if secret token is not configured, always return false
        if (string.IsNullOrEmpty(_expectedSecretToken))
        {
            return false;
        }

        // If header value is null or empty, return false
        if (string.IsNullOrEmpty(headerValue))
        {
            return false;
        }

        // Simple string comparison (not cryptographic auth)
        return headerValue == _expectedSecretToken;
    }
}
