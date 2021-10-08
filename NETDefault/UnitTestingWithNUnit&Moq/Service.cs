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

        public object GetDashboardByUserName(string userName)
        {
            var dashboard = string.Empty;

            try
            {
                var userId = _gateway.GetUserIdByUserName(userName);
                dashboard += userId;

                var hobbies = _gateway.GetHobbiesByUserId(userId);
                dashboard += FormatHobbiesToString(hobbies);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Logging exception: {ex}");
                throw;
            }

            return dashboard;
        }

        private static string FormatHobbiesToString(string[] hobbies)
        {
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
    }
}
