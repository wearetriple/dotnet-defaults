# Coding Guidelines

## C# General

In order to improve team wide readablity and lower onboarding time we agreed to stick to defined coding guidelines. These coding guidelines help us writing better and more coherent code, both on an individual level, as well as across individual developers since the style becomes more aligned.

The coding guidelines can be found [here](https://csharpcodingguidelines.com/).
Feel free to download the .PDF for later references.

## IDE

At Triple we are not tied to a single operating system, and so it happens that also among the .NET team there is cross-platform development. In addition to the free choice of IDE, there are quite a few flavors possible. However, in order to reduce onboarding time, we should strive to facilitate cross-platform development as best as possible. Be sure to add a README file with instructions for project initialization. If your OS/IDE is not described yet, you should add the instructions.

## Testing

We thorougly appreciate it when business logic is well covered by a test suite. This helps with extending functionality, refactoring existing code, and general readability.
In order to promote productivity, we should use the [NUnit](https://nunit.org/) testing framework, in combination with the mocking framework [Moq](https://github.com/Moq) by default.

### Further reading resources

* [Clean Code series](https://www.pearson.com/us/higher-education/series/Robert-C-Martin-Series/348084.html) by Robert C. Martin
  * Clean Code: A Handbook of Agile Software Craftmanship
  * The Clean Coder: A Code Conduct for Professional Programmers
  * Clean Architecture: A Craftman's Guide to Software Structure and Design
  * Clean Agile: Back to Basics
* [The Software Craftsman](https://www.pearson.com/us/higher-education/program/Mancuso-Software-Craftsman-The-Professionalism-Pragmatism-Pride/PGM96980.html): Professionalism, Pragmatism, Pride - Sandro Mancuso
