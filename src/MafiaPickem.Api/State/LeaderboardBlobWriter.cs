using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MafiaPickem.Api.Models.Responses;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace MafiaPickem.Api.State;

public class LeaderboardBlobWriter : ILeaderboardBlobWriter
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public LeaderboardBlobWriter(IConfiguration configuration)
    {
        var connectionString = configuration["BlobStorageConnectionString"]
            ?? throw new InvalidOperationException("BlobStorageConnectionString not configured");
        _containerName = configuration["BlobContainerName"]
            ?? throw new InvalidOperationException("BlobContainerName not configured");

        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task WriteAsync(int tournamentId, LeaderboardResponse leaderboard)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobName = $"leaderboard-{tournamentId}.json";
        var blobClient = containerClient.GetBlobClient(blobName);

        var json = JsonSerializer.Serialize(leaderboard, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        using var stream = new MemoryStream(bytes);

        await blobClient.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = "application/json",
                CacheControl = "max-age=5, must-revalidate",
            }
        });
    }
}
