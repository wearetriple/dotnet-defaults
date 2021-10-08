using System;
using System.Linq;
using System.Text;

namespace ServiceTests
{
    internal class Service
    {
        private IGateway _gateway;

        public Service(IGateway gateway)
        {
            _gateway = gateway;
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
            catch(Exception ex)
            {
                Console.WriteLine($"Logging exception: {ex}");
                throw;
            }

            return dashboard;
        }

        private static string FormatHobbiesToString(string[]? hobbies)
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
            };
        }
    }
}
