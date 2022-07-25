using Gateway.Buckaroo.Exceptions;
using Gateway.Buckaroo.Mappers;
using Gateway.Buckaroo.Models.Dto.Requests;
using Gateway.Buckaroo.Models.Dto.Responses;
using Gateway.Buckaroo.Models.Entities;
using Gateway.Buckaroo.Models.Request;
using Microsoft.Extensions.Logging;

namespace Gateway.Buckaroo;

public class BuckarooGateway : ISubscriptionGateway
{
    private readonly ILogger<BuckarooGateway> _logger;
    private readonly IPspClient _client;

    public BuckarooGateway(
        ILogger<BuckarooGateway> logger,
        IPspClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<Debtor?> GetDebtorAsync(GetDebtorRequest requestModel)
    {
        var buckarooRequest = BuckarooRequestMapper.Map(requestModel);

        try
        {
            var buckarooResponse = await _client.PostDataRequestAsync<GetDebtorInfoRequestDto, DebtorInfoResponseDto>(buckarooRequest);

            if (buckarooResponse.Status.Code.Description.Contains("Success"))
            {
                _logger.LogInformation("Succesfully found debtor {id}", requestModel.DebtorId);

                return BuckarooResponseMapper.Map(buckarooResponse);
            }

            if (buckarooResponse.Status.SubCode.Description.Contains("The debtor is not found"))
            {
                _logger.LogInformation("Failed to find debtor {id}", requestModel.DebtorId);

                return null;
            }

            _logger.LogWarning("Failed to find debtor {id}", requestModel.DebtorId);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            throw;
        }
    }

    public async Task<string?> CreateCombinedSubscriptionAsync(CreateCombinedSubscriptionRequest requestModel)
    {
        var buckarooRequest = BuckarooRequestMapper.Map(requestModel);

        try
        {
            var buckarooResponse = await _client.PostTransactionAsync<CreateCombinedSubscriptionRequestDto, CreateCombinedSubscriptionResponseDto>(buckarooRequest);

            if (buckarooResponse.Status.Code.Description.Contains("Pending input"))
            {
                return buckarooResponse.RequiredAction.RedirectURL;
            }

            throw new BuckarooGatewayException(buckarooResponse.Status.Code.Description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gateway {source} threw an error", nameof(ISubscriptionGateway));

            throw;
        }
    }
}
