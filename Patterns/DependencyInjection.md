# Dependency Injection

Dependency Injection (DI) forms the basis of most applications built within Triple. 
It allows classes to specify what other objects they need (their dependencies) in their constructor signature, which will then be provided by the DI container.
This moves the burden of knowing how to compose them out of the class itself. 

## Fundamentals

- [DI in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0)
- [DI in Azure Functions .NET Core 3.1](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection)
- [DI in Azure Functions .NET >= 5](https://docs.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide)

## Best practices concerning DI

### Always depend on interfaces

Try to request interfaces from the container instead of concrete classes. This makes the coupling between two object more loose, and allows for completely swapping the implementation of an interface without affecting any other class. This also makes mocking in unit tests quite easy, as Moq can mock an interface quite easily.

Example:

```c#
public class Service : IService 
{
    public Service(IRepository repository) 
    {

    }
}
```

The `Service` depends on the interface `IRepository` and not the class `Repository`, making it easy to swap it from `SqlRepository` to `InMemoryRepository` or even to `Mock<IRepository>`. The service itself is available in the container under `IService`, so objects like Controllers or other services that depend on it do not depend on the concrete `Service` but the abstract `IService`.

### Choose the shortest lifetime

When configuring the DI container, one can choose three different lifetimes of a service: Transient, Scoped and Singleton. As a best practice, try to pick the shortest lifetime in which the functionality of the service is not affected.

- Transient: This is for services that do not have state and depend on other objects for getting or setting data.

- Scoped: This is for services that do have some state, and that state is scoped to the request (in the case of ASP.NET Core). `DbContext`s are usually scoped. 

- Singleton: This is for services that have some inner state that is not specific to a user, or are costly to construct and don't really change much during the lifetime of the application.

#### Important

- Don't inject everything as a Singleton because that will save allocations. .NET applications have a garbage collector that allocates quickly and web applications often do have hefty garbage collection runs. It's way more painful to downgrade a Singleton service to a Scoped service and find all the nasty bugs that causes than to just create a new object per request or per use.
- Don't register a service as Scoped because it depends on a service that is Scoped. If it does not have internal state, register it as Transient.
- Do register a service using its interface.

### Service factories

When a service cannot be easily be constructed, because it depends on something that is only available at startup or it depends on a global static object, use the factory options that the container provides. This avoids having to create `IInterfaceXFactory` that generates a `IInterfaceX`, but still gives the option of executing some logic when the implementation of `IInterfaceX` is constructed.

Example:

```c#
// in startup.cs
services.AddSingleton<IWrapperAroundGlobalStatic>((serviceProvider) => 
{
    var options = serviceProvider.GetRequiredService<IOptions<SomeConfig>>();

    AnnoyingDependency.GlobalStatic.CreateSingleton(options.ConnectionString);

    // all dependent services will use `IWrapperAroundGlobalStatic` and not even know its globally available
    return new WrapperAroundGlobalStatic(AnnoyingDependency.GlobalStatic.Instance);
});
```

```c#
// in startup.cs
services.AddTransient<IHardToBuild>((serviceProvider) => 
{
    var random = new Random();
    return new HardToBuild(random.NextDouble());
});
```

### Inject `IEnumerable<IInterface>` to maximize extensibility

When requesting `IEnumerable<IInterface>` from the DI container, the DI container will resolve a list of all objects that have been registered under the `IInterface` type. This simplifies injecting multiple services that all do the same.

Example:

```c#
public class ApplicationTestController
{
    private readonly IReadOnlyDictionary<string, ISensor> _sensors;

    public ApplicationTestController(IEnumerable<ISensor> sensors) 
    {
        _sensors = sensors.ToDictionary(x => x.Name);
    }

    public IActionResult GetStatusOfSensor(string id)
        => _sensors.TryGetValue(id, out var sensor) && sensor.IsHealthy() 
            ? new OkResult() 
            : new InternalServerError();

    public IActionResult GetStatusOfSensors()
        => _sensors.All(x => x.IsHealthy())
            ? new OkResult() 
            : new InternalServerError();
}
```

### Register generic interfaces instead of every possible option

When an application uses generic interfaces, like `IRepository<TEntity>`, and there are quite some entities, registering every option becomes quite tedious:

```c#
// in startup.cs
services.AddTransient<IRepository<Blog>, Repository<Blog>>();
services.AddTransient<IRepository<Category>, Repository<Category>>();
services.AddTransient<IRepository<Author>, Repository<Author>>();
// etc
```

If every implementation is the same, use the following instead:

```c#
// in startup.cs
services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
```

