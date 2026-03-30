# `ILogger<T>` &amp; OpenTelemetry logging and tracing

As .NET developers we're very accustomed to logging application logs using `ILogger<T>`,
but we also want to use the logging and tracing features provided by OpenTelemetry.
If we want to upgrade the logging setup we normally configured using [ILogger<T> and Seq](./Serilog.md),
we need to configure a few things:

## `AddOpenTelemetry`

To add support for OTel in your application, make sure you first configure your logging 
setup by configuring the `ILoggerBuilder`:

```c#
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeScopes = true;
    options.IncludeFormattedMessage = true;

    options.SetResourceBuilder(applicationResourceBuilder);

    options.AddOtlpExporter(options =>
    {
        // example logs ingest url for local seq
        options.Endpoint = new Uri("http://127.0.0.1:5341/ingest/otlp/v1/logs");
        // local seq does not need an api key, but via Headers is how you normally provide it
        options.Headers = null;
        options.Protocol = OtlpExportProtocol.HttpProtobuf;
    });
});
```

Next to that we need to add some OTel services to the `IServiceCollection`:

```c#
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        // this needs to be configured, otherwise we might log too much
        .SetSampler(new AlwaysOnSampler())
        .SetResourceBuilder(applicationResourceBuilder)
        .AddSource(rootActivitySource.Name)
        .AddAspNetCoreInstrumentation()
        // this is required to forward the tracing information to the downstream services
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            // example traces ingest url for local seq
            options.Endpoint = new Uri("http://127.0.0.1:5341/ingest/otlp/v1/traces");
            // local seq does not need an api key, but via Headers is how you normally provide it
            options.Headers = null;
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
        }));
```

Both coding examples require an `applicationResourceBuilder`. This builder provides
some metadata about your application, which is added to all the logs and traces
emitted from your application. An example implementation is:

```c#
var application = Assembly.GetEntryAssembly();
var applicationName = application?.GetName();

var applicationResourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(
        // this uses the entry assembly's name, which is not always the best option. 
        // one can also use some environment variable instead (like `WEBSITE_SITE_NAME`)
        serviceName: applicationName?.Name ?? "no-name",
        serviceVersion: applicationName?.Version?.ToString() ?? "0.0.0");
```

The tracing configuration also needs the name of the `rootActivitySource`. Activity
sources are used to create activities, which correspond with spans in OpenTelemetry.

```c#
var rootActivitySource = new ActivitySource(applicationName?.Name ?? "no-name");

LoggerExtensions.RootActivitySource = rootActivitySource;
```

The `LoggerExtensions.RootActivitySource` (see source below) is used as fallback when
there is currently no active `Activity`, which happens when activities are started from
contexts without a base activity (like `BackgroundService`s or `IHostedService`s).

## Create spans via log scopes

Log scopes are useful by grouping log lines together and adding extra properties
to each log line within a scope. These log scopes translate to spans in OpenTelemetry,
and can easily be created by the code below. Spans make it easy to group certain 
log messages together, which really help if multiple actions in the same trace are 
done in parallel.

These extensions and builder allow developers to write the following code:

```c#
var invoiceId = "INV123";
using (_logger.AddToSpan(invoiceId).StartSpan("Invoice processing"))
{
    _logger.LogInformation("Logging from invoice processing");
}
```

This will create a sub span below the currently active span, and add the `InvoiceId`
tag to that span. It also opens a log scope that is active for the same duration as
the span, which adds the `InvoiceId` to all of the log lines inside the scope.

## Create details logs with log scopes

Not every log scope needs to become a span in OpeTelemetry. By using `AddToLog` and
`StartDetailedLogScope` it is easy to add data to a log scope (and all logs inside
that scope) without having to mention that data in the log message.

```c#
var invoice = // invoice structure that really useful for querying logs;
using (_logger.AddToLog(invoice).StartDetailedLogScope("Invoice processing"))
{
    _logger.LogInformation("Logging from invoice processing");
}
```

## Extensions and builder for creating spans

