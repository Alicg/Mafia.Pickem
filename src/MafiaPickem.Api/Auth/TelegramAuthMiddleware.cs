using MafiaPickem.Api.Data;
using MafiaPickem.Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MafiaPickem.Api.Auth;

public class TelegramAuthMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ITelegramAuthService _telegramAuthService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramAuthMiddleware> _logger;
    private readonly HashSet<long> _adminTelegramIds;
    private const string InitDataHeaderName = "X-Telegram-Init-Data";
#if DEBUG
    private const string DevAuthHeaderName = "X-Dev-Auth";
    private const long DevTelegramId = 999999999;
#endif

    // Routes that don't require authentication
    private readonly HashSet<string> _publicRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/telegram",
        "/api/auth/dev",
        "/api/bot/webhook"
    };

    public TelegramAuthMiddleware(
        ITelegramAuthService telegramAuthService,
        IConfiguration configuration,
        ILogger<TelegramAuthMiddleware> logger)
    {
        _telegramAuthService = telegramAuthService;
        _configuration = configuration;
        _logger = logger;

        var adminIdsConfig = _configuration["PickemAdminTelegramIds"] ?? "";
        _adminTelegramIds = adminIdsConfig
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => long.TryParse(x.Trim(), out var id) ? id : 0)
            .Where(x => x != 0)
            .ToHashSet();
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var requestData = await context.GetHttpRequestDataAsync();
        if (requestData == null)
        {
            await next(context);
            return;
        }

        // Check if route requires authentication
        var url = requestData.Url.AbsolutePath;
        if (_publicRoutes.Contains(url))
        {
            await next(context);
            return;
        }

#if DEBUG
        if (requestData.Headers.TryGetValues(DevAuthHeaderName, out var devHeaders)
            && string.Equals(devHeaders.FirstOrDefault(), "true", StringComparison.OrdinalIgnoreCase))
        {
            await AuthenticateDevUserAsync(context);
            await next(context);
            return;
        }
#endif

        if (!requestData.Headers.TryGetValues(InitDataHeaderName, out var initDataHeaders))
        {
            throw new UnauthorizedAccessException($"Missing {InitDataHeaderName} header");
        }

        var initData = initDataHeaders.FirstOrDefault();
        var telegramResult = _telegramAuthService.ValidateInitData(initData ?? string.Empty);
        if (telegramResult == null)
        {
            throw new UnauthorizedAccessException("Invalid Telegram initData");
        }

        var userRepository = context.InstanceServices.GetRequiredService<IPickemUserRepository>();
        var user = await userRepository.UpsertByTelegramIdAsync(telegramResult.TelegramId, telegramResult.PhotoUrl);

        var isAdmin = _adminTelegramIds.Contains(telegramResult.TelegramId);

        SetUserContext(context, user, isAdmin);

        await next(context);
    }

    private void SetUserContext(FunctionContext context, Models.Domain.PickemUser user, bool isAdmin)
    {
        var userContext = context.InstanceServices.GetService<IUserContext>();
        if (userContext == null)
        {
            _logger.LogWarning("IUserContext not found in service provider");
            return;
        }

        userContext.Set(user, isAdmin);
    }

#if DEBUG
    private async Task AuthenticateDevUserAsync(FunctionContext context)
    {
        var userRepository = context.InstanceServices.GetRequiredService<IPickemUserRepository>();
        var user = await userRepository.UpsertByTelegramIdAsync(DevTelegramId, null);
        var isAdmin = _adminTelegramIds.Contains(DevTelegramId);

        SetUserContext(context, user, isAdmin);
    }
#endif
}

public class UnauthorizedAccessException : Exception
{
    public UnauthorizedAccessException(string message) : base(message)
    {
    }
}
