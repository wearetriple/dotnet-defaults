using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTestingExample
{
    internal class UserServiceTests : UserServiceTestsBase
    {
        [Test]
        public void GetDashboard_WhenCustomerNameNotFound_ItShouldThrow()
        {
            // Arrange
            SetupGetUserIdByUserName();

            // Assert
            Assert.Throws<NotFoundException>(() => _subject.GetDashboardByUserName(UnknownUsername));
        }

        [Test]
        public void GetDashboard_WhenCustomerNameFound_ItShouldBeReturned()
        {
            // Arrange
            SetupGetUserIdByUserName();

            var expected = UserId.ToString();

            // Act
            var result = _subject.GetDashboardByUserName(KnownUsername);

            // Assert
            Assert.That(result.Contains(expected));
        }

        [Test]
        public void GetDashboard_WhenUserFound_ItShouldGetHobbies()
        {
            // Arrange
            SetupGetUserIdByUserName();
            SetupGetHobbiesByUserId(["my-hobby"]);

            // Act
            var result = _subject.GetDashboardByUserName(KnownUsername);

            Console.WriteLine(result);
            
            Assert.That(result.Contains("my-hobby"));
        }

        [TestCaseSource(typeof(HobbiesTestCases))]
        public void GetDashboard_WhenHobbiesFound_ItShouldBeReturned(string[] hobbies, string expectedHobbiesAsString)
        {
            // Arrange
            SetupGetUserIdByUserName();
            SetupGetHobbiesByUserId(hobbies);

            // Act
            var result = _subject.GetDashboardByUserName(KnownUsername);

            // Assert
            Assert.That(result.Contains(expectedHobbiesAsString));
        }

        [TestCase(null, "notification error")]
        [TestCase(0, "0 notifications")]
        [TestCase(1, "1 notification")]
        public void GetDashboard_WhenNotificationsFound_ItShouldBeReturned(int? count, string expectedNotificationString)
        {
            // Arrange
            SetupGetUserIdByUserName();
            SetupGetNotificationsByUserId(count);

            // Act
            var result = _subject.GetDashboardByUserName(KnownUsername);

            // Assert
            Assert.That(result.Contains(expectedNotificationString));
        }
        
        [Test]
        public void GetDashboard_WhenExceptionIsThrown_ItShouldBeLogged()
        {
            // Arrange
            SetupGetUserIdByUserName();

            // Assert
            Assert.Throws<NotFoundException>(() => _subject.GetDashboardByUserName(UnknownUsername));
            _logger.Verify(x => x.Log(
                    LogLevel.Error, 
                    It.IsAny<EventId>(), 
                    It.Is<It.IsAnyType>((v,t) => v.ToString() == $"User with username {UnknownUsername} could not be found"), 
                    It.IsAny<NotFoundException>(), 
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
