using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MafiaPickem.Api.State;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace MafiaPickem.Api.State;

public class MatchStateBlobWriter : IMatchStateBlobWriter
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public MatchStateBlobWriter(IConfiguration configuration)
    {
        var connectionString = configuration["BlobStorageConnectionString"]
            ?? throw new InvalidOperationException("BlobStorageConnectionString not configured");
        _containerName = configuration["BlobContainerName"]
            ?? throw new InvalidOperationException("BlobContainerName not configured");

        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task WriteStateAsync(BlobMatchState state)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobName = $"match-state-{state.MatchId}.json";
        var blobClient = containerClient.GetBlobClient(blobName);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(state, jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        using var stream = new MemoryStream(bytes);

        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = "application/json",
            CacheControl = "max-age=1, must-revalidate"
        };

        await blobClient.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders
        });
    }

    public async Task DeleteStateAsync(int matchId)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobName = $"match-state-{matchId}.json";
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}
