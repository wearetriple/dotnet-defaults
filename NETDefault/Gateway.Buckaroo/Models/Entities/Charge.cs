namespace Gateway.Buckaroo.Models.Entities;

public class Charge
{
    public string RatePlanCode { get; set; } = null!;
    public string RatePlanChargeCode { get; set; } = null!;
    public decimal PricePerUnit { get; set; }
    public decimal VatPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
