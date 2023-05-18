namespace Advent.Ddd.Examples.Example1;

// Rule #1 - Enforce rules (invariants) within aggregate boundary
//
// The rules (invariants) we want to enforce is:
// 1. An order cannot be shipped if it has no order lines
// 2. An order cannot be changed once it is shipped

// In the following example, we encapsulate the Order's state and operations
// inside a single aggregate root `Order`. The aggregate ensures that all change
// to its internal state (`OrderLine` and shipping the order) maintains the
// business rules.

// Specifically:
//
// 1. We protect OrderLines list from being directly manipulated by making it
//    private and read-only.
//
// 2. To change the OrderLines, we expose the UpdateOrderLine method that
//    enforce the business rule of preventing change to a shipped order.
//
// 3. We do not expose any way to change the IsShipped property directly.
//    Instead, we provide a Ship() method that encapsulates the business rules
//    for shipping an order. This method ensures that an order cannot be shipped
//    if it doesn't have any business lines.

public class OrderLine
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }

    public OrderLine(Guid productId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        ProductId = productId;
        Quantity = quantity;
    }
}

public class Order
{
    public Guid Id { get; private set; }

    public IReadOnlyList<OrderLine> OrderLines { get; private set; } =
        Array.Empty<OrderLine>();

    public bool IsShipped { get; private set; }

    public Order(Guid id)
    {
        Id = id;
    }

    public void UpdateOrderLine(Guid productId, int quantity)
    {
        if (IsShipped)
            throw new InvalidOperationException("Cannot update order line for shipped orders.");

        if (quantity == 0)
        {
            // If quantity is zero, remove the order line

            OrderLines = OrderLines.Where(x => x.ProductId == productId).ToArray();
        }
        else
        {
            // Otherwise, update existing order line if there's one,
            // or add a new order line.

            var newOrderLine = new OrderLine(productId, quantity);

            var lines = OrderLines.ToList();
            var index = lines.FindIndex(x => x.ProductId == productId);
            if (index == -1)
                lines.Add(newOrderLine);
            else
                lines[index] = newOrderLine;

            OrderLines = lines;
        }
    }

    public void Ship()
    {
        if (OrderLines.Count == 0)
            throw new InvalidOperationException("Cannot ship an order without order lines.");

        IsShipped = true;
    }
}
