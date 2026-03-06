using MafiaPickem.Api.Bot;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MafiaPickem.Api.Functions;

public class BotWebhookFunction
{
    private readonly ITelegramWebhookValidator _webhookValidator;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ILogger<BotWebhookFunction> _logger;

    public BotWebhookFunction(
        ITelegramWebhookValidator webhookValidator,
        ITelegramBotClient telegramBotClient,
        ILogger<BotWebhookFunction>? logger = null)
    {
        _webhookValidator = webhookValidator;
        _telegramBotClient = telegramBotClient;
        _logger = logger ?? null!;
    }

    [Function("BotWebhook")]
    public async Task<HttpResponseData> HandleWebhook(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bot/webhook")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        // 1. Read the secret token header
        var secretToken = req.Headers.GetValues("X-Telegram-Bot-Api-Secret-Token").FirstOrDefault();

        // 2. Validate the secret token
        if (!_webhookValidator.ValidateSecretToken(secretToken))
        {
            _logger?.LogWarning("Invalid webhook secret token");
            var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await unauthorizedResponse.WriteStringAsync("Unauthorized");
            return unauthorizedResponse;
        }

        // 3. Read and log the update
        try
        {
            string body;
            using (var reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                body = await reader.ReadToEndAsync();
            }
            
            _logger?.LogInformation("Received Telegram webhook update: {Update}", body);

            long? privateChatId = null;

            if (!string.IsNullOrEmpty(body))
            {
                using var jsonDoc = JsonDocument.Parse(body);
                privateChatId = TryGetPrivateChatId(jsonDoc.RootElement);
            }

            if (privateChatId.HasValue)
            {
                await _telegramBotClient.SendMiniAppPromptAsync(privateChatId.Value, cancellationToken);
            }

            // 4. Return 200 OK
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("OK");
            return response;
        }
        catch (JsonException ex)
        {
            _logger?.LogWarning(ex, "Invalid JSON in webhook request");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid JSON");
            return badRequestResponse;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error processing webhook");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Internal server error");
            return errorResponse;
        }
    }

    private static long? TryGetPrivateChatId(JsonElement root)
    {
        if (!TryGetMessage(root, out var message) ||
            !message.TryGetProperty("chat", out var chat) ||
            !chat.TryGetProperty("id", out var chatIdElement) ||
            !chatIdElement.TryGetInt64(out var chatId) ||
            !chat.TryGetProperty("type", out var chatTypeElement))
        {
            return null;
        }

        return string.Equals(chatTypeElement.GetString(), "private", StringComparison.OrdinalIgnoreCase)
            ? chatId
            : null;
    }

    private static bool TryGetMessage(JsonElement root, out JsonElement message)
    {
        if (root.TryGetProperty("message", out message))
        {
            return true;
        }

        if (root.TryGetProperty("edited_message", out message))
        {
            return true;
        }

        message = default;
        return false;
    }
}
