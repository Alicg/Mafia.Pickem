namespace MafiaPickem.Api.State;

public interface IMatchStateBlobReader
{
    Task<BlobMatchState?> ReadStateAsync(int matchId);
}
