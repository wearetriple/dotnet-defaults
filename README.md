# dotnet-defaults

Common boilerplate, practices, and guidelines for Triple .NET projects.

## Required standardizations

Our [Standardization](./Standards/README.md) overview

- [Configuration](./Standards/Configuration/README.md)
- [CSharpier](./Standards/CSharpier/README.md)
- [Renovate](./Standards/Renovate/README.md)
- [Project Readme](./Standards/Readme/README.md)
- [Azure DevOps pipelines via Terraform](./Standards/AzureDevOpsTerraform/README.pipelines.md)
- [Azure DevOps service connections via Terraform](./Standards/AzureDevOpsTerraform/README.service-connections.md)

## Required patterns

### Base setup

- [Nullable reference types and `required`](./Patterns/Nullables.md)
- [Validation](./Patterns/Validation.md)
- [OpenApi Documentation](./Patterns/OpenApi.md)
- [Dependency Injection](./Patterns/DependencyInjection.md)
- [Hosted service](./Patterns/HostedService.md)

### Configuration

- [Configuration](./Patterns/Configuration.md)
- [Options pattern](./Patterns/Options.md)
- [KeyVault](./Patterns/KeyVault.md)

### Resilience

- [Resilience](./Patterns/Resilience.md)
- [Request cancellation](./Patterns/CancelRequest.md)

### Observability

- [Logging with `ILogger<>` using OpenTelemetry to any OTLP sink (or Seq)](./Patterns/Otel.md)
- [Probes](./Patterns/Probes.md)

### Project structure

- [Layered project](./Patterns/LayeredProject.md)

### Older standards

- [Logging with `ILogger<>` using Serilog to Seq](./Patterns/Serilog.md)
- [Web App health check and application warmup](./Patterns/HealthCheck+WarmUp.md)



## Good practices

