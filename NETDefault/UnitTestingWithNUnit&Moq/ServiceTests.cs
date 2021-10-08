using Moq;
using NUnit.Framework;

namespace ServiceTests
{
    internal class ServiceTests : ServiceTestsBase
    {
        [Test]
        public void GetDashboard_WhenCustomerNameNotFound_ItShouldThrow()
        {
            // Arrange
            SetupGetUserIdByUserName(unknownUsername);

            // Assert
            Assert.Throws<NotFoundException>(() => _subject.GetDashboardByUserName(unknownUsername));

            _gateway.Verify(x => x.GetUserIdByUserName(unknownUsername), Times.Once());
            _gateway.VerifyNoOtherCalls();
        }

        [Test]
        public void GetDashboard_WhenCustomerNameFound_ItShouldBeReturned()
        {
            // Arrange
            SetupGetUserIdByUserName(knownUsername);

            var expected = userId.ToString();

            // Act
            var result = _subject.GetDashboardByUserName(knownUsername);

            // Assert
            Assert.AreEqual(expected, result);

            _gateway.Verify(x => x.GetUserIdByUserName(knownUsername), Times.Once());
            _gateway.Verify(x => x.GetHobbiesByUserId(userId), Times.Once());
        }

        [Test]
        public void GetDashboard_WhenUserFound_ItShouldGetHobbies()
        {
            // Arrange
            SetupGetUserIdByUserName(knownUsername);
            SetupGetHobbiesByUserId(userId);

            // Act
            _ = _subject.GetDashboardByUserName(knownUsername);

            // Assert
            _gateway.Verify(x => x.GetUserIdByUserName(knownUsername), Times.Once());
            _gateway.Verify(x => x.GetHobbiesByUserId(userId), Times.Once());
            _gateway.VerifyNoOtherCalls();
        }

        [Test]
        public void GetDashboard_WhenHobbiesFound_ItShouldBeReturned()
        {
            // Arrange
            SetupGetUserIdByUserName(knownUsername);
            SetupGetHobbiesByUserId(userId);

            var expected = userId.ToString() + ", " + hobbies[0] + ", " + hobbies[1] + ", " + hobbies[2];

            // Act
            var result = _subject.GetDashboardByUserName(knownUsername);

            // Assert
            Assert.AreEqual(expected, result);

            _gateway.Verify(x => x.GetUserIdByUserName(knownUsername), Times.Once());
            _gateway.Verify(x => x.GetHobbiesByUserId(userId), Times.Once());
        }
    }
}