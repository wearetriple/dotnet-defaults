using Moq;

namespace UnitTestingExample;

internal sealed class MockUserGateway : Mock<IUserGateway>
{
    private MockUserGateway(string userName, int? userId = null, string[]? hobbies = null, int? notificationCount = null)
    {
        if (userId == null)
        {
            Setup(x => x.GetUserIdByUserName(userName))
                .Throws<NotFoundException>();
        }
        else
        {
            Setup(x => x.GetUserIdByUserName(userName))
                .Returns(userId.Value);
        
        }

        Setup(x => x.GetHobbiesByUserId(userId ?? -1))
            .Returns(hobbies!);

        Setup(x => x.GetNotificationsByUserId(userId ?? -1))
            .Returns(notificationCount);
    }
    
    public static MockUserGateway Create(string userName, int? userId = null, string[]? hobbies = null, int? notificationCount = null) =>
        new (userName, userId, hobbies, notificationCount);
}
