using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DotnetMsPoc.Shared.Messaging.RabbitMq;

public class RabbitMqEventConsumer : BackgroundService, IEventConsumer
{
    private readonly ILogger<RabbitMqEventConsumer> _logger;
    private readonly MessagingOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _queueName;
    private readonly string[] _routingKeys;
    private readonly Func<IServiceProvider, string, string, Task> _handler;
    private IConnection? _connection;
    private IModel? _channel;

    public string QueueName => _queueName;

    public RabbitMqEventConsumer(
        ILogger<RabbitMqEventConsumer> logger,
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

        var config = _options.RabbitMQ;

        for (int i = 0; i < 10; i++)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = config.Host,
                    Port = config.Port,
                    UserName = config.Username,
                    Password = config.Password
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare("domain_events", ExchangeType.Topic, durable: true);
                _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);

                foreach (var key in _routingKeys)
                {
                    _channel.QueueBind(_queueName, "domain_events", key);
                }

                _logger.LogInformation("RabbitMQ consumer connected - queue: {Queue}, routing keys: [{Keys}]",
                    _queueName, string.Join(", ", _routingKeys));
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to connect to RabbitMQ (attempt {Attempt}): {Message}", i + 1, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)), stoppingToken);
            }
        }

        if (_channel == null)
        {
            _logger.LogError("Could not connect to RabbitMQ after retries");
            return;
        }

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var routingKey = ea.RoutingKey;

                using var scope = _serviceProvider.CreateScope();
                await _handler(scope.ServiceProvider, routingKey, body);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event from queue {Queue}", _queueName);
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(_queueName, autoAck: false, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
