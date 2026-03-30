namespace DotnetMsPoc.Shared.Events;

public class OrderModifiedEvent : IDomainEvent
{
    public string EventType => "order.modified";
    public int OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public List<OrderEventItem> Items { get; set; } = new();
    public decimal NewTotalAmount { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
