using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetMsPoc.Shared.Messaging.Kafka;

public class KafkaEventConsumer : BackgroundService, IEventConsumer
{
    private readonly ILogger<KafkaEventConsumer> _logger;
    private readonly MessagingOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _queueName;
    private readonly string[] _routingKeys;
    private readonly Func<IServiceProvider, string, string, Task> _handler;
    private const string TopicPrefix = "domain-events.";

    public string QueueName => _queueName;

    public KafkaEventConsumer(
        ILogger<KafkaEventConsumer> logger,
        IOptions<MessagingOptions> options,
        IServiceProvider serviceProvider,
        string queueName,
        string[] routingKeys,
        Func<IServiceProvider, string, string, Task> handler)
    {
        _logger = logger;
        _options = options.Value;
        _serviceProvider = serviceProvider;
        _queueName = queueName;
        _routingKeys = routingKeys;
        _handler = handler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var config = _options.Kafka;
        var topics = _routingKeys.Select(rk => $"{TopicPrefix}{rk}").ToList();

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config.BootstrapServers,
            GroupId = config.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        for (int attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
                consumer.Subscribe(topics);

                _logger.LogInformation("Kafka consumer connected - group: {GroupId}, topics: [{Topics}]",
                    config.GroupId, string.Join(", ", topics));

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(stoppingToken);
                        if (result?.Message == null) continue;

                        // Extract routing key from topic name (strip prefix)
                        var routingKey = result.Topic.StartsWith(TopicPrefix)
                            ? result.Topic[TopicPrefix.Length..]
                            : result.Topic;

                        using var scope = _serviceProvider.CreateScope();
                        await _handler(scope.ServiceProvider, routingKey, result.Message.Value);

                        consumer.Commit(result);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming from Kafka topic");
                    }
                }

                consumer.Close();
                return;
            }
            catch (Exception ex) when (attempt < 9)
            {
                _logger.LogWarning("Failed to connect to Kafka (attempt {Attempt}): {Message}", attempt + 1, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), stoppingToken);
            }
        }

        _logger.LogError("Could not connect to Kafka after retries");
    }
}
