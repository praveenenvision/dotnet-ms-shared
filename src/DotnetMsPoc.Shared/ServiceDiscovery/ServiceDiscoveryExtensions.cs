using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetMsPoc.Shared.ServiceDiscovery;

public static class ServiceDiscoveryExtensions
{
    public static IServiceCollection AddConsulServiceDiscovery(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ConsulOptions>(configuration.GetSection("ServiceDiscovery"));

        var consulOptions = configuration.GetSection("ServiceDiscovery").Get<ConsulOptions>()
            ?? new ConsulOptions();

        services.AddSingleton<IConsulClient>(_ => new ConsulClient(config =>
        {
            config.Address = new Uri(consulOptions.ConsulAddress);
        }));

        services.AddHostedService<ConsulRegistrationService>();

        return services;
    }
}
