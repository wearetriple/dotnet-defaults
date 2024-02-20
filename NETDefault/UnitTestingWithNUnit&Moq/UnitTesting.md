# Unit Testing with NUnit and Moq

In this directory you will find a small project where we demonstrate the usage of [NUnit](https://docs.nunit.org/articles/nunit/intro.html), [Moq](https://github.com/devlooped/moq) and [AutoMocker](https://github.com/moq/Moq.AutoMocker) to bring our code under test. 
Here, a _Service_ is being the subject under test, which depends on a _Gateway_ and an _ILogger_ for completing the _GetDashboard_ call. \
Since we are unit testing, we do *not* want to include the external calls with the Gateway or the _ILogger_ because that is not what we want to 
test and it would take far too long to complete. It is therefore that we create a fake instance of the gateway and the logger for which we use Moq.
Most of the time Moq is used to create Mocks, but it can also be used to create Stubs. 
For more information about the difference between Mocks and Stubs, see [here](https://martinfowler.com/articles/mocksArentStubs.html). \
Our _Service_ is the actual concrete class we want to test, which we do with NUnit.

## Points of interest

- ### ServiceTestsBase.cs

We start with the _Setup()_ method, which instantiates the concrete class which we will test, and also creates mocks for its dependencies.

While writing tests you will find you need to make a call to your dependency. 
Since it's a fake instance, you cannot make the actual call (which is good!) and so you need to tell it what to do instead. 
This is what happens in the _Arrange_ part of the test methods. 

For example: when we want to test that a _NotFoundException_ is thrown when we request the id of an unknown user, we tell the gateway to return the _NotFoundException_ when a specified userName is passed to a specific method call.
This way we can test that the service handles the _NotFoundException_ correctly.

Lastly some interesting *attributes*:
- The *[[Test](https://docs.nunit.org/articles/nunit/writing-tests/attributes/test.html)]* attribute is used to mark simple (non-parameterized) tests.
- The *[[TestCase](https://docs.nunit.org/articles/nunit/writing-tests/attributes/testcase.html)]* attribute serves to mark a method as a test with parameters and providing inline data to be used in the test. For each testcase, the test will be run, as can be seen in the Test Explorer of Visual Studio.
- The *[[TestCaseSource](https://docs.nunit.org/articles/nunit/writing-tests/attributes/testcasesource.html)]* attribute also marks a method as a test, and is used on parameterized test methods to identify the source from which the required arguments will be provided. Source data is kept separate from the test itself, and may be used in multiple tests.

- ### ServiceTests.cs

This file will be read most often to understand the class we test, so make sure it is clean, easy to read, and consistent. Note the following:
- Test names should contain the _When...ItShould()_ pattern
- We use the _Arrange_, _Act_, _Assert_ pattern to structure our tests and keep them consistent.
- _Assert_ is part of the NUnit Framework, which we use to validate our subject.
- _Assume_ is also part of the NUnit Framework. We use this to set up assumptions for our tests. If the assumption is not met, the test will be inconclusive. This is useful when we want to test a specific scenario, but only if certain conditions are met.
- _Verify_ is part of Moq and can be used on mocks to verify that a method was called with certain parameters. In our example, we use it to verify that the logger was called with a certain message. This is useful in scenarios where we want to test known side effect of our behavior under test. 

- ### Service.cs

The actual concrete class we are testing. Actually not very interesting here, other than that it may be used in a debug session to understand what happens where.

## Takeaway

This structure can be used to initialize your own test class and start writing tests to cover the acceptance criteria. Be sure to fully cover the class under test, for it reduces mistakes, and makes refactoring later easier.

Keep in mind that the readability of the tests is more important than trying to avoid repeating code. Write small helper methods, but try to avoid writing logic in tests. Keep the test as simple as possible and create more tests, instead of more complicated tests.

Further reading:
[Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
[Test Driven Development](https://www.agilealliance.org/glossary/tdd/)