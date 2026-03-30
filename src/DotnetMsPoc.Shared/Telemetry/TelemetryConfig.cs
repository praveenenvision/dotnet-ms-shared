using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DotnetMsPoc.Shared.Telemetry;

public static class TelemetryConfig
{
    public static IServiceCollection AddCustomOpenTelemetry(this IServiceCollection services, string serviceName)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource(serviceName)
                    .AddConsoleExporter();
            });

        return services;
    }
}
