var builder = DistributedApplication.CreateBuilder(args);

// 1. Azurite - Azure Storage emulator
var azurite = builder.AddExecutable("azurite", "npx", "../..", "--yes", "azurite", "--silent", "--skipApiVersionCheck")
    .WithEndpoint(scheme: "http", port: 10000, name: "blob")
    .WithEndpoint(scheme: "http", port: 10001, name: "queue")
    .WithEndpoint(scheme: "http", port: 10002, name: "table");

// 2. Backend - Azure Functions API
var backend = builder.AddExecutable("backend", "npx", "../MafiaPickem.Api", "func", "start", "--port", "7071")
    .WithEndpoint(scheme: "http", port: 7071, name: "http")
    .WaitFor(azurite);

// 3. Frontend - Vite dev server
var frontend = builder.AddExecutable("frontend", "npm", "../frontend", "run", "dev", "--", "--port", "5173", "--strictPort")
    .WithEndpoint(scheme: "http", port: 5173, name: "http")
    .WithEnvironment("VITE_API_URL", backend.GetEndpoint("http"))
    .WaitFor(backend);

builder.Build().Run();
