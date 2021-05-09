# `ILogger<T>` &amp; Seq logging

Seq is a log aggregation service collecting logs from backend services. It enables developers read and analyze logs from a web portal, instead of having to download log files. Inject `ILogger<{concrete type}> logger` into a class to log to Seq via Serilog. Never log any (potential) PII or secrets. 

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
