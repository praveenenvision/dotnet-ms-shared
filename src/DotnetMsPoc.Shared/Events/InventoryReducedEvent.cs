namespace DotnetMsPoc.Shared.Events;

public class InventoryReducedEvent : IDomainEvent
{
    public string EventType => "inventory.reduced";
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantityReduced { get; set; }
    public int NewStock { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
