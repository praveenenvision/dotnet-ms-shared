using DotnetMsPoc.Shared.Events;

namespace DotnetMsPoc.Shared.Messaging;

public interface IRabbitMqPublisher
{
    Task PublishAsync<T>(T domainEvent, string routingKey) where T : IDomainEvent;
}
