# Hosted Service

When a simple background service is required which can access the same objects in memory of an application, [Hosted Services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-5.0&tabs=visual-studio) are a good option. Pro-actively refreshing cache in a `IMemoryCache` is a very good example of when to use Hosted Services.

When not to use Hosted Services:

* The background service does something important: Hosted services are not reliable since they run in a Web App on the background. If an instance is taken offline by the Web App load balancer, the hosted service could be terminated mid-process.
* The background service does not require access to the same objects: It's better to employ a Function App which shares the same App Service Plan than to use a Hosted Service when the background service. Using a separate App Service Plan further isolates the Function App, which can improve its reliability.

## Web Apps 

To implement Hosted Service in a Web App:

- Create a class deriving from `BackgroundService` and implement `ExcecuteAsync(CancellationToken stoppingToken)` with async `while`-loop which only stops when `stoppingToken.IsCancellationRequested`.
- Startup.cs: Add to `ConfigureServices`: `services.AddHostedService<{class}>();`.

## Console Apps 

- Create a class deriving from `BackgroundService` and implement `ExcecuteAsync(CancellationToken stoppingToken)` with async `while`-loop which only stops when `stoppingToken.IsCancellationRequested`.
- Program.cs: Add to `ConfigureServices` of the `new HostBuilder()`: `services.AddHostedService<{class}>();`.
