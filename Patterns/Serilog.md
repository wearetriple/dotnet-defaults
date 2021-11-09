# `ILogger<T>` &amp; Seq logging

Seq is a log aggregation service collecting logs from backend services. It enables developers read and analyze logs from a web portal, instead of having to download log files. Inject `ILogger<{concrete type}> logger` into a class to log to Seq via Serilog. Never log any (potential) PII or secrets. 

## Best practices (not exhaustive)

### Structured logging

To take full advantage of the search capabilities of Seq, the log messages should be created via structured logging pattern. Instead of:

```c#
_logger.LogInformation($"Invoice {invoice.Id} added.");
```

The messages should be created as:

```c#
_logger.LogInformation("Invoice {invoiceId} added.", invoice.Id);
```

This makes correlating log messages more easy because next to the usual data, `invoiceId` will be saved as a seperate property for that log message. This allows developers to query for `invoiceId == "abc123"` and find all the log messages related to that invoice.

### Exception logging

When logging exceptions, always include the exception to the log message:

```c#
_logger.LogWarning(caughtException, "Saving invoice failed.");
```

This will save and include the stack trace of that `caughtException` in Seq, making debugging that unexpected `NullReferenceException` much easier because it's location can be better determined.

### Include project name

When logging to Serilog, it is valuable to include the project name of the application in the configuration of Serilog. This will help in understanding where the log message came from, especially when it is a `Value cannot be null` coming out of nowhere. There are usually multiple projects that write to the same Api Key (API, CMS, Background service, etc), so it is not always clear where certain logging came from. To include the name of the project, use the `Enrich` fluent api of the `LoggerConfiguration`.

### Use useful log levels

- `Fatal`: Application crashes.
- `Error`: Unhandled exceptions; unexpected results.
- `Warnings`: Calls to depricated functionality; handled exceptions; expected but uncommon results.
- `Info`: Application flow summaries; statistic information.
- `Debug`: Application flow messages, happy and unhappy.
- `Verbose`: Everything else which would otherwise flood Debug.

## Web Apps (.NET 5 & .NET Core 3.1)

- Program.cs: Wrap `CreateHostBuilder(args).Build().Run()` in a try-catch. This enables Serilog to log fatal exceptions.
- Program.cs: Use `ConfigureLogging` on `IHostBuilder` to configure Serilog.
- appsettings.json: Configure `Serilog` section to add Seq as sink, and use `$controlSwitch` to configure minimum log level using Seq Api Key Configuration. Configure the default level to `Warning` to prevent noisy application starts.

## Function Apps (.NET 5 isolated)

- Program.cs: Wrap `CreateHostBuilder().Build().Run()` in a try-catch. This enables Serilog to log fatal exceptions.
- Program.cs: Use `ConfigureLogging` on `HostBuilder` to configure Serilog.
- local.settings.json: Configure `Serilog` parameters in `Values` section to add Seq as sink, and use `$controlSwitch` to configure minimum log level using Seq Api Key Configuration.  Configure the default level to `Warning` to prevent noisy application starts.

## Function Apps (.NET Core 3.1)

- Startup.cs: Read configuration and configure `Log.Logger`. Use `AddSerilog` to add Serilog to logging pipeline. Add a `LoggingLevelSwitch` to allow for switching log levels from Seq Api Key Configuration. Configure the default level to `Warning` to prevent noisy application starts.
- local.settings.json: Set `Serilog` parameters in `Values` section to configure the Url and Key of the Seq server.
