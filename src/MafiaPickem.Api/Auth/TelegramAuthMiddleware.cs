using System.Security.Claims;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MafiaPickem.Api.Auth;

public class TelegramAuthMiddleware : IFunctionsWorkerMiddleware
{
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramAuthMiddleware> _logger;
    private readonly HashSet<long> _adminTelegramIds;

    // Routes that don't require authentication
    private readonly HashSet<string> _publicRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/telegram",
        "/api/auth/dev",
        "/api/bot/webhook"
    };

    public TelegramAuthMiddleware(
        IJwtService jwtService,
        IConfiguration configuration,
        ILogger<TelegramAuthMiddleware> logger)
    {
        _jwtService = jwtService;
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

        // Extract Authorization header
        if (!requestData.Headers.TryGetValues("Authorization", out var authHeaders))
        {
            throw new UnauthorizedAccessException("Missing Authorization header");
        }

        var authHeader = authHeaders.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Invalid Authorization header format");
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        // Validate JWT token
        var principal = _jwtService.ValidateToken(token, out var validationError);
        if (principal == null)
        {
            throw new UnauthorizedAccessException($"Invalid or expired token. {validationError ?? "Token validation returned null."}");
        }

        // Extract user ID from claims
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid token claims");
        }

        // Load user from database
        var userRepository = context.InstanceServices.GetService(typeof(IPickemUserRepository)) as IPickemUserRepository;
        var user = await userRepository!.GetByIdAsync(userId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Check admin status
        var isAdmin = _adminTelegramIds.Contains(user.TelegramId);

        // Set user context
        var userContext = context.InstanceServices.GetService(typeof(IUserContext)) as IUserContext;
        if (userContext != null)
        {
            userContext.Set(user, isAdmin);
        }
        else
        {
            _logger.LogWarning("IUserContext not found in service provider");
        }

        await next(context);
    }
}

public class UnauthorizedAccessException : Exception
{
    public UnauthorizedAccessException(string message) : base(message)
    {
    }
}
