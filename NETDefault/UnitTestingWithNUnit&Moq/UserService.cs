using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace UnitTestingExample
{
    public class UserService
    {
        private readonly IUserGateway _gateway;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserGateway gateway, ILogger<UserService> logger)
        {
            _gateway = gateway;
            _logger = logger;
        }

        public string GetDashboardByUserName(string userName)
        {
            var dashboard = string.Empty;

            try
            {
                var userId = _gateway.GetUserIdByUserName(userName);
                dashboard += userId;

                var hobbies = _gateway.GetHobbiesByUserId(userId);
                dashboard += FormatHobbiesToString(hobbies);

                var notificationCount = _gateway.GetNotificationsByUserId(userId);
                dashboard += FormatNotificationCountToString(notificationCount);
            }
            catch(NotFoundException ex)
            {
                _logger.LogError(ex, "User with username {username} could not be found", userName);
                throw;
            }

            return dashboard;
        }

        private static string FormatHobbiesToString(string[] hobbies)
        {
            if (hobbies is null)
            {
                return ", hobbies error";
            }

            if (hobbies.Length is 0)
            {
                return ", zero hobbies";
            }

            var sb = new StringBuilder();

            if (hobbies.Any())
            {
                foreach (var hobby in hobbies)
                {
                    sb.Append($", {hobby}");
                }
            }

            return sb.ToString();
        }

        private static string FormatNotificationCountToString(int? count)
        {
            switch (count)
            {
                case null:
                    return ", notification error";
                case 1:
                    return ", 1 notification";
                default:
                    return $"{count} notifications";
            }
        }
    }
}
