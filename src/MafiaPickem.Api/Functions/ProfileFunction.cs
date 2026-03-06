using MafiaPickem.Api.Auth;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Models.Requests;
using MafiaPickem.Api.Models.Responses;
using MafiaPickem.Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MafiaPickem.Api.Functions;

public class ProfileFunction
{
    private readonly IUserContext _userContext;
    private readonly INicknameService _nicknameService;
    private readonly IPickemUserRepository _userRepository;
    private readonly ILogger<ProfileFunction> _logger;

    public ProfileFunction(
        IUserContext userContext,
        INicknameService nicknameService,
        IPickemUserRepository userRepository,
        ILogger<ProfileFunction>? logger = null)
    {
        _userContext = userContext;
        _nicknameService = nicknameService;
        _userRepository = userRepository;
        _logger = logger ?? null!;
    }

    [Function("GetProfile")]
    public async Task<HttpResponseData> GetProfileHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "me")] HttpRequestData req)
    {
        try
        {
            var profile = GetProfile();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(profile);
            return response;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting user profile");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred");
            return errorResponse;
        }
    }

    [Function("UpdateNickname")]
    public async Task<HttpResponseData> UpdateNicknameHttp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "me/nickname")] HttpRequestData req)
    {
        try
        {
            var request = await req.ReadFromJsonAsync<UpdateNicknameRequest>();
            if (request == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid request body");
                return badRequestResponse;
            }

            var result = await UpdateNickname(request);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result);
            return response;
        }
        catch (InvalidOperationException ex)
        {
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync(ex.Message);
            return badRequestResponse;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating nickname");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("An error occurred");
            return errorResponse;
        }
    }

    public UserProfileResponse GetProfile()
    {
        return new UserProfileResponse
        {
            Id = _userContext.UserId,
            TelegramId = _userContext.TelegramId,
            GameNickname = _userContext.GameNickname,
            PhotoUrl = null, // PhotoUrl is not stored in UserContext, could be fetched if needed
            IsRegistered = _userContext.IsRegistered,
            IsAdmin = _userContext.IsAdmin
        };
    }

    public async Task<AuthResponse> UpdateNickname(UpdateNicknameRequest request)
    {
        // Validate and save nickname
        var validationResult = await _nicknameService.ValidateAndSaveNicknameAsync(
            _userContext.UserId,
            request.GameNickname
        );

        if (!validationResult.IsSuccess)
        {
            throw new InvalidOperationException(validationResult.ErrorMessage);
        }

        // Fetch updated user from database
        var updatedUser = await _userRepository.GetByIdAsync(_userContext.UserId);
        if (updatedUser == null)
        {
            throw new InvalidOperationException("User not found after update");
        }

        return new AuthResponse
        {
            User = new UserProfileResponse
            {
                Id = updatedUser.Id,
                TelegramId = updatedUser.TelegramId,
                GameNickname = updatedUser.GameNickname,
                PhotoUrl = updatedUser.PhotoUrl,
                IsRegistered = !updatedUser.GameNickname.StartsWith("_unregistered_"),
                IsAdmin = _userContext.IsAdmin
            }
        };
    }
}
