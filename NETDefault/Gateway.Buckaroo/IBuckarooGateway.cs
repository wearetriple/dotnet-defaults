using Gateway.Buckaroo.Models.Entities;
using Gateway.Buckaroo.Models.Request;

namespace Gateway.Buckaroo;

public interface ISubscriptionGateway
{
    public Task<Debtor?> GetDebtorAsync(GetDebtorRequest request);

    public Task<string?> CreateCombinedSubscriptionAsync(CreateCombinedSubscriptionRequest request);
}
