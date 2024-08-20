# Nullable reference types

[Nullable reference types](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8#nullable-reference-types) were introduced in [C# 8](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8) in an effort to reduce the number of `NullReferenceException`s thrown by applications. The feature helps the developer spot possible `null` values and deal with them during development instead of during testing / production.

The .net team has a general consensus that this feature improves code quality and reduces faults, so turn it on if possible. (With nullable warnings as errors.)

When a reference type property or variable is not-nullable, you as developer promise that:

- The value is *never* `null`.
- The value is *always* safe to use without checking for `null` *ever*.
- The heat death of the universe occurs earlier than that the value is `null`.

If you cannot promise that: mark it as nullable.

## To enable the feature

- Under the `<TargetFramework>` node in `.csproj` add: `<Nullable>enable</Nullable>`, or use our default [Directory.Build.Props](DirectoryBuildProps.md).
- Fix all possible null reference warnings.

## Common strategies

- Test for null using `if (value is ExpectedType expectedTypeValue)` or `if (value is not null)`.
- Add safe navigation operator: Instead of `.FirstOrDefault().Name` use `.FirstOrDefault()?.Name`.
- Correctly annotate properties: Instead of `public string OptionalName { get; set;}` use `public string? OptionalName { get; set; }`.
- Make sure not-null properties are never null:
    - Convert model to `record` using `public record Person(string Name);`.
    - Initialize property via constructor: `public Person(string name) { Name = name }`.
    - Pinky-promise that a property is never null and is always initialized using `public string Name { get; set; } = default!`.
    - Mark the property as nullable when there is an off-change that it could be `null`.

## Common causes of null

- The value originates from an external location (API / CMS). External data is always evil and, for example, even if a property is required in the CMS it's value can still be `null`.
- The value originates from a deserializer.
- The value originates from an external package.
- The value originates from code that was written by another developer.

## False-positives

The compiler gets confused sometimes and will warn for null when it should not. See the following example:

```c#
var items = new string?[] { null, "", "A", null, "C" };

var notNullItem = items.Where(item => !string.IsNullOrEmpty(item)).First();

var notNullItemLength = notNullItem.Length; // this line gets a warning.
```

In this case `notNullItem` is not null, but the compiler still complains about a possible null reference. There are a few ways to fix this, depending on the situation.

- Safest: Check for null again.
- Safest: Change the logic to prevent `null`s from getting through, by using pattern matching for example: `var notNullItem = items.OfType<string>().Where(item => !string.IsNullOrEmpty(item)).First()`.
- Unsafe: Use the [fuckit operator](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-forgiving): `notNullItem!.Length`.
- Unsafe: Use the [warning pragma](https://docs.microsoft.com/en-us/cpp/preprocessor/warning?view=msvc-160) to locally disable compiler warnings.

## Takeaways

- Don't be that developer that gets caught with their pants down because an uncaught `NullReferenceException` gets thrown from a piece of code that contains the `!`-operator.
- Check your pull request *not* to contain any possible null referencing warnings!
