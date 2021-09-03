# Layered project

Keeping the structure of each .NET project somewhat similar helps developers to quickly find existing code and directly know where to add new code.

## Solution layout

A .NET solution must follow the following guidelines:

- The name of the solution is `{Customer}.{Project}.sln`.
- Put simple, solution-wide used objects like enums, exceptions and simple classes in Common: `{Customer}.{Project}.Common.csproj`.
  - The common project does not depend on any other project.
- Put everything that has to do with an external API of which the project is client of in a Gateway project: `{Customer}.{Project}.Gateways.{External Api}.csproj`.
- If the project exposes an API using ASP.NET Core Web Api, create a project for it called `{Customer}.{Project}.Api.csproj`.
- If the project uses Azure Functions, create a project for those functions called `{Customer}.{Project}.Functions.csproj`.
- Put all general business logic in `{Customer}.{Project}.Business.csproj` or `{Customer}.{Project}.Services.csproj`.
  - If the project is probably going to contain a lot of business logic, split the business logic up into parts and give each part a separate project: `{Customer}.{Project}.Users.csproj` or `{Customer}.{Project}.Services.Payment.csproj`. Common business stuff can still be put in the general business logic project.
- If the project uses other (web) applications that do more than just an API, put them in a project that bests describes their functionality, like `{Customer}.{Project}.Cms.csproj`.
- Put everything that has to do with the persistence layer in a separate project and use a name that describes what it persists `{Customer}.{Project}.Data.csproj`.
  - If the persistence layer is not big it can all be put in the same project.
  - If parts of the persistence layer become big, they can be split of in to separate projects like `{Customer}.{Project}.Data.User.csproj`.
- Unit tests only test a specific project, and should always be called `{ProjectToTest}.Tests.csproj`.
  - Test projects should try to mirror the folder structure of the project it tests. This makes finding tests more easy. If the amount of tests within a folder becomes large, the folder can be split up by adding multiple folders to it.
- Integration tests should be named like unit tests `{ProjectToTest}.Tests.csproj`.
  - `{ProjectToTest}` must be the project it uses as entry point for the integration tests.

### Singular or plural names

Singular and plural names for projects can both be used. Project that contain gateways or applications usually are singular (`CMS`, `AdminDashboard`, `Gateway.Contentful`) and project that are libraries are usually plural (`Repositories`, `Services`). Plural names for projects will prevent issues concerning namespace confusion; you cannot create the object `User` if the project name is `xx.User`. Keeping the names of projects plural will easily deal with those issues.

### Additional folders

Solution folders can be used to group specific projects together. If for example a gateway uses multiple project, group these projects in a solution folder describing the gateway. 

### Shorter project names

When grouping projects in solution folders, the names of these projects can be shortened to simplify their naming. Instead of `{Customer}.{Project}.Gateway.{External API}.{External API project}.csproj` the name `{External API}.{External API project}.csproj` can be used.

### Project folders and namespaces

Folders within projects should group similar objects (to a certain degree), and should be named what seems logical. Group converters in `Converters` and controllers in `Controllers`. Add more layers of folders when the folder becomes too big, like adding `Request` and `Response` folders in `Models` to separate request and response models. Group interfaces in `Abstractions`. Group extension (methods) in `Extensions`.

The namespace of a file should always be comprised of its compound folder name, like `Contoso.CalendarHub.Api.Models.Request`.

### Example

- Contoso.CalendarHub.sln
  - Contoso.CalendarHub.Api.csproj
    - `Controllers`
    - `JsonConverters`
    - `Models`
        - `Request`
        - `Response`
  - Contoso.CalendarHub.Api.Tests.csproj
    - `LoginControllerTests`
    - `UserControllerTests`
  - Contoso.CalendarHub.Business.csproj
    - `Abstractions`
        - `Calendars`
        - `Users`
    - `Services`
        - `Calendar`
        - `User`
  - Contoso.CalendarHub.Business.Tests.csproj
    - `UserServiceTests`
    - `CalendarServiceTest`
  - Contoso.CalendarHub.Common.csproj
    - `Attributes`
    - `Extensions`
    - `Enums`
    - `Models`
    - `Resolvers`
  - Contoso.CalendarHub.Cms.csproj
  - Contoso.CalendarHub.Data.Users.csproj
    - `Entities`
    - `Repositories`
  - Contoso.CalendarHub.Data.Invitations.csproj
    - `Entities`
  - Contoso.CalendarHub.Functions.csproj
    - `Attributes`
    - `Constants`
    - `Extensions`
  - Contoso.CalendarHub.Gateways.Adyen.csproj
    - `Models`
  - Contoso.CalendarHub.Gateways.GoogleCalendar.csproj
    - `Models`
    - `Helpers`
  - Contoso.CalendarHub.Gateways.GoogleCalendar.Tests.csproj
  - Contoso.CalendarHub.Gateways.OutlookCalendar.csproj
    - `Models`
  - Contoso.CalendarHub.Gateways.OutlookCalendar.Tests.csproj

