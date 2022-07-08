using System.ComponentModel.DataAnnotations;

namespace Gateway.Buckaroo.Models;

public class CreateCombinedRequestModel
{
    [Required]
    public Debtor DebtorModel { get; set; } = null!;

    [Required]
    public Subscription SubscriptionModel { get; set; } = null!;

    public class Debtor
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public string EmailAddress { get; set; } = null!;

        [Required]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public Address Address { get; set; } = null!;
    }

    public class Address
    {
        [Required]
        public string Street { get; set; } = null!;

        [Required]
        public int HouseNumber { get; set; }

        public string HouseNumberSuffix { get; set; } = "";

        [Required]
        public string PostalCode { get; set; } = null!;

        [Required]
        public string City { get; set; } = null!;
    }

    public class Subscription
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public string InvoiceDescription { get; set; } = null!;

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public decimal VatPercentage { get; set; }

        public string? StartDate { get; set; }

        public string? EndDate { get; set; }

        public string? RatePlanCode { get; set; }

        public string? RatePlanChargeCode { get; set; }

        [Required]
        public string ConfigurationCode { get; set; } = null!;

        [Required]
        public Guid DebtorId { get; set; }

        [Required]
        public string DebtorFirstName { get; set; } = null!;

        [Required]
        public string DebtorLastName { get; set; } = null!;

        [Required]
        public string ChannelCode { get; set; } = null!;

        [Required]
        public string OriginCode { get; set; } = null!;

        [Required]
        public bool HasPremiumAccess { get; set; }

        [Required]
        public bool HasPhysicalMagazine { get; set; }

        public Beneficiary? Beneficiary { get; set; } = null!;

        public Address? InvoiceAddress { get; set; } = null!;

        [Required]
        public string SubscriptionType { get; set; } = null!;
    }

    public class Dates
    {
        [Required]
        public int DurationInMonths { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }

    public class Beneficiary
    {
        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public Address Address { get; set; } = null!;
    }
}
