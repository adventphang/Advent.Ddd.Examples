#nullable disable

namespace Advent.Ddd.Examples.Example5;

// No anaemic model
//
// The term "anemic" refers to a model where the entities primarily contain
// data (properties) with little or no behaviour (methods). The business logic
// ends up elsewhere in the application, often in service classes.

public class OrderLine
{
    public string Product { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public List<OrderLine> OrderLines { get; set; }
    public decimal Total { get; set; }
}

public class OrderAppService
{
    public class AddOrderLineInput
    {
        public int OrderId { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
    }

    public void AddOrderLine(AddOrderLineInput dto)
    {
        // Simplified: get order, get product, validate, create order line
        Order order = new();
        OrderLine orderLine = new();

        order.OrderLines.Add(orderLine);
        order.Total += orderLine.Quantity * orderLine.Price;
    }
}