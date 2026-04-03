# `IOptions<T>`

The [options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options) 
must be employed to configure services and expose environment variables. This pattern creates 
`IOptions*<ConcreteClass>` in the DI container that contain the configuration. 

## When to use what

In order to support dynamic reloading of options (e.g. via KeyVault or App Configuration),
we should always use `IOptionsMonitor<T>` and ban the other options. The `CurrentValue`
from `IOptionsMonitor<T>` always contains up-to-date configuration, and is cached when
the configuration is not updated.

`IOptionsMonitor` also allows adding an event handler via `OnChange`, which makes it
possible to react to configuration changes. This can come in handy to replace certain
shared instances (like `BlobServiceClient`) when the connection details change. However,
since the connection details to databases and storages are usually stored in environment
variables, changes in those trigger restarts, which causes the connection details to be
updated anyway.

The use of `IOptions`, `IOptionSnapshot` and `IOptionsFactory` should be [banned](../Standards/Configuration/README.md)
in our projects.

Use the `FakeOptionsMonitor.Create<T>()` snippet from below to create `IOptionsMonitor<T>`s
for use in tests.

## What to use as `T`

The concrete class used as `T` should adhere to the following:

- `T` should be a `record`, so config can be compared at runtime.
- `T` should use validation attributes, and should validate it's config after construction (see
[Options validation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-10.0#options-validation)).
- `T` can define defaults, so not all base configuration have to be defined in `appsettings.json`.

## Example extension method

```csharp
public record ExampleConfig : IConfigSection 
{
    public static string SectionName => "Example";

    [Required, MinLength(10)]
    public string DefaultSetting { get; set; } = "Fallback value";
}

public interface IConfigSection
{
    abstract static string SectionName { get; }
}

public static class ServiceCollectionExtensions 
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddConfiguration<TConfiguration>(IConfiguration config)
            where TConfiguration : class, IEquatable<TConfiguration>, IConfigSection
        {
            var section = config.GetSection(TConfiguration.SectionName);
            services.AddOptions<TConfiguration>()
                .Bind(configSection)

                // Will throw an exception when IOptions<TConfiguration> is injected if validation fails 
                .ValidateDataAnnotations()

                // Optional: will throw an exception during startup when validation fails - will prevent application from starting up..
                .ValidateOnStartup();
            
            return services;
        }
    }
}

// Use in `ConfigureServices`:
services.AddConfiguration<ExampleConfig>(context.Configuration);
```

## FakeOptionsMonitor

```csharp
public static class FakeOptionsMonitor
{
    public static FakeOptionsMonitor<T> Create<T>(T value)
        where T : class => new(value);
}

public class FakeOptionsMonitor<T> : IOptionsMonitor<T>
    where T : class
{
    private Action<T, string?>? _listeners;

    internal FakeOptionsMonitor(T currentValue)
    {
        CurrentValue = currentValue;
    }

    public T CurrentValue { get; private set; }

    public T Get(string? name) => CurrentValue;

    public IDisposable? OnChange(Action<T, string?> listener)
    {
        _listeners += listener;

        return new Unsubscriber(() =>
        {
            _listeners -= listener;
        });
    }

    private class Unsubscriber(Action action) : IDisposable
    {
        public void Dispose()
        {
            action.Invoke();
        }
    }

    public void SimulateUpdate(T value)
    {
        CurrentValue = value;
        _listeners?.Invoke(value, null);
    }
}
```
