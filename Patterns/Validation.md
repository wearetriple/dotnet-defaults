# Validation

## Data Annotation Validators

Validating data using [Data Annotation Validators](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-5.0) is a simple but powerful way of validating incoming data or settings. It provides a convenient amount of validators that can be assigned to properties using attributes. 

```c#
public class Data 
{
    [MinLength(10)]
    public string? Name { get; set; }

    [Required]
    public IEnumerable<Data> Children { get; set; }

    [Required]
    public Data Parent { get; set; }

    [Required, JsonConverter(typeof(DateConverter))]
    public DateTime StartDate { get;. set; }
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

The `[ValidateEnumerable]` and `[ValidateObject]` attributes instruct the validator to go into the enumerable or object. Error messages from the deeper properties are grouped under the error message of the `Children` and `Parent` properties. The implementation of these attributes can be found in NETDefault/Libs/Triple.DataAnnotations.

### Validating dates

Validating dates in a concise way can be achieved using specific JsonConverter classes. In the above example you can find the property `StartDate` of type `DateTime`. The `JsonConverter` makes use of the class `DateConverter`:

```c#
public class DateConverter : IsoDateTimeConverter
{
        public DateConverter()
        {
            DateTimeFormat = "yyyy-MM-dd";
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
