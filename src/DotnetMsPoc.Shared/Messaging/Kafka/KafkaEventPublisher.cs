using System.Text.Json;
using Confluent.Kafka;
using DotnetMsPoc.Shared.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetMsPoc.Shared.Messaging.Kafka;

public class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;
    private const string TopicPrefix = "domain-events.";

    public KafkaEventPublisher(IOptions<MessagingOptions> options, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;
        var config = options.Value.Kafka;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishAsync<T>(T domainEvent, string routingKey) where T : IDomainEvent
    {
        try
        {
            var topic = $"{TopicPrefix}{routingKey}";
            var message = JsonSerializer.Serialize(domainEvent);

            var kafkaMessage = new Message<string, string>
            {
                Key = domainEvent.TraceId,
                Value = message,
                Headers = new Headers
                {
                    { "event_type", System.Text.Encoding.UTF8.GetBytes(domainEvent.EventType) },
                    { "trace_id", System.Text.Encoding.UTF8.GetBytes(domainEvent.TraceId) }
                }
            };

            var result = await _producer.ProduceAsync(topic, kafkaMessage);

            _logger.LogInformation("Published {EventType} to Kafka topic {Topic}, TraceId: {TraceId}, Offset: {Offset}",
                domainEvent.EventType, topic, domainEvent.TraceId, result.Offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish {EventType} event to Kafka", domainEvent.EventType);
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}
