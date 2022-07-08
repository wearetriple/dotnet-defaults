namespace Gateway.Buckaroo.Models.Entities;

public class Address
{
    public string Street { get; set; } = null!;

    public int HouseNumber { get; set; }

    public string HouseNumberSuffix { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string City { get; set; } = null!;
}
