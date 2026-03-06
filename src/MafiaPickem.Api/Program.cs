using System.Text.Json;
using Azure.Core.Serialization;
using MafiaPickem.Api.Auth;
using MafiaPickem.Api.Bot;
using MafiaPickem.Api.Data;
using MafiaPickem.Api.Services;
using MafiaPickem.Api.State;
using MafiaPickem.ServiceDefaults;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(builder =>
    {
        builder.UseMiddleware<ExceptionHandlingMiddleware>();
        builder.UseMiddleware<TelegramAuthMiddleware>();
    })
    .ConfigureServices(services =>
    {
        services.AddMafiaPickemServiceDefaults();

        services.Configure<WorkerOptions>(options =>
        {
            options.Serializer = new JsonObjectSerializer(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
        });
        // Infrastructure
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

        // Repositories
        services.AddScoped<IPickemUserRepository, PickemUserRepository>();
        services.AddScoped<ITournamentRepository, TournamentRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IPredictionRepository, PredictionRepository>();
        services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();

        // Services
        services.AddHttpClient();
        services.AddSingleton<ITelegramAuthService, TelegramAuthService>();
        services.AddSingleton<ITelegramWebhookValidator, TelegramWebhookValidator>();
        services.AddHostedService<TelegramWebhookRegistrationService>();
        services.AddSingleton<IMatchStateBlobWriter, MatchStateBlobWriter>();
        services.AddSingleton<ILeaderboardBlobWriter, LeaderboardBlobWriter>();
        services.AddScoped<INicknameService, NicknameService>();
        services.AddScoped<IMatchStateService, MatchStateService>();
        services.AddScoped<IPredictionService, PredictionService>();
        services.AddScoped<IScoringService, ScoringService>();
        services.AddScoped<IStatePublishService, StatePublishService>();

        // Auth
        services.AddScoped<IUserContext, UserContext>();
    })
    .Build();

host.Run();
