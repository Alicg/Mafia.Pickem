namespace MafiaPickem.Api.State;

public interface IMatchStateBlobWriter
{
    Task WriteStateAsync(BlobMatchState state);
}
