namespace Advent.Ddd.Examples.Example4;

// Reference other aggregates by their aggregate root identity
//
// In Domain-Driven Design, one of the key principles is to keep aggregates
// separate and only reference other aggregates by their identity. This practice
// promotes loose coupling and helps maintain the consistency boundary around
// each aggregate.
//
// Building up from the previous example, each order might be tied to an
// `Account`.
//
// Instead of referencing Account directly, we reference it by its ID.

// In the following example, the `Order` aggregate doesn't hold a direct
// reference to the `Account` aggregate. Instead, it just stores the
// `AccountId`. This means that the `Order` aggregate does not need to know
// about the inner workings or invariants of the `Account` aggregate.
// It also ensures that changes in the `Account` aggregate do not directly
// affect the `Order` aggregate, which promotes isolation and helps maintain
// each aggregate's consistency.

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
    public Guid? AccountId { get; private set; }

    public IReadOnlyList<OrderLine> OrderLines { get; private set; } =
        Array.Empty<OrderLine>();

    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsShipped { get; private set; }

    public Order(Guid id)
    {
        Id = id;
    }

    public void UpdateAccount(Guid accountId) => AccountId = accountId;

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
            // or add a new order line.

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

public class Account
{
    public Guid Id { get; private set; }

    public Account(Guid id)
    {
        Id = id;
    }
}