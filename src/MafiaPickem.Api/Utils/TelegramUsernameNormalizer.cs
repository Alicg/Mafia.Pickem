namespace MafiaPickem.Api.Utils;

public static class TelegramUsernameNormalizer
{
    public static string? Normalize(string? telegramUsername)
    {
        if (string.IsNullOrWhiteSpace(telegramUsername))
        {
            return null;
        }

        return telegramUsername.Trim().TrimStart('@').ToLowerInvariant();
    }

    public static string? NormalizeMention(string? telegramUsername)
    {
        var normalized = Normalize(telegramUsername);
        return normalized == null ? null : $"@{normalized}";
    }
}