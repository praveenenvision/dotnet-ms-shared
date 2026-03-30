namespace DotnetMsPoc.Shared.Events;

public class OrderCancelledEvent : IDomainEvent
{
    public string EventType => "order.cancelled";
    public int OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
