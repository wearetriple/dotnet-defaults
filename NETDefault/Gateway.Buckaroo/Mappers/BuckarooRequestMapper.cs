using Gateway.Buckaroo.Constants;
using Gateway.Buckaroo.Models.Dto.Requests;
using Gateway.Buckaroo.Models.Request;

namespace Gateway.Buckaroo.Mappers;

internal static class BuckarooRequestMapper
{
    internal static GetDebtorInfoRequestDto Map(GetDebtorRequest request)
        => new()
        {
            Services = new ServiceDto
            {
                ServiceList = new List<ServiceListDto>
                {
                    new ServiceListDto
                    {
                        Name = ParameterConstants.CreditManagement3,
                        Action = ParameterConstants.DebtorInfo,
                        Parameters = new List<ParameterDto>
                        {
                            new ParameterDto
                            {
                                Name = ParameterConstants.DebtorCode,
                                GroupType = ParameterConstants.Debtor,
                                GroupID = "",
                                Value = request.DebtorId.ToString()
                            }
                        }
                    }
                }
            }
        };

    internal static CreateCombinedSubscriptionRequestDto Map(CreateCombinedSubscriptionRequest request)
    {
        var subscriptionChargeParameters = new List<ParameterDto>
        {
            new ParameterDto
            {
                Name = ParameterConstants.RatePlanCode,
                GroupType = ParameterConstants.AddRatePlan,
                GroupID = "subscription",
                Value = request.Subscription.SubscriptionCharge.RatePlanCode
            },
            new ParameterDto
            {
                Name = ParameterConstants.RatePlanChargeCode,
                GroupType = ParameterConstants.AddRatePlanCharge,
                GroupID = "subscription",
                Value = request.Subscription.SubscriptionCharge.RatePlanChargeCode
            },
            new ParameterDto
            {
                Name = ParameterConstants.PricePerUnit,
                GroupType = ParameterConstants.AddRatePlanCharge,
                GroupID = "subscription",
                Value = request.Subscription.SubscriptionCharge.PricePerUnit.ToString(),
            },
            new ParameterDto
            {
                Name = ParameterConstants.VatPercentage,
                GroupType = ParameterConstants.AddRatePlanCharge,
                GroupID = "subscription",
                Value = request.Subscription.SubscriptionCharge.VatPercentage.ToString(),
            },
            new ParameterDto
            {
                Name = ParameterConstants.TransactionVatPercentage,
                GroupType = "",
                GroupID = "subscription",
                Value = "21",
            },
            new ParameterDto
            {
                Name = ParameterConstants.StartDate,
                GroupType = ParameterConstants.AddRatePlan,
                GroupID = "subscription",
                Value = request.Subscription.SubscriptionCharge.StartDate.ToString("dd-MM-yyyy"),
            },
            new ParameterDto
            {
                Name = ParameterConstants.EndDate,
                GroupType = ParameterConstants.AddRatePlan,
                GroupID = "subscription",
                Value = request.Subscription.SubscriptionCharge.EndDate == null ? "" : request.Subscription.SubscriptionCharge.EndDate.Value.ToString("dd-MM-yyyy")
            }
        };

        var customParameters = new ParameterCollectionDto
        {
            List = new List<CustomParameterDto>
            {
                new CustomParameterDto
                {
                    Name = ParameterConstants.Gender,
                    Value = "Other"
                }
            }
        };

        var debtorParameters = new List<ParameterDto>
        {
            new ParameterDto
            {
                Name = ParameterConstants.Code,
                GroupType = ParameterConstants.Debtor,
                GroupID = "",
                Value = request.Debtor.Id.ToString()
            },
            new ParameterDto
            {
                Name = ParameterConstants.FirstName,
                GroupType = ParameterConstants.Person,
                GroupID = "",
                Value = request.Debtor.FirstName
            },
            new ParameterDto
            {
                Name = ParameterConstants.LastName,
                GroupType = ParameterConstants.Person,
                GroupID = "",
                Value = request.Debtor.LastName
            },
            new ParameterDto
            {
                Name = ParameterConstants.Culture,
                GroupType = ParameterConstants.Person,
                GroupID = "",
                Value = "nl-NL"
            },
            new ParameterDto
            {
                Name = ParameterConstants.Street,
                GroupType = ParameterConstants.Address,
                GroupID = "",
                Value = request.Debtor.Address.Street
            },
            new ParameterDto
            {
                Name = ParameterConstants.HouseNumber,
                GroupType = ParameterConstants.Address,
                GroupID = "",
                Value = request.Debtor.Address.HouseNumber.ToString()
            },
            new ParameterDto
            {
                Name = ParameterConstants.HouseNumberSuffix,
                GroupType = ParameterConstants.Address,
                GroupID = "",
                Value = request.Debtor.Address.HouseNumberSuffix
            },
            new ParameterDto
            {
                Name = ParameterConstants.ZipCode,
                GroupType = ParameterConstants.Address,
                GroupID = "",
                Value = request.Debtor.Address.PostalCode
            },
            new ParameterDto
            {
                Name = ParameterConstants.City,
                GroupType = ParameterConstants.Address,
                GroupID = "",
                Value = request.Debtor.Address.City
            },
            new ParameterDto
            {
                Name = ParameterConstants.Country,
                GroupType = ParameterConstants.Address,
                GroupID = "",
                Value = "NL"
            },
            new ParameterDto
            {
                Name = ParameterConstants.Email,
                GroupType = ParameterConstants.Email,
                GroupID = "",
                Value = request.Debtor.EmailAddress
            },
            new ParameterDto
            {
                Name = ParameterConstants.Mobile,
                GroupType = ParameterConstants.Phone,
                GroupID = "",
                Value = request.Debtor.PhoneNumber
            }
        };

        var subscriptionRequestDto = new CreateCombinedSubscriptionRequestDto
        {
            Description = request.Subscription.Name,
            Currency = "EUR",
            StartRecurrent = "true",
            ContinueOnIncomplete = "1",
            AmountDebit = 10.00m,
            AmountCredit = 0,
            Invoice = request.Subscription.InvoiceDescription ?? "",
            CustomParameters = customParameters,
            Services = new ServiceDto
            {
                ServiceList = new List<ServiceListDto>
                    {
                        new ServiceListDto
                        {
                            Name = ParameterConstants.Subscriptions,
                            Action = ParameterConstants.CreateCombinedSubscription,
                            Parameters = new List<ParameterDto>
                            {
                                new ParameterDto
                                {
                                    Name = ParameterConstants.IncludeTransaction,
                                    GroupType = "",
                                    GroupID = "",
                                    Value = "true"
                                },
                                new ParameterDto
                                {
                                    Name = ParameterConstants.ConfigurationCode,
                                    Value = request.Subscription.ConfigurationCode
                                }
                            }
                        }
                    }
            }
        };

        foreach (var parameter in debtorParameters)
        {
            subscriptionRequestDto.Services.ServiceList.First().Parameters!.Add(parameter);
        }

        foreach (var parameter in subscriptionChargeParameters)
        {
            subscriptionRequestDto.Services.ServiceList.First().Parameters!.Add(parameter);
        }

        return subscriptionRequestDto;
    }
}
