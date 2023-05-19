namespace Advent.Ddd.Examples.Example2;

// Always consistent
//
// The principle of keeping the aggregate root responsible for the integrity
// of the aggregate ensures that all interactions with members of the aggregate
// (entities or value objects) are done through the root.
// This allows the aggregate root to maintain the business rules.
//
// Building up from the previous example, there are a few additional business
// rules:
// 1. Add unit price to each order line.
// 2. Add a total discount to the order.
// 3. Discount must be zero or more.
// 4. Track the total order amount so we don't have to recalculate each time.
//
// In the following example, we maintain the integrity of the aggregate as
// follows:
//
// 1. `OrderLines` are not exposed for motification. A readonly list is exposed
//    for queries. This ensures that the order lines cannot be added, removed or
//    changed from outside the `Order`.
//
// 2. `OrderLine` does not expost any public setters where the property values
//    can be modified. Technically, `Order` aggregate root has the authority to
//    change this value. However, the setters or methods cannot be exposed
//    exclusively for `Order` only. A popular compromise here it to make the
//    class internal.
//
// 3. Operations that change the state of the aggregate (`UpdateOrderLine` and
//    `UpdateDiscount`, `Ship`) are implemented in the aggregate root.
//    They ensures the business rules are satisfied.
//
// 4. The total amount of the order is calculated within the aggregate, ensuring
//    that it always stays consistent with the order lines and discount.

public class OrderLine
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public OrderLine(Guid productId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}

public class Order
{
    public Guid Id { get; private set; }

    public IReadOnlyList<OrderLine> OrderLines { get; private set; } =
        Array.Empty<OrderLine>();

    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsShipped { get; private set; }

    public Order(Guid id)
    {
        Id = id;
    }

    public void UpdateDiscount(decimal discount)
    {
        if (discount < 0)
            throw new ArgumentException("Discount must be zero or more");

        EnsureNotShipped();

        Discount = discount;

        UpdateTotalAmount();
    }

    public void UpdateOrderLine(Guid productId, int quantity, decimal unitPrice)
    {
        EnsureNotShipped();

        if (quantity == 0)
        {
            // If quantity is zero, remove the order line

            OrderLines = OrderLines.Where(x => x.ProductId == productId).ToArray();
        }
        else
        {
            // Otherwise, update existing order line if there's one,
            // or add a new order line. We attempt to maintain the order line
            // ordering

            var newOrderLine = new OrderLine(productId, quantity, unitPrice);

            var lines = OrderLines.ToList();
            var index = lines.FindIndex(x => x.ProductId == productId);
            if (index == -1)
                lines.Add(newOrderLine);
            else
                lines[index] = newOrderLine;

            OrderLines = lines;
        }

        // Update the total amount

        UpdateTotalAmount();
    }

    public void Ship()
    {
        if (OrderLines.Count == 0)
            throw new InvalidOperationException("Cannot ship an order without order lines.");

        IsShipped = true;
    }

    private void EnsureNotShipped()
    {
        if (IsShipped)
            throw new InvalidOperationException("Cannot update order line for shipped orders.");
    }

    private void UpdateTotalAmount()
    {
        TotalAmount = OrderLines.Sum(x => x.UnitPrice * x.Quantity) - Discount;
    }
}
