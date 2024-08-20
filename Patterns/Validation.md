# Validation

## Data Annotation Validators

Validating data using [Data Annotation Validators](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations) is a simple but powerful way of validating incoming data or settings. It provides a convenient amount of validators that can be assigned to properties using attributes. 

```c#
public class Data 
{
    [MinLength(10)]
    public string? Name { get; set; }

    [Required]
    public IEnumerable<Data> Children { get; set; }

    [Required]
    public Data Parent { get; set; }

    [Required, JsonConverter(typeof(DateConverter))] // Uses the default constructor
    public DateTime StartDate { get; set; }

    [Required, JsonConverter(typeof(DateConverter), "yyyy-MM")] // Overloads the constructor with a custom format
    public DateTime EndMonth { get; set; }
}
```

When using Data Annotation Validators please keep in mind:

- Use `[Required]` to make an property required. `null` is a valid value for `[MinLength(10)]` as it ignores `null`.
- Nested objects (like Parent) and arrays (like Children) are not validated (see below).
- Data Annotation Validators are sync only. If `async` is required, use something like `FluentValidation`. Keep in mind that `ASP.NET Core` will always validate sync, `async` validation should always be implemented inside a controller.

### Validating nested objects and arrays

Data Annotation Validators validate the property, not the object within the property. The validating `Data` in the example above will not validate `Children`, but only the array of `Children`. If that property has `[MaxLength(2)]`, the amount of children are validated, but every child can be invalid. To validate deeper into the graph, update the `Data` model as follows:

```c#
public class Data 
{
    [MinLength(10)]
    public string? Name { get; set; }

    [Required]
    [ValidateEnumerable]
    public IEnumerable<Data> Children { get; set; }

    [Required]
    [ValidateObject]
    public Data Parent { get; set; }
}
```

The `[ValidateEnumerable]` and `[ValidateObject]` attributes instruct the validator to go into the enumerable or object. Error messages from the deeper properties are grouped under the error message of the `Children` and `Parent` properties. The implementation of these attributes can be found at the end of this document.

### Validating dates

Validating dates in a concise way can be achieved using specific JsonConverter classes. In the above example you can find the property `StartDate` of type `DateTime`, using the default constructor for DateConverter. You can overload the constructor with a custom format as shown by the `EndMonth` property. The `JsonConverter` makes use of the class `DateConverter`:

```c#
public class DateConverter : IsoDateTimeConverter
{
        public DateConverter()
        {
            DateTimeFormat = "yyyy-MM-dd";
        }

        public DateConverter(string format)
        {
            DateTimeFormat = format;
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            try
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
            catch
            {
                return null;
            }
        }
}
```

:warning: Be aware that deserialization occurs before validation, which means that `2000-01-01` cannot be parsed with the format `yyyy-MM` which will result in an exception. The `try-catch` block ensures `null` is returned in these cases, which causes the `DateConverter` to default to `DateTime.MinValue`.
Mitigating this can be done by adding a `[Range]` to catch the `DateTime.Minvalue`.

### Validating strings using Regex

Input validation with regex can be achieved by the use of attributes, inheriting from `RegularExpressionAttribute` Note that the regex pattern has been declared in the `base`

```c#
    public class StormtrooperIdAttribute : RegularExpressionAttribute
    {
        public StormtrooperIdAttribute() : base("^\\d{7}$")
        {
            ErrorMessage = "Invalid Stormtrooper Id: should consist of 7 numbers";
        }
    }
```

## Nested validation

```c#
public class CompositeValidationResult : ValidationResult
{
    public string? MemberName { get; private set; }
    public List<ValidationResult> Results { get; private set; } = new List<ValidationResult>();

    public CompositeValidationResult(string errorMessage, string? memberName = default) : base(errorMessage) { MemberName = memberName; }
}

/// <summary>
/// This attribute will instruct the entity validator to also validate the properties of this object, instead of just the object itself.
/// </summary>
public class ValidateObjectAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        var results = new List<ValidationResult>();
        var context = new ValidationContext(value, validationContext, null);

        Validator.TryValidateObject(value, context, results, true);

        if (results.Count != 0)
        {
            var compositeResults = new CompositeValidationResult($"Validation for {validationContext.DisplayName} failed!", validationContext.MemberName ?? "Unknown member");
            compositeResults.Results.AddRange(results);

            return compositeResults;
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// This attribute will instruct the entity validator to also validate each of the elements of this enumerable, instead of just the enumerable itself.
/// </summary>
public class ValidateEnumerableAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is not IEnumerable enumerable)
        {
            return new ValidationResult($"{nameof(ValidateEnumerableAttribute)} can only be used on IEnumerable's");
        }

        var results = new List<ValidationResult>();

        foreach (var item in enumerable)
        {
            var context = new ValidationContext(item, validationContext, null);

            Validator.TryValidateObject(item, context, results, true);
        }

        var compositeResults = new CompositeValidationResult($"Validation for {validationContext.DisplayName} failed!", validationContext.MemberName ?? "Unknown member");
        if (results.Count != 0)
        {
            compositeResults.Results.AddRange(results);
            return compositeResults;
        }

        return ValidationResult.Success;
    }
}
```
