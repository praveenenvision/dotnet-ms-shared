using DotnetMsPoc.Shared.Events;
using FluentAssertions;
using Xunit;

namespace DotnetMsPoc.Shared.Tests.Events;

public class DomainEventTests
{
    [Fact]
    public void OrderPlacedEvent_ShouldImplementIDomainEvent()
    {
        var evt = new OrderPlacedEvent();
        evt.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void OrderPlacedEvent_EventType_ShouldBeCorrect()
    {
        var evt = new OrderPlacedEvent();
        evt.EventType.Should().Be("order.placed");
    }

    [Fact]
    public void OrderModifiedEvent_EventType_ShouldBeCorrect()
    {
        var evt = new OrderModifiedEvent();
        evt.EventType.Should().Be("order.modified");
    }

    [Fact]
    public void OrderCancelledEvent_EventType_ShouldBeCorrect()
    {
        var evt = new OrderCancelledEvent();
        evt.EventType.Should().Be("order.cancelled");
    }

    [Fact]
    public void OrderConfirmedEvent_EventType_ShouldBeCorrect()
    {
        var evt = new OrderConfirmedEvent();
        evt.EventType.Should().Be("order.confirmed");
    }

    [Fact]
    public void InventoryReducedEvent_EventType_ShouldBeCorrect()
    {
        var evt = new InventoryReducedEvent();
        evt.EventType.Should().Be("inventory.reduced");
    }

    [Fact]
    public void AllEvents_Timestamp_ShouldBeCloseToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);

        var events = new IDomainEvent[]
        {
            new OrderPlacedEvent(),
            new OrderModifiedEvent(),
            new OrderCancelledEvent(),
            new OrderConfirmedEvent(),
            new InventoryReducedEvent()
        };

        var after = DateTime.UtcNow.AddSeconds(1);

        foreach (var evt in events)
        {
            evt.Timestamp.Should().BeOnOrAfter(before);
            evt.Timestamp.Should().BeOnOrBefore(after);
        }
    }

    [Fact]
    public void AllEvents_TraceId_ShouldDefaultToEmpty()
    {
        var events = new IDomainEvent[]
        {
            new OrderPlacedEvent(),
            new OrderModifiedEvent(),
            new OrderCancelledEvent(),
            new OrderConfirmedEvent(),
            new InventoryReducedEvent()
        };

        foreach (var evt in events)
        {
            evt.TraceId.Should().BeEmpty();
        }
    }

    [Fact]
    public void OrderPlacedEvent_ShouldHoldItemsAndAmount()
    {
        var evt = new OrderPlacedEvent
        {
            OrderId = 1,
            CustomerEmail = "test@example.com",
            TotalAmount = 199.98m,
            TraceId = "trace-123",
            Items = new List<OrderEventItem>
            {
                new() { ProductId = 1, ProductName = "Laptop", Quantity = 2, UnitPrice = 99.99m }
            }
        };

        evt.OrderId.Should().Be(1);
        evt.CustomerEmail.Should().Be("test@example.com");
        evt.TotalAmount.Should().Be(199.98m);
        evt.TraceId.Should().Be("trace-123");
        evt.Items.Should().HaveCount(1);
        evt.Items[0].ProductName.Should().Be("Laptop");
    }

    [Fact]
    public void OrderCancelledEvent_ShouldHoldReason()
    {
        var evt = new OrderCancelledEvent
        {
            OrderId = 5,
            Reason = "Changed my mind"
        };

        evt.Reason.Should().Be("Changed my mind");
    }

    [Fact]
    public void InventoryReducedEvent_ShouldHoldStockInfo()
    {
        var evt = new InventoryReducedEvent
        {
            ProductId = 10,
            ProductName = "Keyboard",
            QuantityReduced = 3,
            NewStock = 47
        };

        evt.ProductId.Should().Be(10);
        evt.QuantityReduced.Should().Be(3);
        evt.NewStock.Should().Be(47);
    }
}
