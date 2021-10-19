# `IOptions<T>`

The [options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-5.0) should be employed to configure services and expose environment variables. This pattern creates `IOptions<ConcreateClass>` in the DI container that contain the configuration. Using `Options.Create(new ConcreteClass { /* .. */ }` creates working `IOptions<ConcreteClass>` instances for use in unit tests.

## When to use what

- `IOptions<T>`: Available as singleton and does not change during lifetime of application.
- `IOptionsSnapshot<T>`: Available as scoped service and fetches the latest configuration every time it is created. Useful when using configuration that can update independently from the application (KeyVault for example).
- `IOptionsMonitor<T>`: Available as singleton and exposes an event that triggers once the configuration is updated. Can be useful when using configuration that can change frequently.

## Web Apps (.NET 5 & .NET Core 3.1)

- Startup.cs: Add `services.AddOptions<ConcreteClass>()` to add `IOptions<ConcreteClass>` to the DI container. This `ConcreteClass` should be a simple POCO. 
- Startup.cs: Chain `.Bind(Configuration.GetSection("sectionName"))` to bind a certain section from `appsettings.json` to this `ConcreteClass`.
- Startup.cs: Chain `.ValidateDataAnnotations().ValidateAtStartupTime()` to validate the configuration at startup time, preventing the application to run on bad configuration. Annotate properties in `ConcreteClass` with `[Required]` or other data annotation attributes to specify the validation.

## Function Apps (.NET 5 isolated)

- Program.cs: Add `services.AddOptions<ConcreteClass>()` to add `IOptions<ConcreteClass>` to the DI container. This `ConcreteClass` should be a simple POCO.
- Program.cs: Chain `.Bind(context.Configuration.GetSection("sectionName"))` to bind a certain section from `local.settings.json` to this `ConcreteClass`.
- Program.cs: Chain `.ValidateDataAnnotations()` to validate the configuration at startup time, preventing the application to run on bad configuration. Annotate properties in `ConcreteClass` with `[Required]` or other data annotation attributes to specify the validation. Validation at startup time is not supported in Function Apps.

## Function Apps (.NET Core 3.1)

- Program.cs: Add `services.AddOptions<ConcreteClass>()` to add `IOptions<ConcreteClass>` to the DI container. This `ConcreteClass` should be a simple POCO.
- Program.cs: Chain `.Bind(context.Configuration.GetSection("sectionName"))` to bind a certain section from `local.settings.json` to this `ConcreteClass`.
- Program.cs: Chain `.ValidateDataAnnotations()` to validate the configuration at startup time, preventing the application to run on bad configuration. Annotate properties in `ConcreteClass` with `[Required]` or other data annotation attributes to specify the validation. Validation at startup time is not supported in Function Apps.
