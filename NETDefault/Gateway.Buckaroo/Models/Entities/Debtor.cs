namespace Gateway.Buckaroo.Models.Entities;

public class Debtor
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public Address Address { get; set; } = null!;

    public IEnumerable<string>? SubscriptionIds { get; set; }

    public IEnumerable<string>? InvoiceIds { get; set; }
}
