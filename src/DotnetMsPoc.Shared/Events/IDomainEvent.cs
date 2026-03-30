namespace DotnetMsPoc.Shared.Events;

public interface IDomainEvent
{
    string EventType { get; }
    string TraceId { get; }
    DateTime Timestamp { get; }
}
