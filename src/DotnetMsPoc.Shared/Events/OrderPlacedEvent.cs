namespace DotnetMsPoc.Shared.Events;

public class OrderPlacedEvent : IDomainEvent
{
    public string EventType => "order.placed";
    public int OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public List<OrderEventItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
