using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Requests;
using MafiaPickem.Api.Models.Responses;
using MafiaPickem.Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MafiaPickem.Api.Functions;

public class AuthFunction
{
    private readonly ITelegramAuthService _telegramAuthService;
    private readonly IPickemUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthFunction> _logger;
    private readonly HashSet<long> _adminTelegramIds;

    public AuthFunction(
        ITelegramAuthService telegramAuthService,
        IPickemUserRepository userRepository,
        IConfiguration configuration,
        ILogger<AuthFunction>? logger = null)
    {
        _telegramAuthService = telegramAuthService;
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger ?? null!;

        var adminIdsConfig = _configuration["PickemAdminTelegramIds"] ?? "";
        _adminTelegramIds = adminIdsConfig
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => long.TryParse(x.Trim(), out var id) ? id : 0)
            .Where(x => x != 0)
            .ToHashSet();
    }

    [Function("AuthenticateTelegram")]
    public async Task<HttpResponseData> AuthenticateTelegramHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/telegram")] HttpRequestData req)
    {
        try
        {
            var request = await req.ReadFromJsonAsync<TelegramAuthRequest>();
            if (request == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid request body");
                return badRequestResponse;
            }

            var result = await AuthenticateTelegram(request);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (UnauthorizedAccessException ex)
        {
            var unauthorizedResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await unauthorizedResponse.WriteStringAsync(ex.Message);
            return unauthorizedResponse;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during Telegram authentication");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred during authentication");
            return errorResponse;
        }
    }

    public async Task<AuthResponse> AuthenticateTelegram(TelegramAuthRequest request)
    {
        // Validate Telegram initData
        var telegramResult = _telegramAuthService.ValidateInitData(request.InitData);
        if (telegramResult == null)
        {
            throw new UnauthorizedAccessException("Invalid Telegram authentication data");
        }

        // Upsert user in database
        var user = await _userRepository.UpsertByTelegramIdAsync(
            telegramResult.TelegramId,
            telegramResult.PhotoUrl
        );

        // Check if user is admin
        var isAdmin = _adminTelegramIds.Contains(telegramResult.TelegramId);

        // Build response
        return new AuthResponse
        {
            User = BuildUserProfile(user, isAdmin)
        };
    }

    private static UserProfileResponse BuildUserProfile(Models.Domain.PickemUser user, bool isAdmin)
    {
        return new UserProfileResponse
        {
            Id = user.Id,
            TelegramId = user.TelegramId,
            GameNickname = user.GameNickname,
            PhotoUrl = user.PhotoUrl,
            IsRegistered = !user.GameNickname.StartsWith("_unregistered_"),
            IsAdmin = isAdmin
        };
    }
}
