using DotnetMsPoc.Shared.Messaging.Kafka;
using DotnetMsPoc.Shared.Messaging.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetMsPoc.Shared.Messaging;

public static class MessagingServiceExtensions
{
    /// <summary>
    /// Registers the IEventPublisher (singleton) based on the Messaging:Provider config value.
    /// </summary>
    public static IServiceCollection AddEventPublisher(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MessagingOptions>(configuration.GetSection(MessagingOptions.SectionName));

        var provider = configuration.GetValue<string>($"{MessagingOptions.SectionName}:Provider") ?? "RabbitMQ";

        if (provider.Equals("Kafka", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
        }
        else
        {
            services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        }

        return services;
    }

    /// <summary>
    /// Registers a hosted event consumer that subscribes to the given routing keys.
    /// The handler callback receives a scoped IServiceProvider, the routing key, and the JSON body.
    /// </summary>
    public static IServiceCollection AddEventConsumer(
        this IServiceCollection services,
        IConfiguration configuration,
        string queueName,
        string[] routingKeys,
        Func<IServiceProvider, string, string, Task> handler)
    {
        services.Configure<MessagingOptions>(configuration.GetSection(MessagingOptions.SectionName));

        var provider = configuration.GetValue<string>($"{MessagingOptions.SectionName}:Provider") ?? "RabbitMQ";

        if (provider.Equals("Kafka", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IHostedService>(sp => new KafkaEventConsumer(
                sp.GetRequiredService<ILogger<KafkaEventConsumer>>(),
                sp.GetRequiredService<IOptions<MessagingOptions>>(),
                sp,
                queueName,
                routingKeys,
                handler));
        }
        else
        {
            services.AddSingleton<IHostedService>(sp => new RabbitMqEventConsumer(
                sp.GetRequiredService<ILogger<RabbitMqEventConsumer>>(),
                sp.GetRequiredService<IOptions<MessagingOptions>>(),
                sp,
                queueName,
                routingKeys,
                handler));
        }

        return services;
    }
}