```c#
public static class LoggerExtensions
{
    // to be set by the shared app host builder during app construction
    // this must be internal when using in real project, so only the base setup has access and configures it correctly
    public static ActivitySource RootActivitySource = null!;

    extension(ILogger logger)
    {
        public IDisposable StartSpan([CallerMemberName] string spanName = "")
        {
            var source = Activity.Current?.Source ?? RootActivitySource;

            var spanBuilder = new SpanBuilder(logger, source);
            return spanBuilder.StartSpan(spanName);
        }

        public ISpanBuilder AddToSpan<T>(T value, [CallerArgumentExpression(nameof(value))] string valueName = "")
        {
            var source = Activity.Current?.Source ?? RootActivitySource;

            var spanBuilder = new SpanBuilder(logger, source);
            spanBuilder.AddToSpan(value, valueName);
            return spanBuilder;
        }
    }
}

public interface ISpanBuilder
{
    ISpanBuilder AddToSpan<T>(T value, [CallerArgumentExpression(nameof(value))] string valueName = "");

    IDisposable StartSpan([CallerMemberName] string spanName = "");
}

internal class SpanBuilder : ISpanBuilder
{
    private readonly ILogger _logger;
    private readonly ActivitySource _activitySource;

    private Dictionary<string, object?>? _state;

    public SpanBuilder(
        ILogger logger,
        ActivitySource activitySource)
    {
        _logger = logger;
        _activitySource = activitySource;
    }

    public ISpanBuilder AddToSpan<T>(T value, [CallerArgumentExpression(nameof(value))] string valueName = "")
    {
        if (!(valueName?.Length > 0))
        {
            return this;
        }
        else if (char.IsLower(valueName[0]))
        {
            valueName = $"{char.ToUpper(valueName[0])}{valueName[1..]}";
        }

        _state ??= new();
        _state[valueName] = value;
        return this;
    }

    public IDisposable StartSpan([CallerMemberName] string spanName = "")
    {
        var activity = _activitySource.StartActivity(
            ActivityKind.Internal,
            parentContext: Activity.Current?.Context ?? default,
            tags: _state,
            name: spanName);

        // if there is no state, there is no need to start a log scope
        if (_state == null)
        {
            return activity ?? Pfas.Default;
        }
        else
        {
            var scope = _logger.BeginScope(_state);
            return new DoubleDispose(activity, scope);
        }
    }

    private struct DoubleDispose(
        IDisposable? disposable1,
        IDisposable? disposable2) : IDisposable
    {
        public void Dispose()
        {
            try
            {
                disposable1?.Dispose();
                disposable1 = null;
            }
            catch
            {
                // we don't care about issues here
            }

            try
            {
                disposable2?.Dispose();
                disposable2 = null;
            }
            catch
            {
                // we don't care about issues here
            }
        }
    }
}

internal sealed class Pfas : IDisposable
{
    public static readonly IDisposable Default = new Pfas();

    public void Dispose()
    {
        // doesn't do anything
    }
}

```

## Extensions and builder for detailed log scopes

