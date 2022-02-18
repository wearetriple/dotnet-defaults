# Request Handling

In-process Azure Functions up to .NET 8.0 can make use of the [RequestHandler](../RequestHandlerNETCore31/RequestHandler.cs) in order to:

- Deserialize function input to your POCO request model
- Validate request models
- Dispatch a function for additional logic
- Formulate a proper HTTP response

## How to use

- Create your own [RequestHandler](../RequestHandlerNETCore31/RequestHandler.cs) class, preferrably in a _Common_ place.
- In the endpoint that receives the HTTP Request you can use it directly, for it is a static class.

## Note

The in-process hosting model offers better performance over out-of-process, but on the other hand offers less control. Out-of-process hosting has the benefit of employing [middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0), which offers more fine-grained modular control over requests and responses. This is recommended from .NET 6 on.