## Inter-project data exchange and isolation

When dividing a solution into several distinct projects it is important to maintain a level of isolation between these projects. Keeping these projects reasonably isolated helps to control the impact of changes; a change is one project should not force a lot of changes in other projects. Isolation will prevent developers from having to notify users of the API of your solution that some outbound data changes due to modification in a database column. Keep in mind to not overdo this, implementation details will always tend to spread a little bit to other parts of the solution.

### Isolate models

Models, or any object containing data, are usually passed around between various parts of the application. Each project should use its own type of object for its own use.

- An API always has its own Request- and ResponseModels. These models describe the exact shape of requests it expects and responses it generates.
  - `UpdateUserRequestModel`, `UserDetailsResponseModel`
- The Common projects contains domain objects, commands, responses, and other types which facilitate communication between parts of the application.
  - `AddUserCommand`, `UserDetails`
- A gateway always has its own models. These models describe what data must be send to the external system and what data comes out of the external system. The gateway exposes an interface for use by the solution which accepts and returns domain objects.
  - `CreatePaymentRequestModel`, `PaymentDetailsResponseModel`
- The persistence layer uses its own entities to store data in a database or other storage, and exposes an interface which accepts and returns domain objects.
  - `UserEntity`, `RoleEntity`
- Business logic uses domain objects for its logic.

A payment request is made to the projects API controller, receiving an `AddPaymentRequestModel`. The API validates whether the request is valid (using data annotations on the model, or FluentValidation for that model). If the request is valid, the API controller transform the model to `AddPaymentCommand` and passes it to `IPaymentService`. The implementer of this interface can opt to validate `AddPaymentCommand`, but can also assume its validity. The service talks to `IPaymentGateway` which accepts the `AddPaymentCommand`, converts it to `AdyenCreatePaymentModel` and sends it to the payment provider. The payment provider returns `PaymentModel`, which is converted to `PaymentDetails` which is returned by the `IPaymentGateway`. The payment service uses that dto to talk to the `IPaymentRepository` to save the payment. That repository converts the `PaymentDetails` to `PaymentEntity` which is persisted in the database.

In the example above, several boundaries have been drawn. Over these lines the Dependency Inversion principle is applies, isolating components. A change in the shape of `AddPaymentRequestModel` will not automatically force other services and interfaces to change. This makes modifying the application more easy and make your system more loosely coupled. It does require more specific models - one pair for each component. A setup where these mappings are automatically generated is advisable and the use of things like AutoMapper, ExpressionMapper or GeneratedMapper is encouraged.

### Isolate with interfaces

To shield other parts of the solution from implementation details of a project, the project should expose its usable API surface as interfaces. These interfaces only describe the shape of the API, without bothering the user of that API with any implementation detail. The naming of these interfaces should be generic, even if there is only one implementor. Use for example `IPaymentGateway` which is implemented by `AdyenPaymentGateway`, because, if the payment provider is changed further down the road, having a `IAdyenPaymentGateway` be implemented by `MollyPaymentGateway` is awkward.

To fully utilize this isolation the use of [Dependency Injection](DependencyInjection.md) is required. Also, to help the developer what features can be used and what are details, classes that are only used within a project which have no use outside that project must be configured as `internal`. This will keep all the internals `internal`. Using the `[InternalsVisibleTo]` attribute other projects can be given access to those internals, which is required in cases like [Unit Tests](UnitTests.md). Mocking interfaces is also easy, which greatly helps with [Unit Testing](UnitTests.md).


