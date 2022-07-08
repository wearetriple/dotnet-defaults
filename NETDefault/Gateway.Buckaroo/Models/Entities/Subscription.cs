namespace Gateway.Buckaroo.Models.Entities;

public class Subscription
{
    public string Id { get; set; } = null!;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? InvoiceDescription { get; set; }
    public bool? IsRecurring { get; set; }
    public string? DebtorId { get; set; }
    public string? DebtorName { get; set; }
    public string? NextRunDate { get; set; }
    public string? Status { get; set; }
    public Charge SubscriptionCharge { get; set; } = null!;
    public Address? InvoiceAddress { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? RatePlanCode { get; set; }
    public string? ConfigurationCode { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentCardNumber { get; set; }
    public string? PaymentMethodConfigurationStatus { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal VatPercentage { get; set; }
}
