using System.Security.Claims;
using MafiaPickem.Api.Models.Domain;

namespace MafiaPickem.Api.Services;

public interface IJwtService
{
    string GenerateToken(PickemUser user, bool isAdmin);
    ClaimsPrincipal? ValidateToken(string token);
}
