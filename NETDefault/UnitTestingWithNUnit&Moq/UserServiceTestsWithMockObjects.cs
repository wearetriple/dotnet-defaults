using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTestingExample;

internal class UserServiceTestsWithMockObjects
{       
    [Test]
    public void GetDashboard_WhenCustomerNameNotFound_ItShouldThrow()
    {
        // Arrange
        const string userName = "invalidUserName";
        var sut = CreateSubject(MockUserGateWay.Create(userName));
        
        // Assert
        Assert.Throws<NotFoundException>(() => sut.GetDashboardByUserName(userName));
    }

    [Test]
    public void GetDashboard_WhenCustomerNameFound_IdShouldBeReturned()
    {
        // Arrange
        const string userName = "validUserName";
        var sut = CreateSubject(MockUserGateWay.Create(userName, 1));

        // Act
        var result = sut.GetDashboardByUserName(userName);

        // Assert
        Assert.That(result.Contains('1'));
    }

    [Test]
    public void GetDashboard_WhenUserFound_ItShouldGetHobbies()
    {
        // Arrange
        const string hobby = "my-hobby";
        const string userName = "validUserName";
        var gateway = MockUserGateWay.Create(userName,1, [hobby]);
        var sut = CreateSubject(gateway);

        // Act
        var result = sut.GetDashboardByUserName(userName);

        // Assert
        Assert.That(result.Contains(hobby));
    }

    [TestCaseSource(typeof(HobbiesTestCases))]
    public void GetDashboard_WhenHobbiesFound_ItShouldBeReturned(string[] hobbies, string expectedHobbiesAsString)
    {
        // Arrange
        const string userName = "validUserName";
        var gateway = MockUserGateWay.Create(userName, 1, hobbies);
        var sut = CreateSubject(gateway);

        // Act
        var result = sut.GetDashboardByUserName(userName);

        // Assert
        Assert.That(result.Contains(expectedHobbiesAsString));
    }
    
    [Test]
    public void GetDashBoard_WhenNoHobbiesAreFound_ItShouldReturnError()
    {
        // Arrange
        const string userName = "validUserName";
        var gateway = MockUserGateWay.Create(userName, 1);
        var sut = CreateSubject(gateway);
        
        // Act
        var result = sut.GetDashboardByUserName(userName);

        // Assert
        Assert.That(result.Contains(", hobbies error"));
    }

    [TestCase(null, "notification error")]
    [TestCase(0, "0 notifications")]
    [TestCase(1, "1 notification")]
    public void GetDashboard_WhenNotificationsFound_ItShouldBeReturned(int? count, string expectedNotificationString)
    {
        // Arrange
        var gateway = MockUserGateWay.Create("validUserName", 1, notificationCount: count);
        var sut = CreateSubject(gateway);
        
        // Act
        var result = sut.GetDashboardByUserName("validUserName");

        // Assert
        Assert.That(result.Contains(expectedNotificationString));
    }
        
    [Test]
    public void GetDashboard_WhenExceptionIsThrown_ItShouldBeLogged()
    {
        // Arrange
        const string userName = "invalidUserName";

        var logger = MockLogger.Create("User with username invalidUserName could not be found");
        var gateWay = MockUserGateWay.Create(userName);
        var sut = CreateSubject(gateWay, logger);
            
        // Act
        Assume.That(() => sut.GetDashboardByUserName(userName), Throws.Exception);
            
        // Assert
        logger.Verify();
    }

    private static UserService CreateSubject(IMock<IUserGateway>? gateway = null, IMock<ILogger<UserService>>? logger = null)
    {
        return new UserService(gateway?.Object ?? Mock.Of<IUserGateway>(), logger?.Object ?? Mock.Of<ILogger<UserService>>());
    }
}
