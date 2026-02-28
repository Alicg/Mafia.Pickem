namespace MafiaPickem.Api.State;

public interface IMatchStateBlobWriter
{
    Task WriteStateAsync(BlobMatchState state);
    Task DeleteStateAsync(int matchId);
    Task<DateTime?> GetLastPublishTimeAsync(int matchId);
}
