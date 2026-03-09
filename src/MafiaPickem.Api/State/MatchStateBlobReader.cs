using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace MafiaPickem.Api.State;

public class MatchStateBlobReader : IMatchStateBlobReader
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public MatchStateBlobReader(IConfiguration configuration)
    {
        var connectionString = configuration["BlobStorageConnectionString"]
            ?? throw new InvalidOperationException("BlobStorageConnectionString not configured");
        _containerName = configuration["BlobContainerName"]
            ?? throw new InvalidOperationException("BlobContainerName not configured");

        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<BlobMatchState?> ReadStateAsync(int matchId)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient($"match-state-{matchId}.json");

        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        await using var stream = await blobClient.OpenReadAsync();
        return await JsonSerializer.DeserializeAsync<BlobMatchState>(stream, _jsonOptions);
    }
}
