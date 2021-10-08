# Unit Testing with NUnit and Moq

In this directory you will find a small project where we demonstrate the usage of NUnit and Moq to bring our code under test. Here, a _Service_ is being the subject under test, which depends on a _Gateway_ for completing the _GetDashboard_ call. Since we are unit testing, we do *not* want to include the external calls with the Gateway because that is not what we want to test and it would take far too long to complete. It is therefore that we create a fake instance of this gateway -  a.k.a. a mock, moq, substitute - for which we use [Moq](https://github.com/moq). Our _Service_ is the actual concrete class we want to test, which we do with [NUnit](https://github.com/moq).

## Points of interest

- ### ServiceTestsBase.cs

The base class of the subject contains the ceremony to support the tests. Here we start with the _Setup()_ method, which instantiates your concrete class which we will test, and also creates mocks for its dependencies.

While writing tests you will find you need to make a call to your dependency. Since it's a fake instance, you cannot make the actual call (which is good!) and so you need to tell it what to do instead. This is what happens in the _Setup{DoSomething}()_ methods. For example: when we use the gateway to get a userId with the name 'unknown', we will get a _NotFoundException_ back from it.

Above in the file you will find protected fields that we use in the tests. These you will create as you write the tests.

Lastly some interesting *attributes*:
- The *[[Test](https://docs.nunit.org/articles/nunit/writing-tests/attributes/test.html)]* attribute is used to mark simple (non-parameterized) tests.
- The *[[TestCase](https://docs.nunit.org/articles/nunit/writing-tests/attributes/testcase.html)]* attribute serves to mark a method as a test with parameters and providing inline data to be used in the test. For each testcase, the test will be run, as can be seen in the Test Explorer of Visual Studio.
- The *[[TestCaseSource](https://docs.nunit.org/articles/nunit/writing-tests/attributes/testcasesource.html)]* attribute also marks a method as a test, and is used on parameterized test methods to identify the source from which the required arguments will be provided. Source data is kept separate from the test itself, and may be used in multiple tests.

- ### ServiceTests.cs

The test class *only* contains the actual tests without any ceremony. This file will be read most often to understand the class we test, so make sure it is clean, easy to read, and consistent. Note the following:
- Test names should contain the _When...ItShould()_ pattern
- We use the _Arrange_, _Act_, _Assert_ pattern to structure our tests and keep them consistent.
- _Assert_ is part of the NUnit Framework, which we use to validate our subject.
- _.Verify()_ is part of the Moq Framework, which we use to validate the calls to dependencies.

- ### Service.cs

The actual concrete class we are testing. Actually not very interesting here, other than that it may be used in a debug session to understand what happens where.

## Takeaway

This structure can be used to initialize your own test class and start writing tests to cover the acceptance criteria. Be sure to fully cover the class under test, for it reduces mistakes, and makes refactoring later easier.

Further reading:
[Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
[Test Driven Development](https://www.agilealliance.org/glossary/tdd/)