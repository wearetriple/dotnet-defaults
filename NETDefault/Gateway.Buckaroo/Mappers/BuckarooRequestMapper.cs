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
                            Name = "CreditManagement3",
                            Action = "DebtorInfo",
                            Parameters = new List<ParameterDto>
                            {
                                new ParameterDto
                                {
                                    Name = "DebtorCode",
                                    GroupType = "Debtor",
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
                Name = "RatePlanCode",
                GroupType = "AddRatePlan",
                GroupID = "subscription",
                Value = request.Subscription.SubscriptionCharge.RatePlanCode
            },
            new ParameterDto
            {
                Name = "RatePlanChargeCode",
                GroupType = "AddRatePlanCharge",
                GroupID = "subscription",
                Value = request.Subscription.SubscriptionCharge.RatePlanChargeCode
            },
            new ParameterDto
            {
                Name = "PricePerUnit",
                GroupType = "AddRatePlanCharge",
                GroupID = "subscription",
                Value = request.Subscription.SubscriptionCharge.PricePerUnit.ToString(),
            },
            new ParameterDto
            {
                Name = "VatPercentage",
                GroupType = "AddRatePlanCharge",
                GroupID = "subscription",
                Value = request.Subscription.SubscriptionCharge.VatPercentage.ToString(),
            },
            new ParameterDto
            {
                Name = "TransactionVatPercentage",
                GroupType = "",
                GroupID = "subscription",
                Value = "21",
            },
            new ParameterDto
            {
                Name = "StartDate",
                GroupType = "AddRatePlan",
                GroupID = "subscription",
                Value = request.Subscription.SubscriptionCharge.StartDate.ToString("dd-MM-yyyy"),
            },
            new ParameterDto
            {
                Name = "EndDate",
                GroupType = "AddRatePlan",
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
                    Name = "Gender",
                    Value = "Other"
                }
            }
        };

        var debtorParameters = new List<ParameterDto>
        {
            new ParameterDto
            {
                Name = "Code",
                GroupType = "Debtor",
                GroupID = "",
                Value = request.Debtor.Id.ToString()
            },
            new ParameterDto
            {
                Name = "FirstName",
                GroupType = "Person",
                GroupID = "",
                Value = request.Debtor.FirstName
            },
            new ParameterDto
            {
                Name = "LastName",
                GroupType = "Person",
                GroupID = "",
                Value = request.Debtor.LastName
            },
            new ParameterDto
            {
                Name = "Culture",
                GroupType = "Person",
                GroupID = "",
                Value = "nl-NL"
            },
            new ParameterDto
            {
                Name = "Street",
                GroupType = "Address",
                GroupID = "",
                Value = request.Debtor.Address.Street
            },
            new ParameterDto
            {
                Name = "HouseNumber",
                GroupType = "Address",
                GroupID = "",
                Value = request.Debtor.Address.HouseNumber.ToString()
            },
            new ParameterDto
            {
                Name = "HouseNumberSuffix",
                GroupType = "Address",
                GroupID = "",
                Value = request.Debtor.Address.HouseNumberSuffix
            },
            new ParameterDto
            {
                Name = "ZipCode",
                GroupType = "Address",
                GroupID = "",
                Value = request.Debtor.Address.PostalCode
            },
            new ParameterDto
            {
                Name = "City",
                GroupType = "Address",
                GroupID = "",
                Value = request.Debtor.Address.City
            },
            new ParameterDto
            {
                Name = "Country",
                GroupType = "Address",
                GroupID = "",
                Value = "NL"
            },
            new ParameterDto
            {
                Name = "Email",
                GroupType = "Email",
                GroupID = "",
                Value = request.Debtor.EmailAddress
            },
            new ParameterDto
            {
                Name = "Mobile",
                GroupType = "Phone",
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
                            Name = "Subscriptions",
                            Action = "CreateCombinedSubscription",
                            Parameters = new List<ParameterDto>
                            {
                                new ParameterDto
                                {
                                    Name = "IncludeTransaction",
                                    GroupType = "",
                                    GroupID = "",
                                    Value = "true"
                                },
                                new ParameterDto
                                {
                                    Name = "ConfigurationCode",
                                    Value = request.Subscription.ConfigurationCode
                                }
                            }
                        }
                    }
            }
        };

        foreach (var parameter in debtorParameters)
        {
            subscriptionRequestDto.Services.ServiceList.FirstOrDefault()!.Parameters!.Add(parameter);
        }

        foreach (var parameter in subscriptionChargeParameters)
        {
            subscriptionRequestDto.Services.ServiceList.FirstOrDefault()!.Parameters!.Add(parameter);
        }

        return subscriptionRequestDto;
    }
}
