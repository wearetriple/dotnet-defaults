using System;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace UnitTestingExample
{
    internal class UserServiceTests
    {
        private UserService _subject;
        private Mock<IUserGateway> _gateway;
        private Mock<ILogger<UserService>> _logger;

        [SetUp]
        public void Setup()
        {
            var autoMocker = new AutoMocker();

            _gateway = autoMocker.GetMock<IUserGateway>();

            _logger = autoMocker.GetMock<ILogger<UserService>>();

            _subject = autoMocker.CreateInstance<UserService>();
        }
        
        [Test]
        public void GetDashboard_WhenCustomerNameNotFound_ItShouldThrow()
        {
            // Arrange
            const string userName = "invalidUserName";
            _gateway
                .Setup(x => x.GetUserIdByUserName(userName))
                .Throws<NotFoundException>();
            
            // Assert
            Assert.Throws<NotFoundException>(() => _subject.GetDashboardByUserName(userName));
        }

        [Test]
        public void GetDashboard_WhenCustomerNameFound_IdShouldBeReturned()
        {
            // Arrange
            const string userName = "someUserName";
            _gateway
                .Setup(x => x.GetUserIdByUserName(userName))
                .Returns(1);   

            // Act
            var result = _subject.GetDashboardByUserName(userName);

            // Assert
            Assert.That(result.Contains('1'));
        }

        [Test]
        public void GetDashboard_WhenUserFound_ItShouldGetHobbies()
        {
            // Arrange
            const string hobby = "my-hobby";
            SetupGetHobbies([hobby]);

            // Act
            var result = _subject.GetDashboardByUserName("blaat");

            Assert.That(result.Contains(hobby));
        }

        [TestCaseSource(typeof(HobbiesTestCases))]
        public void GetDashboard_WhenHobbiesFound_ItShouldBeReturned(string[] hobbies, string expectedHobbiesAsString)
        {
            // Arrange
            SetupGetHobbies(hobbies);

            // Act
            var result = _subject.GetDashboardByUserName("KnownUsername");

            // Assert
            Assert.That(result.Contains(expectedHobbiesAsString));
        }

        [TestCase(null, "notification error")]
        [TestCase(0, "0 notifications")]
        [TestCase(1, "1 notification")]
        public void GetDashboard_WhenNotificationsFound_ItShouldBeReturned(int? count, string expectedNotificationString)
        {
            // Arrange
            _gateway
                .Setup(x => x.GetNotificationsByUserId(It.IsAny<int>()))
                .Returns(count);

            // Act
            var result = _subject.GetDashboardByUserName("KnownUsername");

            // Assert
            Assert.That(result.Contains(expectedNotificationString));
        }
        
        [Test]
        public void GetDashboard_WhenExceptionIsThrown_ItShouldBeLogged()
        {
            // Arrange
            const string userName = "invalidUserName";
            
            _gateway
                .Setup(x => x.GetUserIdByUserName(userName))
                .Throws<NotFoundException>();

            Assume.That(() => _subject.GetDashboardByUserName(userName), Throws.Exception);
            
            _logger.Verify(x => x.Log(
                    LogLevel.Error, 
                    It.IsAny<EventId>(), 
                    It.Is<It.IsAnyType>((v,t) => v.ToString() == "User with username invalidUserName could not be found"), 
                    It.IsAny<NotFoundException>(), 
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
        
        private void SetupGetHobbies(string[] hobbies)
        {
            _gateway
                .Setup(x => x.GetHobbiesByUserId(It.IsAny<int>()))
                .Returns(hobbies);
        }
    }
}
