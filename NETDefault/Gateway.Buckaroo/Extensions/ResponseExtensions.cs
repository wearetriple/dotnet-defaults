using System.Globalization;
using System.Text.Json;
using Gateway.Buckaroo.Exceptions;
using Gateway.Buckaroo.Models.Dto.Responses;
using ParameterDto = Gateway.Buckaroo.Models.Dto.Responses.ParameterDto;

namespace AndC.Platform.Gateways.Buckaroo.Extensions;

internal static class ResponseExtensions
{
    private static IFormatProvider _formatProvider = new CultureInfo("en-US");

    internal static IEnumerable<ParameterDto> GetServiceParameters(this IBuckarooResponseDto response)
        => response?.Services?.First().Parameters?.OfType<ParameterDto>().ToList()
            ?? throw new BuckarooGatewayException("Failed to get response Parameters");

    internal static string GetStringByName(this IEnumerable<ParameterDto> parameters, string parameterName)
    {
        try
        {
            return parameters.Single(p => p.Name == parameterName).Value ?? "";
        }
        catch (ArgumentNullException)
        {
            throw new BuckarooGatewayException($"{nameof(GetStringByName)}: parameter collection is null");
        }
        catch (InvalidOperationException)
        {
            throw new BuckarooGatewayException($"{nameof(GetStringByName)}: failed to locate the single item for {parameterName}");
        }
    }

    internal static bool GetBooleanByName(this IEnumerable<ParameterDto> parameters, string parameterName)
    {
        try
        {
            var stringValue = GetStringByName(parameters, parameterName);
            return bool.Parse(stringValue);
        }
        catch (FormatException)
        {
            throw new BuckarooGatewayException($"{nameof(GetBooleanByName)} : failed to parse {parameterName} as bool");
        }
    }

    internal static Guid GetGuidByName(this IEnumerable<ParameterDto> parameters, string parameterName)
    {
        try
        {
            var stringValue = GetStringByName(parameters, parameterName);
            return Guid.Parse(stringValue);
        }
        catch (FormatException)
        {
            throw new BuckarooGatewayException($"{nameof(GetGuidByName)} : failed to parse {parameterName} as Guid");
        }
    }

    internal static T GetEnumByName<T>(this IEnumerable<ParameterDto> parameters, string parameterName) where T : struct, Enum
    {
        try
        {
            var stringValue = GetStringByName(parameters, parameterName);
            return Enum.Parse<T>(stringValue);
        }
        catch (ArgumentException)
        {
            throw new BuckarooGatewayException($"{nameof(GetEnumByName)} : failed to parse {parameterName} as Enum");
        }
    }

    internal static int GetIntByName(this IEnumerable<ParameterDto> parameters, string parameterName)
    {
        try
        {
            var stringValue = GetStringByName(parameters, parameterName);
            return int.Parse(stringValue);
        }
        catch (OverflowException)
        {
            throw new BuckarooGatewayException($"{nameof(GetIntByName)} : {parameterName} is bigger/smaller than int max/min values");
        }
        catch (FormatException)
        {
            throw new BuckarooGatewayException($"{nameof(GetIntByName)} : failed to parse {parameterName} as int");
        }
    }

    internal static decimal GetDecimalByName(this IEnumerable<ParameterDto> parameters, string parameterName)
    {
        try
        {
            var stringValue = GetStringByName(parameters, parameterName);
            return decimal.Parse(stringValue, _formatProvider);
        }
        catch (OverflowException)
        {
            throw new BuckarooGatewayException($"{nameof(GetDecimalByName)} : is bigger/smaller than decimal max/min values");
        }
        catch (FormatException)
        {
            throw new BuckarooGatewayException($"{nameof(GetDecimalByName)} : failed to parse {parameterName} as decimal");
        }
    }

    internal static IEnumerable<string> GetStringCollectionByName(this IEnumerable<ParameterDto> parameters, string parameterName)
    {
        try
        {
            var stringValue = GetStringByName(parameters, parameterName);
            return stringValue.Replace("\"", "").Split(',').ToList();
        }
        catch (OverflowException)
        {
            throw new BuckarooGatewayException($"{nameof(GetStringCollectionByName)} : failed to split up {parameterName} into a collection");
        }
    }

    internal static T? GetObjectByName<T>(this IEnumerable<ParameterDto> parameters, string parameterName)
    {
        try
        {
            var stringValue = parameters.Single(p => p.Name == parameterName).Value;
            return JsonSerializer.Deserialize<T>(stringValue);
        }
        catch (JsonException)
        {
            throw new BuckarooGatewayException($"{nameof(GetObjectByName)} : failed to deserialize to {nameof(T)} because of invalid Json");
        }
        catch (Exception)
        {
            return default;
        }
    }
}
