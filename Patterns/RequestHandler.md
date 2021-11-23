# Request Handlers

Request handlers are used wherever a client sends a request to our endpoint, and we need to do the following things:
- Verify input query parameters or body, and map to POCO.
- Dispatch the POCO where the logic will be conducted.
- Receive output and form a proper HTTP Response.

## How to use

- Create your own _RequestHandler_ class, preferrably in a _Common_ place.
- In the endpoint that receives the HTTP Request you can use it directly, for it is a static class.

## .NET Core 3.1

// Describe difficulties we experience at the moment - and what we'd solve in the 6.0 version

## .NET 6.0

To be implemented..
