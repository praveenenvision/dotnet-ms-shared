using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotnetMsPoc.Shared.ServiceDiscovery;

public class ConsulRegistrationService : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly ConsulOptions _options;
    private readonly ILogger<ConsulRegistrationService> _logger;
    private string _registrationId = string.Empty;

    public ConsulRegistrationService(
        IConsulClient consulClient,
        IOptions<ConsulOptions> options,
        ILogger<ConsulRegistrationService> logger)
    {
        _consulClient = consulClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _registrationId = $"{_options.ServiceName}-{Guid.NewGuid():N}";

        var registration = new AgentServiceRegistration
        {
            ID = _registrationId,
            Name = _options.ServiceName,
            Address = _options.ServiceAddress,
            Port = _options.ServicePort,
            Tags = _options.Tags,
            Check = new AgentServiceCheck
            {
                HTTP = $"http://{_options.ServiceAddress}:{_options.ServicePort}{_options.HealthCheckEndpoint}",
                Interval = TimeSpan.FromSeconds(_options.HealthCheckIntervalSeconds),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(_options.DeregisterAfterMinutes),
                Timeout = TimeSpan.FromSeconds(5)
            }
        };

        _logger.LogInformation(
            "Registering service {ServiceName} (ID: {ServiceId}) with Consul at {ConsulAddress}",
            _options.ServiceName, _registrationId, _options.ConsulAddress);

        await _consulClient.Agent.ServiceRegister(registration, cancellationToken);

        _logger.LogInformation("Service {ServiceName} registered with Consul", _options.ServiceName);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deregistering service {ServiceId} from Consul", _registrationId);
        await _consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
    }
}
