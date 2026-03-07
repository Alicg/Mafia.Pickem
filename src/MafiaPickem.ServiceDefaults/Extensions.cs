using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace MafiaPickem.ServiceDefaults;

/// <summary>
/// Shared service defaults for MafiaPickem services.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds common service defaults to the host application builder.
    /// </summary>
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddMafiaPickemServiceDefaults(builder.Configuration, builder.Environment);
        return builder;
    }

    public static IServiceCollection AddMafiaPickemServiceDefaults(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddLogging(logging =>
        {
            logging.AddOpenTelemetry();
        });

        services.Configure<OpenTelemetryLoggerOptions>(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;

            if (UseOtlpExporter(configuration))
            {
                options.AddOtlpExporter();
            }
        });

        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(environment.ApplicationName)
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (UseOtlpExporter(configuration))
                {
                    metrics.AddOtlpExporter();
                }
            })
            .WithTracing(tracing =>
            {
                if (environment.IsDevelopment())
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing
                    .AddSource(environment.ApplicationName)
                    .AddHttpClientInstrumentation(options => options.RecordException = true)
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.SetDbStatementForText = true;
                    });

                if (UseOtlpExporter(configuration))
                {
                    tracing.AddOtlpExporter();
                }
            });

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return services;
    }

    private static bool UseOtlpExporter(IConfiguration configuration)
    {
        return !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
    }
}
