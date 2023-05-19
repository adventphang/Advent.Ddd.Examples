namespace Advent.Ddd.Examples.Example6;

// Method represents domain behaviour
//
// In a basic or naive model, we might have operations (methods) like
// SetBalance for a BankAccount. However, these don't represent meaningful
// behaviours in the banking domain.
//
// In banking, we talk about Depositing and Withdrawing money,
// not increasing or decreasing a balance.
//
// When naming methods, follow the best practices:
//
// 1. Use uniquitous language - if domain experts call a process "Terminate",
//    name the same method as "Terminate". This ensures model accurately
//    reflects the domain, and is understandable to both developers and domain
//    experts.
//
// 2. Start with a Verb - Operations should start with a verb, because they
//    perform an action or behaviour. Examples: Cancel, Ship, Terminate
//
// 3. Avoid CRUD-style naming - Name should reflect business operations.
//    Instead of SetTerminationTime, use Terminate
//    Instead of Update, use ChangeBillingFrequency, ChangeBillingAddress
//    Instead of SaveOrder, use PlaceOrder
//
// Finally, a domain method generally raises a domain event.

public class BankAccount : BaseAggregateRoot
{
    public string CustomerName { get; private set; } = null!;
    public decimal Balance { get; private set; }

    #region Constructors

    private BankAccount(Guid id, string customerName)
        : base(id)
    {
        CustomerName = customerName;
    }

    #endregion

    #region Don't do this - this doesn't represent domain behaviour

    // The following methods changes the state without encapsulating business rules

    public void SetBalance(decimal balance)
    {
        Balance = balance;
    }

    public void SetCustomer(string customerName)
    {
        CustomerName = customerName;
    }

    #endregion

    #region Do this instead - this represents a domain behaviour

    public static BankAccount Open(Guid id, string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Invalid customer name"); 

        var account = new BankAccount(id, customerName);
        account.AddDomainEvent(new BankAccountOpened(id, customerName));
        return account;
    }

    public void RenameCustomer(string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Invalid customer name");

        if (customerName == CustomerName)
            return;

        CustomerName = customerName;
        AddDomainEvent(new BankAccountCustomerRenamed(Id, CustomerName));
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive.", nameof(amount));

        Balance += amount;

        AddDomainEvent(new BankAccountDeposited(Id, amount, Balance));
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Withdraw amount must be positive.", nameof(amount));
        if (Balance < amount)
            throw new InvalidOperationException("Insufficient balance.");

        Balance -= amount;

        AddDomainEvent(new BankAccountWithdrawn(Id, amount, Balance));
    }

    #endregion
}

public record BankAccountOpened(
    Guid BankAccountId,
    string CustomerName);

public record BankAccountCustomerRenamed(
    Guid BankAccountId,
    string CustomerName);

public record BankAccountDeposited(
    Guid BankAccountId,
    decimal Amount,
    decimal NewBalance);

public record BankAccountWithdrawn(
    Guid BankAccountId,
    decimal Amount,
    decimal NewBalance);