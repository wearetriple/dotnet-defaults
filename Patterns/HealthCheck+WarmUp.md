# Health check and application warm-up

Within an Azure Web App there are two features which can help achieve better and more reliable performance. The health check allows the load-balancer to monitor the health of App Service instances and remove unhealthy instances from the pool. The application warm-up feature forces the load-balancer to wait before adding a new instance until the newly created instance has reported it is ready and warm. This warm-up is also triggered during deployment slot swaps, which greatly improves the responsiveness of the application after swap.

When the instance is the only instance in the load balancing pool Azure ignores the status of the warm-up, although still triggering it, and will directly expose the cold instance to the internet. Similarly, if all instances within the pool are unhealthy the health status is ignored.

The health check and application warm-up features are only available for Azure Web Apps so only the implementation for ASP.NET applications is described.

## ASP.NET

### Warm-up

The Application Initialization feature in IIS can be used to warm-up an instance before it is added to the load balancing pool. This feature *only* support HTTP and no HTTPS. By adding the following `<applicationInitialization>`-node to web.config the feature can be enabled. Each of the `initializationPage` endpoints is touched with an HTTP request upon start up and initialization is done when all endpoints have returned *a* result. This result can be anything, a 307 Temporarily Moved from the `UseHttpsRedirection()` middleware, a 401 Unauthorized because the request is done anonymously, and even a 500 Internal Server Error is accepted as valid result. The endpoint should only give a response when it has finished initializing.

```xml
<configration>
  <system.webServer>
    <applicationInitialization>
      <add initializationPage="/{url-to-check}" />
    </applicationInitialization>
  </system.webServer>
</configuration>
```

To prevent Https redirection for the initialization page, modify `app.UseHttpsRedirection()` to allow for excluding specific paths from Https redirection:

```c#
var httpPaths = Configuration.GetSection("HttpPaths").Get<HttpPathSettings>();

app.UseWhen(
    httpContext => !httpPaths.Paths.Any(path => httpContext.Request.Path.StartsWithSegments(path)), 
    app => app.UseHttpsRedirection());
```

Similar exceptions must be made when the application has been set up with authentication and authorization. The initialization pages should be accessable anonymously. It is advisable to make those endpoints hard to guess by incorporating a GUID or add some query parameters. Do not expose private data via initialization pages.

See the WebApp project for more implementation details.

### Swap warm-up

When using deployment slots, the Application Initialization feature is useful to instruct Azure to wait with the DNS change after the swapped slot has warmed up. This prevents a cold instance from being exposed to the internet directly after swap. When performing a swap, the app settings of the target slot are applied to the source slot. Since this restarts the application in the source slot, Azure will wait before the source slot has come back online and report its readiness via Application Initialization. When it does, the DNS settings are changed and the source application is moved in the target slot. The target application is moved to the source slot and is restarted with the staging slot settings. If the preview option was enabled, the old production application will remain online for a quick revert if needed.

No additional configuration is needed to enable swap warm-up, other than defining [App Service Deployment Slots](https://docs.microsoft.com/en-us/azure/app-service/deploy-staging-slots). 

NOTE: The documentation mentions [customizing warm-up](https://docs.microsoft.com/en-us/azure/app-service/deploy-staging-slots#specify-custom-warm-up) by setting environment variables. When testing, it was found that setting `WEBSITE_WARMUP_PATH` crashes the App Service and leaves it unresponsive. Use with caution.

### Health check

In Azure App Service, the [health check](https://docs.microsoft.com/en-us/azure/app-service/monitor-instances-health-check) is an endpoint which is polled every 2 to 10 minutes and an instance is considered healthy if the endpoint returns an OK-ish (200 - 299) response. This feature supports HTTPS, and when the App Service is configured to be HTTPS only, the health check is performed using HTTPS.

In ASP.NET Core, the [health check](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks) is a feature in which health checks can be added to an ASP.NET application which report the health status of the entire application. 

These two health check features can be combined to make for easy health monitoring; by the load-balancer and possible by an external monitoring tool. 

To implement health checks:

- Add a class which implements `IHealthCheck`. Have the implementation return a correct `HealthStatus`.
- Startup.cs: Add each health check to the list of health checks by adding to `ConfigureServices`: `services.AddHealthChecks().AddCheck<{Class}>("{description}");`.
- Startup.cs: Map an endpoint to the health check by adding to `UseEndpoints` in  `Configure`: `endpoints.MapHealthChecks("/{endpoint}]");`.
- If needed, [extra authorization can be added](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks#use-health-checks-routing) to the health check endpoint to prevent anonymous access to this information.

If more details are required in the health check response, a simple `ResponseWriter` can be added to provide those details:

```c#
endpoints.MapHealthChecks("/{endpoint}}", new HealthCheckOptions
{
  ResponseWriter = async (context, report) =>
  {
    using var streamWriter = new StreamWriter(context.Response.Body);

    foreach (var entry in report.Entries)
    {
      await streamWriter.WriteLineAsync($"{entry.Key}: {entry.Value.Status}.");
    }

    await streamWriter.FlushAsync();
  }
}
```

If that is not enough, use the [HealthCheck UI package](https://github.com/xabaril/AspNetCore.Diagnostics.HealthChecks).
