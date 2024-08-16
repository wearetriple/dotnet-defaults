# `ILogger<T>` &amp; Seq logging

Seq is a log aggregation service collecting logs from backend services. It enables developers read and analyze logs from a web portal, instead of having to download log files. Inject `ILogger<{concrete type}> logger` (from `Microsoft.Extensions.Logging`) into a class to log to Seq via Serilog. Never log any (potential) PII or secrets. 

## Best practices (not exhaustive)

### Structured logging

To take full advantage of the search capabilities of Seq, the log messages should be created via structured logging pattern. Instead of:

```c#
_logger.LogInformation($"Invoice {invoice.Id} added.");
```

The messages should be created as:

```c#
_logger.LogInformation("Invoice {InvoiceId} added.", invoice.Id);
```

This makes correlating log messages more easy because next to the usual data, `InvoiceId` will be saved as a separate property for that log message. This allows developers to query for `InvoiceId == "abc123"` and find all the log messages related to that invoice.

### Object logging

In some cases it can be desirable to log a complete object. In this case serilog provides the *destructuring operator*: `@`

```c#
Log.Information("Processing invoice {@Invoice}", invoice);
```

This will cause the supplied object to be broken down into its properties with values, which are logged as JSON by default.

### Exception logging

When logging exceptions, always include the exception to the log message:

```c#
_logger.LogWarning(caughtException, "Saving invoice failed.");
```

This will save and include the stack trace of that `caughtException` in Seq, making debugging that unexpected `NullReferenceException` much easier because it's location can be better determined.

### Create log scopes to correlate logging

(Implementation at the end of this document)

Use log scopes to correlate messages:

```c#
var invoiceId = "INV123";
using (_logger.AddToScope(invoiceId).BeginScope("Invoice processing"))
{
    _invoiceService.DoStuff(invoiceId);
}
```

This will create a log scope that will, until it is disposed, add the `InvoiceId` property with value `"INV123"` to every log message that is created within the scope. The logs inside the Invoice Service will also have the `InvoiceId` property added. Next to that, the `BeginScope` method will log an `"Invoice processing started"`-message at the start of the scope and `"Invoice processing finished"` once it is disposed. 

### Correlate logging between requests

Correlating log messages using unique identifiers makes debugging interactions between services less challenging. Including the TraceIdentifier that exists on the HttpContext in a very global log scope makes correlating log messages for an entire request possible. Sending the TraceIdentifier in a header (like `X-Trace`) to other services which log that header makes it possible to correlate a request and it subsequent downstream requests.

### Return correlation ids in error messages

It is useful to return unique identifiers like the TraceIdentifier when encountering issues that result in things like 500 Internal Server Error responses. This will give clients of your API / application the possibility to flag issues and provide useful information which can be used to find relevant log messages.

### Include project name

When logging to Serilog, it is valuable to include the project name of the application in the configuration of Serilog. This will help in understanding where the log message came from, especially when it is a `Value cannot be null` coming out of nowhere. There are usually multiple projects that write to the same Api Key (API, CMS, Background service, etc), so it is not always clear where certain logging came from. To include the name of the project, use the `Enrich` fluent api of the `LoggerConfiguration`.

### Use logging instead of debugging when developing locally

It is very easy as .NET-developer to hit F5 (Start Debugging) and debug your application. The current tooling for .NET is of very high quality and it is easy to find issues in your code using debugging. But because of those debugging features it is even more easy to simply forget about adding log messages at the correct places.

Because of this, try to finishing the last piece of a feature by not using any debugging features, and see if you can follow and debug the application using only logging. To aid in this, it could be useful to run a local SEQ instance using docker:

```
docker run --name seq -d -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
```

This starts a SEQ instance under `http://localhost:5341` which you can use to post log messages to. Using a local instance makes it easy to follow your own log messages without having to ignore any other messages.

### Use useful log levels

- `Fatal`: Application crashes.
- `Error`: Unhandled exceptions; unexpected results.
- `Warnings`: Calls to deprecated functionality; handled exceptions; expected but uncommon results.
- `Info`: Application flow summaries; statistic information.
- `Debug`: Application flow messages, happy and unhappy.
- `Verbose`: Everything else which would otherwise flood Debug.

## Log scope builder

```c#
public static class ILoggerExtensions
{
    public static IDisposable BeginHttpContextScope(this ILogger logger, HttpContext httpContext)
    {
        var builder = logger.AddToScope(httpContext.TraceIdentifier);

        // Log the X-Trace request header from the request to trace remote requests.
        if (httpContext.Request.Headers.TryGetValue("X-Trace", out var headerValues) &&
            headerValues.ToString() is string remoteTraceIdentifier)
        {
            builder.AddToScope(remoteTraceIdentifier);
        }

        return builder.BeginScope("Request");
    }

    public static ILogScopeBuilder AddToScope<TValue>(this ILogger logger, TValue value, [CallerArgumentExpression("value")] string argumentExpression = "")
    {
        return new LogScopeBuilder(logger).AddToScope(value, argumentExpression);
    }
}

public interface ILogScopeBuilder
{
    ILogScopeBuilder AddToScope<TValue>(TValue value, [CallerArgumentExpression("value")] string argumentExpression = "");

    IDisposable BeginScope();

    IDisposable BeginScope(string message, params object?[] args);
}

internal class LogScope : IDisposable
{
    private readonly ILogger _logger;
    private readonly IDisposable _scope;
    private readonly string _message;
    private readonly object?[] _args;

    public LogScope(ILogger logger, Dictionary<string, object?> data, string message, params object?[] args)
    {
        _logger = logger;
        _message = message;
        _args = args;

        _scope = _logger.BeginScope(data);
        _logger.LogInformation($"{_message} started", _args);
    }

    public void Dispose()
    {
        _logger.LogInformation($"{_message} finished", _args);
        _scope.Dispose();
    }
}

internal class LogScopeBuilder : ILogScopeBuilder
{
    private readonly Dictionary<string, object?> _data = new();
    private readonly ILogger _logger;

    public LogScopeBuilder(ILogger logger)
    {
        _logger = logger;
    }

    public ILogScopeBuilder AddToScope<TValue>(TValue value, [CallerArgumentExpression("value")] string argumentExpression = "")
    {
        var property = argumentExpression.Split(".")[^1];

        _data[property] = value;

        return this;
    }

    public IDisposable BeginScope() => _logger.BeginScope(_data);

    public IDisposable BeginScope(string message, params object?[] args) => new LogScope(_logger, _data, message, args);
}

```