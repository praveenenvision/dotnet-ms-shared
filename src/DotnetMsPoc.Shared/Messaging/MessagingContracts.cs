using DotnetMsPoc.Shared.Events;

namespace DotnetMsPoc.Shared.Messaging;

/// <summary>
/// Transport-agnostic event publisher contract.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(T domainEvent, string routingKey) where T : IDomainEvent;
}

/// <summary>
/// Transport-agnostic event consumer contract.
/// Implementations run as a BackgroundService and invoke the registered handler for each message.
/// </summary>
public interface IEventConsumer
{
    /// <summary>
    /// The queue/group name this consumer reads from.
    /// </summary>
    string QueueName { get; }
}

/// <summary>
/// Callback invoked when a message arrives on a consumer.
/// </summary>
public delegate Task EventReceivedHandler(string routingKey, string jsonBody);

/// <summary>
/// Configuration for messaging provider selection and transport settings.
/// Bind to the "Messaging" section of appsettings.json.
/// </summary>
public class MessagingOptions
{
    public const string SectionName = "Messaging";

    /// <summary>"RabbitMQ" or "Kafka"</summary>
    public string Provider { get; set; } = "RabbitMQ";

    public RabbitMqOptions RabbitMQ { get; set; } = new();
    public KafkaOptions Kafka { get; set; } = new();
}

public class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}

public class KafkaOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string GroupId { get; set; } = "default-group";
}
