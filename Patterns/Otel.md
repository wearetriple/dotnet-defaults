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

## Extensions and builder 

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
            return activity ?? Undisposable.Default;
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

    private sealed class Undisposable : IDisposable
    {
        public static readonly IDisposable Default = new Undisposable();

        public void Dispose()
        {
            // don't do anything
        }
    }
}

```
