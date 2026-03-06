var builder = DistributedApplication.CreateBuilder(args);

var sqlConnectionString = GetConfigValue(
    "SqlConnectionString",
    "Server=localhost;Database=MafiaPickem;Trusted_Connection=True;TrustServerCertificate=True;");
var blobStorageConnectionString = GetConfigValue("BlobStorageConnectionString", "UseDevelopmentStorage=true");
var blobContainerName = GetConfigValue("BlobContainerName", "match-states");
var pickemAdminTelegramIds = GetConfigValue("PickemAdminTelegramIds", "999999999");
var telegramBotToken = GetConfigValue("TelegramBotToken", "YOUR_BOT_TOKEN_HERE");
var telegramWebhookSecretToken = GetConfigValue("TelegramWebhookSecretToken", "your-webhook-secret-token-here");

var frontendApiProxyTarget = GetConfigValue("VITE_DEV_PROXY_API_TARGET", "http://localhost:7071");
var frontendBlobProxyTarget = GetConfigValue("VITE_DEV_PROXY_BLOB_TARGET", "http://127.0.0.1:10000");
var frontendBlobProxyAccount = GetConfigValue("VITE_DEV_PROXY_BLOB_ACCOUNT", "devstoreaccount1");
var frontendBlobProxyContainer = GetConfigValue("VITE_DEV_PROXY_BLOB_CONTAINER", blobContainerName);

// 1. Azurite - Azure Storage emulator
var azurite = builder.AddExecutable("azurite", "npx", "../..", "--yes", "azurite", "--silent", "--skipApiVersionCheck");

// 2. Backend - Azure Functions API
var backend = builder.AddExecutable("backend", "npx", "../MafiaPickem.Api", "func", "start", "--port", "7071")
    .WithEnvironment("SqlConnectionString", sqlConnectionString)
    .WithEnvironment("BlobStorageConnectionString", blobStorageConnectionString)
    .WithEnvironment("BlobContainerName", blobContainerName)
    .WithEnvironment("PickemAdminTelegramIds", pickemAdminTelegramIds)
    .WithEnvironment("TelegramBotToken", telegramBotToken)
    .WithEnvironment("TelegramWebhookSecretToken", telegramWebhookSecretToken)
    .WaitFor(azurite);

// 3. Frontend - Vite dev server
var frontend = builder.AddExecutable("frontend", "npm", "../frontend", "run", "dev", "--", "--port", "5173", "--strictPort")
    .WithEnvironment("VITE_DEV_PROXY_API_TARGET", frontendApiProxyTarget)
    .WithEnvironment("VITE_DEV_PROXY_BLOB_TARGET", frontendBlobProxyTarget)
    .WithEnvironment("VITE_DEV_PROXY_BLOB_ACCOUNT", frontendBlobProxyAccount)
    .WithEnvironment("VITE_DEV_PROXY_BLOB_CONTAINER", frontendBlobProxyContainer)
    .WaitFor(backend);

builder.Build().Run();

string GetConfigValue(string key, string defaultValue)
{
    return string.IsNullOrWhiteSpace(builder.Configuration[key])
        ? defaultValue
        : builder.Configuration[key]!;
}
