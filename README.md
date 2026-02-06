# dotnet-defaults

Common boilerplate, practices, and guidelines for Triple .NET projects.

## Required standardizations

Our [Standardization](./Standards/README.md) overview

- [Configuration](./Standards/Configuration/README.md)
- [Renovate](./Standards/Renovate/README.md)

## Required patterns

Our [Patterns](./Patterns/README.md) overview

- [Dependency Injection](./Patterns/DependencyInjection.md)
- [Layered project](./Patterns/LayeredProject.md)
- [Nullable reference types and `required`](./Patterns/Nullables.md)
- [Options pattern](./Patterns/Options.md)
- [Configuration](./Patterns/Configuration.md)
- [KeyVault](./Patterns/KeyVault.md)
- [Logging with `ILogger<>` using OpenTelemetry to any OTLP sink (or Seq)](./Patterns/Otel.md)
- [Logging with `ILogger<>` using Serilog to Seq](./Patterns/Serilog.md)
- [Health check and application warmup](./Patterns/HealthCheck+WarmUp.md)

## Good practices

- [Request cancellation](./Patterns/CancelRequest.md)
- [Hosted service](./Patterns/HostedService.md)
- [Data Annotation Validation](./Patterns/Validation.md)
- [Polly: The .NET resilience library](./Patterns/Polly.md)
