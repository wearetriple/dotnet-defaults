using Gateway.Buckaroo.Models.Entities;

namespace Gateway.Buckaroo.Models.Request;

public class PurchaseRequest
{
    public Debtor Debtor { get; set; } = null!;

    public Subscription Subscription { get; set; } = null!;
}
