using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace MafiaPickem.Api.State;

public class TournamentOverlayClientStateBlobStore : ITournamentOverlayClientStateStore
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public TournamentOverlayClientStateBlobStore(IConfiguration configuration)
    {
        var connectionString = configuration["BlobStorageConnectionString"]
            ?? throw new InvalidOperationException("BlobStorageConnectionString not configured");
        _containerName = configuration["BlobContainerName"]
            ?? throw new InvalidOperationException("BlobContainerName not configured");

        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<TournamentOverlayClientState?> ReadAsync(int tournamentId)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(GetBlobName(tournamentId));

        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        await using var stream = await blobClient.OpenReadAsync();
        return await JsonSerializer.DeserializeAsync<TournamentOverlayClientState>(stream, _jsonOptions);
    }

    public async Task WriteAsync(TournamentOverlayClientState state)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(GetBlobName(state.TournamentId));
        var json = JsonSerializer.Serialize(state, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        await using var stream = new MemoryStream(bytes);
        await blobClient.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = "application/json",
                CacheControl = "max-age=1, must-revalidate"
            }
        });
    }

    private static string GetBlobName(int tournamentId)
    {
        return $"overlay-client-state-{tournamentId}.json";
    }
}