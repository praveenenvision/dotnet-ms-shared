using System.Text;
using System.Text.Json;
using DotnetMsPoc.Shared.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DotnetMsPoc.Shared.Messaging.RabbitMq;

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private const string ExchangeName = "domain_events";

    public RabbitMqEventPublisher(IOptions<MessagingOptions> options, ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;
        var config = options.Value.RabbitMQ;

        var factory = new ConnectionFactory
        {
            HostName = config.Host,
            Port = config.Port,
            UserName = config.Username,
            Password = config.Password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
    }

    public Task PublishAsync<T>(T domainEvent, string routingKey) where T : IDomainEvent
    {
        try
        {
            var message = JsonSerializer.Serialize(domainEvent);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Headers = new Dictionary<string, object>
            {
                { "event_type", domainEvent.EventType },
                { "trace_id", domainEvent.TraceId }
            };

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Published {EventType} event with TraceId: {TraceId}, RoutingKey: {RoutingKey}",
                domainEvent.EventType, domainEvent.TraceId, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish {EventType} event", domainEvent.EventType);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
