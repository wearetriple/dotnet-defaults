# Triple .NET Vocabulary

Keyword | Description
------------ | -------------
Accessor | A class tasked to return a specific value or object that is used at many places (like IHttpContextAccessor).
Gateway | An API Gateway serves as a central interface / project for all external communications for a specific endpoint.
Helper | A static object containing methods that help with simple, mundane tasks.
Provider | A class implementing a certain strategy / algorithm.
Repository | A class wrapping a data source that is owned by the application.
Resolver | A class tasked to return a specific value or object based on some input data. Since it is an object with its own lifetime, it has access to resources from DI and should be used to abstract away an implementation detail.
Service | The place for your business logic, naturally backed by unit tests.
