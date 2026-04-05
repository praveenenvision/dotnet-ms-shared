namespace DotnetMsPoc.Shared.ServiceDiscovery;

public class ConsulOptions
{
    public string ConsulAddress { get; set; } = "http://localhost:8500";
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceAddress { get; set; } = string.Empty;
    public int ServicePort { get; set; } = 8080;
    public string HealthCheckEndpoint { get; set; } = "/health";
    public int HealthCheckIntervalSeconds { get; set; } = 10;
    public int DeregisterAfterMinutes { get; set; } = 1;
    public string[] Tags { get; set; } = [];
}
