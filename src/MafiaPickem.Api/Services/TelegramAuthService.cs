using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace MafiaPickem.Api.Services;

public class TelegramAuthService : ITelegramAuthService
{
    private readonly string _botToken;

    public TelegramAuthService(IConfiguration configuration)
    {
        _botToken = configuration["TelegramBotToken"]
            ?? throw new InvalidOperationException("TelegramBotToken not configured");
    }

    public TelegramAuthResult? ValidateInitData(string initData)
    {
        if (string.IsNullOrEmpty(initData))
        {
            return null;
        }

        try
        {
            // Parse initData as URL-encoded query string
            var parameters = ParseQueryString(initData);

            if (!parameters.ContainsKey("hash"))
            {
                return null;
            }

            var receivedHash = parameters["hash"];
            parameters.Remove("hash");

            // Sort parameters alphabetically and build data-check string
            var sortedParams = parameters.OrderBy(kvp => kvp.Key);
            var dataCheckString = string.Join("\n", sortedParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            // Calculate HMAC according to Telegram's algorithm
            // Step 1: HMAC-SHA256("WebAppData", botToken) -> secret_key
            var secretKey = HMACSHA256.HashData(
                Encoding.UTF8.GetBytes("WebAppData"),
                Encoding.UTF8.GetBytes(_botToken)
            );

            // Step 2: HMAC-SHA256(secret_key, data_check_string)
            var hash = HMACSHA256.HashData(secretKey, Encoding.UTF8.GetBytes(dataCheckString));
            var calculatedHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            // Compare hashes
            if (receivedHash != calculatedHash)
            {
                return null;
            }

            // Extract user data from the "user" parameter
            if (!parameters.ContainsKey("user"))
            {
                return null;
            }

            var userJson = parameters["user"];
            var userNode = JsonNode.Parse(userJson);
            
            if (userNode == null)
            {
                return null;
            }

            return new TelegramAuthResult
            {
                TelegramId = userNode["id"]?.GetValue<long>() ?? 0,
                FirstName = userNode["first_name"]?.GetValue<string>(),
                LastName = userNode["last_name"]?.GetValue<string>(),
                Username = userNode["username"]?.GetValue<string>(),
                PhotoUrl = userNode["photo_url"]?.GetValue<string>()
            };
        }
        catch
        {
            return null;
        }
    }

    private static Dictionary<string, string> ParseQueryString(string queryString)
    {
        var parameters = new Dictionary<string, string>();
        var pairs = queryString.Split('&');

        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2);
            if (keyValue.Length == 2)
            {
                var key = Uri.UnescapeDataString(keyValue[0]);
                var value = Uri.UnescapeDataString(keyValue[1]);
                parameters[key] = value;
            }
        }

        return parameters;
    }
}
