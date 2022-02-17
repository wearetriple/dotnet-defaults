# Request Handling

## Background information

Our `ASP.NET Core 3.1` applications are hosted on Windows by the Web Server called Internet Information Services [IIS](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-6.0). This means that `ASP.NET Core` acts as a module that plugs into the IIS pipeline. Running `ASP.NET Core` apps with IIS is done by hosting the application within an IIS Worker process (w3wp.exe), which is the so-called [in-process hosting model](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/in-process-hosting?view=aspnetcore-6.0).

Since `ASP.NET Core` 3.0 this in-process hosting has been enabled by default for all apps deployed to IIS. Setting the mode can be done explicitly in the project file:

``` XML
<PropertyGroup>
  <AspNetCoreHostingModel>OutOfProcess/InProcess</AspNetCoreHostingModel>
</PropertyGroup>
```

If your application is using the default in-process hosting, then using the static [RequestHandlerNETCore31](../RequestHandlerNETCore31/RequestHandler.cs) for `ASP.NET Core 3.1` is your pick.

The in-process hosting model offers better performance over out-of-process, but on the other hand offers less control. Out-of-process hosting has the benefit of employing [middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0), which offers more fine-grained modular control over requests and responses. 

Important difference:

- In in-process hosting, IIS hands over requests as an 'HttpContext' instance to your application.

- In out-of-proces hosting, IIS only forwards the request to [Kestrel](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-6.0), after which the request is forwarded into the `ASP.NET Core` middleware pipeline. The middleware pipeline ultimately hands off the 'HTTPContext' instance to your application.

## How to use

### .NET Core 3.1

- Create your own _RequestHandler_ class, preferrably in a _Common_ place.
- In the endpoint that receives the HTTP Request you can use it directly, for it is a static class.

## .NET 6.0

- It is recommended to use the middleware pipeline from .NET 6 on, since it offers better control over requests and responses.
