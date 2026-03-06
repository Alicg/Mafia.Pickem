using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MafiaPickem.Api.Models.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace MafiaPickem.Api.Services;

public class JwtService : IJwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly ILogger<JwtService>? _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService>? logger = null)
    {
        _secret = configuration["JwtSecret"]
            ?? throw new InvalidOperationException("JwtSecret not configured");
        _issuer = configuration["JwtIssuer"]
            ?? throw new InvalidOperationException("JwtIssuer not configured");
        _tokenHandler = new JwtSecurityTokenHandler();
        _logger = logger;
    }

    public string GenerateToken(PickemUser user, bool isAdmin)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("telegram_id", user.TelegramId.ToString()),
            new("is_admin", isAdmin.ToString())
        };

        // Only include nickname if user is registered (not using placeholder)
        if (!user.GameNickname.StartsWith("_unregistered_"))
        {
            claims.Add(new Claim("nickname", user.GameNickname));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(48),
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        return ValidateToken(token, out _);
    }

    public ClaimsPrincipal? ValidateToken(string token, out string? errorMessage)
    {
        try
        {
            errorMessage = null;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _issuer,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            _logger?.LogWarning(
                ex,
                "JWT validation failed. Issuer='{Issuer}', tokenLength={TokenLength}, error='{ErrorMessage}'",
                _issuer,
                token?.Length ?? 0,
                errorMessage);
            return null;
        }
    }
}
