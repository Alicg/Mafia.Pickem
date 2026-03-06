using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace MafiaPickem.ServiceDefaults;

/// <summary>
/// Service defaults for MafiaPickem services.
/// This project provides shared configuration that can be referenced by individual services.
/// It will be expanded in future phases to include telemetry, resilience, and other cross-cutting concerns.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds common service defaults to the host application builder.
    /// Placeholder for future implementation.
    /// </summary>
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMafiaPickemServiceDefaults();
        return builder;
    }

    public static IServiceCollection AddMafiaPickemServiceDefaults(this IServiceCollection services)
    {
        // Minimum shared baseline for local observability and diagnostics.
        services.AddHealthChecks();
        return services;
    }
}
