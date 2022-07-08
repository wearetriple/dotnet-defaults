using Gateway.Buckaroo.Models.Entities;

namespace Gateway.Buckaroo.Models.Request;

public class CreateCombinedSubscriptionRequest
{
    public Debtor Debtor { get; set; } = null!;

    public Subscription Subscription { get; set; } = null!;
}
