using AndC.Platform.Gateways.Buckaroo.Extensions;
using Gateway.Buckaroo.Constants;
using Gateway.Buckaroo.Models.Dto.Responses;
using Gateway.Buckaroo.Models.Entities;

namespace Gateway.Buckaroo.Mappers;

internal static class BuckarooResponseMapper
{
    internal static Debtor Map(DebtorInfoResponseDto response)
    {
        var parameters = response.GetServiceParameters();

        return new Debtor
        {
            Id = parameters.GetGuidByName(ParameterConstants.Code),
            FirstName = parameters.GetStringByName(ParameterConstants.FirstName),
            LastName = parameters.GetStringByName(ParameterConstants.LastName),
            EmailAddress = parameters.GetStringByName(ParameterConstants.Email),
            PhoneNumber = parameters.GetStringByName(ParameterConstants.Mobile),
            Address = new Address
            {
                Street = parameters.GetStringByName(ParameterConstants.Street),
                HouseNumber = parameters.GetIntByName(ParameterConstants.HouseNumber),
                HouseNumberSuffix = parameters.GetStringByName(ParameterConstants.HouseNumberSuffix),
                PostalCode = parameters.GetStringByName(ParameterConstants.ZipCode),
                City = parameters.GetStringByName(ParameterConstants.City),
            },
            SubscriptionIds = parameters.GetStringCollectionByName(ParameterConstants.SubscriptionIds),
            InvoiceIds = parameters.GetStringCollectionByName(ParameterConstants.InvoiceIds),
        };
    }
}
