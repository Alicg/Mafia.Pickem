namespace MafiaPickem.Api.Services;

public interface IStatePublishService
{
    Task PublishMatchStateAsync(int matchId, bool forcePublish = false);
}