```c#
public static class LoggerExtensions
{
    extension(ILogger logger)
    {
        public ILogBuilder AddToLog(
            object? value,
            [CallerArgumentExpression(nameof(value))] string valueName = ""
        )
        {
            var logBuilder = new LogBuilder(logger);
            logBuilder.AddToLog(value, valueName);
            return logBuilder;
        }

        public ILogBuilder AddToLog<TValue>(
            IEnumerable<KeyValuePair<string, TValue>> value,
            [CallerArgumentExpression(nameof(value))] string valueName = ""
        )
        {
            var logBuilder = new LogBuilder(logger);
            logBuilder.AddToLog(value, valueName);
            return logBuilder;
        }

        public ILogBuilder AddJsonToLog(
            string value,
            [CallerArgumentExpression(nameof(value))] string valueName = ""
        )
        {
            var logBuilder = new LogBuilder(logger);
            logBuilder.AddJsonToLog(value, valueName);
            return logBuilder;
        }
    }
}

public interface ILogBuilder
{
    ILogBuilder AddToLog(
        object? value,
        [CallerArgumentExpression(nameof(value))] string valueName = ""
    );

    ILogBuilder AddToLog<TValue>(
        IEnumerable<KeyValuePair<string, TValue>> value,
        [CallerArgumentExpression(nameof(value))] string valueName = ""
    );

    ILogBuilder AddJsonToLog(
        string value,
        [CallerArgumentExpression(nameof(value))] string valueName = ""
    );

    IDisposable StartDetailedLogScope([CallerMemberName] string spanName = "");
}

internal class LogBuilder : ILogBuilder
{
    private static readonly JsonDocumentOptions DefaultJsonOptions = new JsonDocumentOptions
    {
        AllowTrailingCommas = true,
    };

    private readonly ILogger _logger;

    private readonly string? _property;
    private readonly Dictionary<string, object?> _state;

    public LogBuilder(ILogger logger)
    {
        _logger = logger;

        _property = null;
        _state = [];
    }

    internal LogBuilder(ILogger logger, string property, Dictionary<string, object?> state)
    {
        _logger = logger;

        _property = property;
        _state = state;
    }

    public ILogBuilder AddToLog(
        object? value,
        [CallerArgumentExpression(nameof(value))] string valueName = ""
    )
    {
        if (GetFullName(valueName) is not string fullName)
        {
            return this;
        }

        if (value is IDictionary dictionary)
        {
            var nestedLogBuilder = new LogBuilder(_logger, fullName, _state);
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key is string key)
                {
                    nestedLogBuilder.AddToLog(entry.Value, key);
                }
            }

            return this;
        }
        else if (value is StringValues stringValues)
        {
            if (stringValues.Count == 1)
            {
                _state.Add(fullName, stringValues[0]);
            }
            else
            {
                _state.Add(fullName, stringValues);
            }

            return this;
        }
        else if (value is string[] stringArray)
        {
            if (stringArray.Length == 1)
            {
                _state.Add(fullName, stringArray[0]);
            }
            else
            {
                _state.Add(fullName, stringArray);
            }

            return this;
        }
        else
        {
            _state.Add(fullName, value);
            return this;
        }
    }

    public ILogBuilder AddToLog<TValue>(
        IEnumerable<KeyValuePair<string, TValue>> value,
        [CallerArgumentExpression(nameof(value))] string valueName = ""
    )
    {
        if (GetFullName(valueName) is not string fullName)
        {
            return this;
        }

        var nestedLogBuilder = new LogBuilder(_logger, fullName, _state);
        foreach (var entry in value)
        {
            nestedLogBuilder.AddToLog(entry.Value, entry.Key);
        }

        return this;
    }

    public ILogBuilder AddJsonToLog(
        string value,
        [CallerArgumentExpression(nameof(value))] string valueName = ""
    )
    {
        if (GetFullName(valueName) is not string fullName)
        {
            return this;
        }

        try
        {
            var element = JsonElement.Parse(value, DefaultJsonOptions);
            var dictionary = element.Flatten(fullName);

            foreach (var entry in dictionary)
            {
                _state.Add(entry.Key, entry.Value);
            }

            return this;
        }
        catch
        {
            _state.Add(fullName, value);
            return this;
        }
    }

    public IDisposable StartDetailedLogScope([CallerMemberName] string spanName = "")
    {
        return _logger.BeginScope(_state) ?? Pfas.Default;
    }

    private string? GetFullName(string valueName)
    {
        if (PropertyNameHelper.Sanitize(valueName) is not string name)
        {
            return null;
        }

        return _property == null ? name : $"{_property}.{name}";
    }
}

internal static class Flattener
{
    extension(JsonElement element)
    {
        /// <summary>
        /// Flattens a JSON structure into a single-level dictionary.
        /// </summary>
        /// <param name="element">The <see cref="JsonElement"/> which should be flattened.</param>
        /// <returns>A dictionary mapping each JSON path to its corresponding value.</returns>
        public IDictionary<string, string> Flatten(string rootName)
        {
            var result = new Dictionary<string, string>();
            Visit(element, rootName, result);
            return result;
        }
    }

    private static void Visit(JsonElement element, string path, IDictionary<string, string> result)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                VisitArray(element, path, result);
                break;
            case JsonValueKind.Object:
                VisitObject(element, path, result);
                break;
            default:
                result[path] = element.ToString();
                break;
        }
    }

    private static void VisitArray(
        JsonElement element,
        string path,
        IDictionary<string, string> result
    )
    {
        var i = 0;
        foreach (var item in element.EnumerateArray())
        {
            var itemPath = $"{path}[{i}]";
            Visit(item, itemPath, result);
            i++;
        }
    }

    private static void VisitObject(
        JsonElement element,
        string path,
        IDictionary<string, string> result
    )
    {
        foreach (var property in element.EnumerateObject())
        {
            var propertyPath = $"{path}.{PropertyNameHelper.Sanitize(property.Name)}";
            Visit(property.Value, propertyPath, result);
        }
    }
}

internal static class PropertyNameHelper
{
    [return: NotNullIfNotNull(nameof(input))]
    public static string? Sanitize(string? input)
    {
        if (input == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        if (char.IsLower(input[0]))
        {
            input = $"{char.ToUpper(input[0])}{input[1..]}";
        }

        input = input.Replace("-", "_");

        return input;
    }
}

```
