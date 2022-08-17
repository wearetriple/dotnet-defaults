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
- Register a service using its interface.

### Service factories

Some services cannot be constructed easily. This could be because it requires some dynamic data or requires data that cannot be registered in the DI container (like a `string`). The DI container can help with building these services as it allows for running a factory function every time the service is requested from the DI container. Since the depending service can just request `IHardToConstructService` and not care how the instance was constructed, this solution is invisible for the users of `IHardToConstructService`, so there is no need for a `IHardToConstructServiceFactory` which leaks the fact that `IHardToConstructService` is hard to construct.

Example:

```c#
// in startup.cs
services.AddTransient<IHardToConstructService>((serviceProvider) => 
{
    // get a dynamic variable before creating an instance of HardToBuild
    var seed = new Random().NextDouble();

    // create a new instance of HardToBuild with that dynamic variable
    return new HardToConstructService(seed);
});
```

The service factory can also be used to construct wrappers around global static object, like the Firebase SDK. By configuring the `IWrapperAroundGlobalStatic` as `Singleton` this will be the only instance within the application and the service factory will only be executed once. Users of `IWrapperAroundGlobalStatic` will just depend on the interface, and unit tests can simply mock `IWrapperAroundGlobalStatic` instead of having to override the global static object.

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

### Registering `internal` services

When building class libraries it is a good practice to make library specific details invisible by marking some classes as `internal`. A service in `Contoso.Services` just needs `IRepository<SomeEntity>` from `Contoso.Repositories` and should not be able to see or touch `SomeEntityRepositoryImplementation`. But when registering that repository in the DI container `Contoso.API` a developer can no longer do `services.AddScoped<IRepository<SomeEntity>, SomeEntityRepositoryImplementation>()` as `Contoso.API` cannot access the internals of `Contoso.Repositories`. 
To solve this, every class library should provide extension method on IServiceCollection like this:

```c#
namespace Contoso.Repositories
{
    public static class ContosoRepositoriesServiceCollectionExtensions 
    {
        public static void AddContosoRepositories(this IServiceCollection services) 
        {
            services.AddScoped<IRepository<SomeEntity>, SomeEntityRepositoryImplementation>();
        }
    }
}
```

Because this class is in the `Repositories` project, it can reference the internal `SomeEntityRepositoryImplementation`. `Contoso.API` can simply call `services.AddContosoRepositories()` and have all the appropriate services registered. Because calls to the dependency container are idempotent, you can safely have your library depend on the dependency registration of others by calling their registration in yours.  
E.g., a `Contoso.Services` project is allowed to call `.AddContosoRepositories`.

Keep in mind that this approach should be all or nothing, you either take full control of dependency registration in your library, or leave it entirely up to users of your library what services to inject. 
If you run into problems with your library injecting a lot of unused services, you should either split the project into multiple projects as it has become too broad and contains a diffuse set of classes or split the extension method into multiple methods and call one or multiple of them in projects that use the library.

#### Configuration with options

It is good practice to combine this with the [Options pattern](Options.md) by having the extension methods accept a IConfiguration instance to allow the library to setup its own configuration:

```c#
namespace Contoso.Gateway
{
    public static class ContosoGatewayServiceCollectionExtensions
    {
        public static void AddContosoRepositories(this IServiceCollection services, IConfiguration config) 
        {
            services.AddScoped<ISomeGateway, SomeGatewayImpelmentation>();
			services.AddOptions<SomeGatewayConfiguration>()
				.Bind(configuration.GetSection(SomeGatewayConfiguration.SectionName));
        }
    }
}
```

