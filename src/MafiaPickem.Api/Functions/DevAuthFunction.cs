#if DEBUG
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Responses;
using MafiaPickem.Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MafiaPickem.Api.Functions;

/// <summary>
/// Dev-only auth endpoint that bypasses Telegram initData validation.
/// Only compiled in DEBUG builds — never available in production.
/// </summary>
public class DevAuthFunction
{
    private readonly IPickemUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DevAuthFunction> _logger;

    private const long DevTelegramId = 999999999;

    public DevAuthFunction(
        IPickemUserRepository userRepository,
        IConfiguration configuration,
        ILogger<DevAuthFunction> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    [Function("DevAuthenticate")]
    public async Task<HttpResponseData> DevAuth(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/dev")] HttpRequestData req)
    {
        _logger.LogWarning("DEV AUTH endpoint called — this must never be available in production!");

        var adminIdsConfig = _configuration["PickemAdminTelegramIds"] ?? "";
        var adminTelegramIds = adminIdsConfig
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => long.TryParse(x.Trim(), out var id) ? id : 0)
            .Where(x => x != 0)
            .ToHashSet();

        // Upsert a dev user with a fixed Telegram ID
        var user = await _userRepository.UpsertByTelegramIdAsync(DevTelegramId, null);
        var isAdmin = adminTelegramIds.Contains(DevTelegramId);

        var result = new AuthResponse
        {
            User = new UserProfileResponse
            {
                Id = user.Id,
                TelegramId = user.TelegramId,
                GameNickname = user.GameNickname,
                PhotoUrl = user.PhotoUrl,
                IsRegistered = !user.GameNickname.StartsWith("_unregistered_"),
                IsAdmin = isAdmin
            }
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(result);
        return response;
    }
}
#endif
