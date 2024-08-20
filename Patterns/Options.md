# `IOptions<T>`

The [options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options) should be employed to configure services and expose environment variables. This pattern creates `IOptions<ConcreteClass>` in the DI container that contain the configuration. Using `Options.Create(new ConcreteClass { /* .. */ }` creates working `IOptions<ConcreteClass>` instances for use in unit tests.

## When to use what

- `IOptions<T>`: Available as singleton and does not change during lifetime of application.
- `IOptionsSnapshot<T>`: Available as scoped service and fetches the latest configuration every time it is created. Useful when using configuration that can update independently from the application (KeyVault or App Configuration for example).
- `IOptionsMonitor<T>`: Available as singleton and exposes an event that triggers once the configuration is updated. Can be useful when using configuration that can change frequently.
- `IOptionsFactory<T>`: Available as singleton which exposes a `Create()` method which creates a new `ConcreteClass` instance when invoked. This factory can be used when the configured keys are rotated on a periodic basis. Once the service detects that its configuration has expired (APIs returning 401s for example), the service can recreate its configuration by invoking `Create()` on the factory.
