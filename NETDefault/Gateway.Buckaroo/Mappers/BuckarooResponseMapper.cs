using Gateway.Buckaroo.Models.Dto.Responses;
using Gateway.Buckaroo.Models.Entities;

namespace Gateway.Buckaroo.Mappers;

internal static class BuckarooResponseMapper
{
    internal static Debtor Map(DebtorInfoResponseDto response)
    {
        var id = response?.Services?.FirstOrDefault(s => s.Name == "CreditManagement3")?.Parameters?.FirstOrDefault(p => p.Name == "Code")?.Value;
        var guid = string.IsNullOrWhiteSpace(id)
            ? Guid.Empty
            : Guid.Parse(id);

        return new Debtor
        {
            Id = guid,
            FirstName = response?.Services?.FirstOrDefault(s => s.Name == "CreditManagement3")?.Parameters?.FirstOrDefault(p => p.Name == "FirstName")?.Value ?? "",
            LastName = response?.Services?.FirstOrDefault(s => s.Name == "CreditManagement3")?.Parameters?.FirstOrDefault(p => p.Name == "LastName")?.Value ?? "",
            EmailAddress = response?.Services?.FirstOrDefault(s => s.Name == "CreditManagement3")?.Parameters?.FirstOrDefault(p => p.Name == "Email")?.Value ?? "",
            PhoneNumber = response?.Services?.FirstOrDefault(s => s.Name == "CreditManagement3")?.Parameters?.FirstOrDefault(p => p.Name == "Mobile")?.Value ?? "",
            Address = new Address
            {
                Street = response?.Services?.FirstOrDefault(s => s.Name == "CreditManagement3")?.Parameters?.FirstOrDefault(p => p.Name == "Street")?.Value ?? "",
                HouseNumber = int.Parse(response?.Services?.FirstOrDefault(s => s.Name == "CreditManagement3")?.Parameters?.FirstOrDefault(p => p.Name == "HouseNumber")?.Value!),
                HouseNumberSuffix = response?.Services?.FirstOrDefault(s => s.Name == "CreditManagement3")?.Parameters?.FirstOrDefault(p => p.Name == "HouseNumberSuffix")?.Value ?? "",
                PostalCode = response?.Services?.FirstOrDefault(s => s.Name == "CreditManagement3")?.Parameters?.FirstOrDefault(p => p.Name == "ZipCode")?.Value ?? "",
                City = response?.Services?.FirstOrDefault(s => s.Name == "CreditManagement3")?.Parameters?.FirstOrDefault(p => p.Name == "City")?.Value ?? "",
            }
        };
    }
}
