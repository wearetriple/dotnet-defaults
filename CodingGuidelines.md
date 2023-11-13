# Coding Guidelines

## C# General

In order to improve team wide readability and lower onboarding time we agreed to stick to defined coding guidelines. These coding guidelines help us writing better and more coherent code, both on an individual level, as well as across individual developers since the style becomes more aligned.
Details can be found in the .editorconfig file, included in the root of this repository. The .editorconfig should be included in every solution. The team is open for changes through a pull request on this file.

The coding guidelines can be found [here](https://csharpcodingguidelines.com/).
Feel free to download the .PDF for later references.

## IDE

At Triple we are not tied to a single operating system, and so it happens that also among the .NET team there is cross-platform development. In addition to the free choice of IDE, there are quite a few flavors possible. However, in order to reduce onboarding time, we should strive to facilitate cross-platform development as best as possible. Be sure to add a README file with instructions for project initialization. If your OS/IDE is not described yet, you should add the instructions.

## Unit Testing

We thoroughly appreciate it when business logic is well covered by a test suite. This helps with extending functionality, refactoring existing code, and general readability.
Our current prefered unit testing framework is [NUnit](https://nunit.org/), but if a particular project is suited to a different one (like XUnit) feel free to do so. Do however document and motivate this choice in the readme.

When writing Tests, you must structure the test files in such a way that it mirrors the class under test. For example, when you are writing a test for 'Triple.Common.Services.TripleCoin.CoinService.cs', then the tests should be filed under 'Triple.Common.Tests.Services.TripleCoin.CoinServiceTests.cs'. This improves project navigation.

### Further reading resources

#### ASP.NET

* [ASP.NET Core best practices](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios)
* [General ASP.NET Core Guidance](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AspNetCoreGuidance.md)
* [Use IHttpClientFactory instead of HttpClient](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0)

#### Async

* [Async Guidance](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md)

#### General coding

* [Clean Code series](https://www.pearson.com/us/higher-education/series/Robert-C-Martin-Series/348084.html) by Robert C. Martin
  * Clean Code: A Handbook of Agile Software Craftmanship
  * The Clean Coder: A Code Conduct for Professional Programmers
  * Clean Architecture: A Craftman's Guide to Software Structure and Design
  * Clean Agile: Back to Basics
* [The Software Craftsman](https://www.pearson.com/us/higher-education/program/Mancuso-Software-Craftsman-The-Professionalism-Pragmatism-Pride/PGM96980.html): Professionalism, Pragmatism, Pride - Sandro Mancuso
* [Steve Smith on how to upgrade your C# game](https://ardalis.com/how-to-become-master-writing-c-code/)
