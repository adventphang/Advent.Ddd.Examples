namespace Advent.Ddd.Examples;

public abstract class BaseAggregateRoot
{
    public Guid Id { get; private set; }

    public BaseAggregateRoot()
    {
        // EMPTY
    }

    public BaseAggregateRoot(Guid id)
    {
        Id = id;
    }

    protected void AddDomainEvent(object domainEvent)
    {
        // Function to store domain events
    }
}

