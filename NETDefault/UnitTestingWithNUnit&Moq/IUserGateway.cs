namespace UnitTestingExample
{
    public interface IUserGateway
    {
        public int GetUserIdByUserName(string userName);
        public string[] GetHobbiesByUserId(int userId);
        public int? GetNotificationsByUserId(int? count);
    }
}
