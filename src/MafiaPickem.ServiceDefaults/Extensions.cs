using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        // Placeholder for future service defaults configuration
        // This will be expanded to include:
        // - OpenTelemetry configuration
        // - Health checks
        // - Service discovery
        // - Resilience policies
        return builder;
    }
}
