using System;

namespace UnitTestingExample
{
    public interface IGateway
    {
        public int GetUserIdByUserName(string userName);
        public string[] GetHobbiesByUserId(int userId);
        public int? GetNotificationsByUserId(int? count);
    }
}
