# Validation

## Data Annotation Validators

Validating data using Data Annotation Validators is a simple but powerful way of validating incoming data or settings. It provides a convenient amount of validators that can be assigned to properties using attributes. 

```c#
public class Data 
{
    [MinLength(10)]
    public string? Name { get; set; }
}
```

