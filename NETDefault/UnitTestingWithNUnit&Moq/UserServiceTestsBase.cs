using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace UnitTestingExample
{
    internal class UserServiceTestsBase
    {
        protected UserService _subject;
        protected Mock<IUserGateway> _gateway;

        protected string _knownUsername = "known";
        protected string _unknownUsername = "unknown";
        protected int _userId = 2;

        [SetUp]
        public void Setup()
        {
            var autoMocker = new AutoMocker();

            _gateway = autoMocker.GetMock<IUserGateway>();

            _subject = autoMocker.CreateInstance<UserService>();
        }

        protected void SetupGetUserIdByUserName(string username)
        {
            _gateway
                .Setup(x => x.GetUserIdByUserName("unknown"))
                .Throws<NotFoundException>();

            _gateway
                    .Setup(x => x.GetUserIdByUserName("known"))
                    .Returns(_userId);
        }

        protected void SetupGetHobbiesByUserId(string[] hobbies)
        {
            _gateway
                .Setup(x => x.GetHobbiesByUserId(_userId))
                .Returns(hobbies);
        }

        protected void SetupGetNotificationsByUserId(int? count)
        {
            _gateway
                .Setup(x => x.GetNotificationsByUserId(_userId))
                .Returns(count);
        }
    }
}
