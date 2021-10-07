using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace ServiceTests
{
    internal class ServiceTestsBase
    {
        protected Service _subject;
        protected Mock<IGateway> _gateway;

        protected string knownUsername = "known";
        protected string unknownUsername = "unknown";
        protected int userId = 2;
        protected string[] hobbies = new string[3] { "eat", "sleep", "repeat" };

        [SetUp]
        public void Setup()
        {
            var autoMocker = new AutoMocker();

            _gateway = autoMocker.GetMock<IGateway>();

            _subject = autoMocker.CreateInstance<Service>();
        }

        protected void SetupGetUserIdByUserName(string username)
        {
            _gateway
                .Setup(x => x.GetUserIdByUserName("unknown"))
                .Throws<NotFoundException>();

            _gateway
                    .Setup(x => x.GetUserIdByUserName("known"))
                    .Returns(userId);
        }

        protected void SetupGetHobbiesByUserId(int id)
        {
            _gateway
                .Setup(x => x.GetHobbiesByUserId(userId))
                .Returns(hobbies);
        }
    }
}