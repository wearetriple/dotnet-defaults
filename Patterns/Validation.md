# Validation

We tend to use 2 flavors of validation:

- [Data Annotation Validation](#data-annotation-validation). Use when validation 
is very simple and the classes are fully owned by the application.
- [Fluent Validation](#fluent-validation). Use this when the requirements are slightly
more complex, has more logic, frequently needs to compare multiple properties at 
the same time. Fluent Validation also removes the validation from the class definition,
which helps when the class definition is not fully owned by the application.

## Data Annotation Validation

Validating data using [Data Annotation Validators](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)
is a simple but powerful way of validating incoming data or settings. It provides 
a convenient amount of validators that can be assigned to properties using attributes. 

```c#
public record Data 
{
    [MinLength(10)]
    public string? Name { get; set; }

    [Required]
    public IEnumerable<Data> Children { get; set; } = null!; // using required here prevents Data from being validated as it cannot be deserialized

    [Required]
    public Data Parent { get; set; } = null!;

    [Required, JsonConverter(typeof(DateConverter))]
    public DateTime StartDate { get; set; }

    [Required, JsonConverter(typeof(DateConverter), "yyyy-MM")]
    public DateTime EndMonth { get; set; }
}
```

Validating a model can be done by using `Validator.TryValidateObject`. This will 
add validation results in the given `validationResults`. Don't forget to set 
`validateAllProperties` to `true`, otherwise not all properties are validated.

```c#
var context = new ValidationContext(obj);
var validationResults = new List<ValidationResult>();

var isValid = Validator.TryValidateObject(obj, context, validationResults, validateAllProperties: true);
```

When using Data Annotation Validators please keep in mind:

- Use `[Required]` to make an property required. `null` is a valid value for 
`[MinLength(10)]` as it ignores `null`.
- `[Required]` and `required` are two different things. Making properties not `required`
allows these properties to be validated by `[Required]`.
- Nested objects (like `Parent`) and arrays (like `Children`) are not validated
out of the box (see below).

### Validate entire objects

In some cases separate validation of each property is not good enough. Use the following
two strategies for those cases:

#### `IValidatableObject`

Add the interface `IValidatableObject`, and implement the `Validate` method:

```c#
public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
{
    if (StartDate < EndMonth)
    {
        yield return new ValidationResult("StartDate must be before EndMonth");
    }
}
```

This method is invoked when all validation attributes succeed.

#### Custom validation attribute

When adding an interface is not desirable, or when the validation should be reusable,
a custom validation attribute can be created. Keep in mind that `null` should always
be accepted as valid by this validation attribute, and that the given `value` needs
to be cast first.

```c#
public class DataValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        if (value is not Data data)
        {
            return new ValidationResult($"Attribute is only valid for {nameof(Data)}");
        }

        if (data.StartDate > data.EndMonth)
        {
            return new ValidationResult("StartDate must be before EndMonth");
        }

        return ValidationResult.Success;
    }
}
```

### Validating nested objects and arrays

Data Annotation Validators validate the property, not the object within the property. 
The validating `Data` in the example above will not validate `Children`, but only 
the array of `Children`. If that property has `[MaxLength(2)]`, the amount of children 
are validated, but every child can be invalid. To validate deeper into the graph, 
update the `Data` model as follows:

```c#
public record Data 
{
    [MinLength(10)]
    public string? Name { get; set; }

    [Required, ValidateEnumerable]
    public IEnumerable<Data> Children { get; set; } = null!;

    [Required, ValidateObject]
    public Data Parent { get; set; } = null!;
}
```

#### Nested validation

The `ValidateEnumerable` and `ValidateObject` attributes instruct the validator 
to go into the enumerable or object. Error messages from the deeper properties are 
grouped under the error message of the `Children` and `Parent` properties. The implementation 
of these attributes can be found at the end of this document.

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

        if (Validator.TryValidateObject(value, context, results, true)) 
        {
            return ValidationResult.Success;
        }

        var compositeResults = new CompositeValidationResult($"Validation for {validationContext.DisplayName} failed!", validationContext.MemberName ?? "Unknown member");
        compositeResults.Results.AddRange(results);

        return compositeResults;
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

        if (results.Count == 0)
        {
            return ValidationResult.Success;
        }

        var compositeResults = new CompositeValidationResult($"Validation for {validationContext.DisplayName} failed!", validationContext.MemberName ?? "Unknown member");
        compositeResults.Results.AddRange(results);
        return compositeResults;
    }
}
```

### Validating dates

Validating dates in a concise way can be achieved using specific JsonConverter 
classes. In the above example you can find the property `StartDate` of type `DateTime`, 
using the default constructor for DateConverter. You can overload the constructor 
with a custom format as shown by the `EndMonth` property. The `JsonConverter` makes 
use of the class `DateConverter`:

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

:warning: Be aware that deserialization occurs before validation, which means that 
`2000-01-01` cannot be parsed with the format `yyyy-MM` as it will result in an 
exception. The `try-catch` block ensures `null` is returned in these cases, which 
causes the `DateConverter` to default to `DateTime.MinValue`. Mitigating this can 
be done by adding a `[Range]` to catch the `DateTime.MinValue`.

### Validating strings using Regex

Input validation with regex can be achieved by the use of attributes, inheriting 
from `RegularExpressionAttribute` Note that the regex pattern has been declared 
in the `base`

```c#
public class StormtrooperIdAttribute : RegularExpressionAttribute
{
    public StormtrooperIdAttribute() : base("^\\d{7}$")
    {
        ErrorMessage = "Invalid Stormtrooper Id: should consist of 7 numbers";
    }
}
```

## Fluent Validation

[Fluent Validation](https://docs.fluentvalidation.net/en/latest/) is a NuGet package
that replaces Data Annotation Validation by classes containing strongly typed validation
rules using a fluent API. An identical validator for the example class `Data` from 
above is as follows:

```c#
public class DataFluentValidator : AbstractValidator<Data>
{
    public DataFluentValidator()
    {
        RuleFor(x => x.Name).MinimumLength(10);
        
        // NotNull is similar to [Required]
        // SetValidator sets the validator for Parent, which is not validated otherwise
        RuleFor(x => x.Parent).NotNull().SetValidator(this);

        // SetValidator needs to be set for each child inside the array
        RuleFor(x => x.Children).NotNull().ForEach(child => child.SetValidator(this));

        RuleFor(x => x.StartDate).NotNull().LessThan(x => x.EndMonth);
        
        RuleFor(x => x.EndMonth).NotNull();
    }
}
```

Using this validator is as follows:

```c#
var validator = new DataFluentValidator();
var results = validator.Validate(data);
```

[Fluent Validation also supports dependency injection](https://docs.fluentvalidation.net/en/latest/di.html), 
which can help with setting up validation in generic contexts.

## Returning validation results

The APIs we build, whether they use Data Annotation Validation or Fluent Validation, 
should return the validation errors back to the client, so the developers and applications
using our APIs can see what they do wrong and what to fix. Few things we should do:

- Respond with HTTP status code `400 Bad Request` or `422 Unprocessable Entity`.
- The response model must be an object containing the validation errors, and optionally
some more data.
- The response model should not contain any types from `System.ComponentModel.DataAnnotations`
or `FluentValidation` as those can change unexpectedly when updating package versions,
potentially breaking users of our API.
- The mentioned member / property names in the response should be cased identically
to what the request model expected.
- The mentioned member / property names should use JSON path when they describe
nested properties.

Example:

```json
{
    "errorCode": "E0067",
    "message": "Invalid request",
    "validationErrors" : [
        {
            "errorMessage": "'startDate' must be less than 'endMonth'.",
            "propertyName": "startDate"
        },
        {
            "errorMessage": "The length of 'name' must be at least 10 characters. You entered 4 characters.",
            "propertyName": "children[0].name"
        }
    ] 
}
```
