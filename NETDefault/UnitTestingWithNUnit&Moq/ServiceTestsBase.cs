using Moq;
using Moq.AutoMock;
using NUnit.Framework;

namespace ServiceTests
{
    internal class ServiceTestsBase
    {
        protected Service _subject;
        protected Mock<IGateway> _gateway;

        protected string _knownUsername = "known";
        protected string _unknownUsername = "unknown";
        protected int _userId = 2;
        protected string[] _hobbies = new string[3] { "eat", "sleep", "repeat" };

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
                    .Returns(_userId);
        }

        protected void SetupGetHobbiesByUserId(int id)
        {
            _gateway
                .Setup(x => x.GetHobbiesByUserId(_userId))
                .Returns(_hobbies);
        }
    }
}
