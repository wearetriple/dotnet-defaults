using System;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace UnitTestingExample
{
    internal class UserServiceTestsBase
    {
        protected UserService _subject;
        private Mock<IUserGateway> _gateway;
        protected Mock<ILogger<UserService>> _logger;

        protected const string KnownUsername = "known";
        protected const string UnknownUsername = "unknown";
        protected const int UserId = 2;

        [SetUp]
        public void Setup()
        {
            var autoMocker = new AutoMocker();

            _gateway = autoMocker.GetMock<IUserGateway>();

            _logger = autoMocker.GetMock<ILogger<UserService>>();

            _subject = autoMocker.CreateInstance<UserService>();
        }

        protected void SetupGetUserIdByUserName()
        {
            _gateway
                .Setup(x => x.GetUserIdByUserName(UnknownUsername))
                .Throws<NotFoundException>();

            _gateway
                    .Setup(x => x.GetUserIdByUserName(KnownUsername))
                    .Returns(UserId);
        }

        protected void SetupGetHobbiesByUserId(string[] hobbies)
        {
            _gateway
                .Setup(x => x.GetHobbiesByUserId(UserId))
                .Returns(hobbies);
        }

        protected void SetupGetNotificationsByUserId(int? count)
        {
            _gateway
                .Setup(x => x.GetNotificationsByUserId(UserId))
                .Returns(count);
        }
    }
}
