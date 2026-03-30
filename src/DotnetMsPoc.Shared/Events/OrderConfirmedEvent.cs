namespace DotnetMsPoc.Shared.Events;

public class OrderConfirmedEvent : IDomainEvent
{
    public string EventType => "order.confirmed";
    public int OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public List<OrderEventItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
